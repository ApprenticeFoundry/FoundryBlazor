
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Canvas;


namespace FoundryBlazor.Shape;

public interface IPanZoomService
{
    void Reset();
    double ZoomWheel(double delta);
    double Zoom();
    double LastZoom();
    double SetZoom(double zoom);

    Point Pan();
    Point PanBy(int dx, int dy);
    Point SetPan(int x, int y);

    Point Movement();
    Rectangle HitRectStart(CanvasMouseArgs args);
    Rectangle HitRectTopLeft(CanvasMouseArgs args, Rectangle rect);
    Rectangle HitRectContinue(CanvasMouseArgs args, Rectangle rect);
    Task<PanZoomService> TranslateAndScale(Canvas2DContext ctx, FoGlyph2D page);
    Rectangle TransformRect(Rectangle rect);
    Rectangle AntiScaleRect(Rectangle rect);
    Rectangle Normalize(Rectangle rect);
    public void Deconstruct(out double zoom, out int panx, out int pany);

    public void SetOnComplete(Action action);

}

public class PanZoomService : IPanZoomService
{
    private double _zoom = 1.0;
    private double _lastZoom = 1.0;
    private Point _pan = new(0, 0);
    private Point _Delta = new(0, 0);
    private Point _LastLocation = new(0, 0);

    protected Matrix2D? _matrix;
    protected Matrix2D? _invMatrix;

    private Action? OnComplete;

    public PanZoomService()
    {
    }

    public void SetOnComplete(Action action)
    {
        OnComplete = action;
    }

    public void Reset()
    {
        _lastZoom = _zoom;
        _zoom = 1.0;
        _pan = new(0, 0);
        Smash();
        OnComplete?.Invoke();
    }

    public Rectangle Normalize(Rectangle dragArea)
    {
        var left = dragArea.Left;
        var right = dragArea.Right;
        if ( left > right) {
            left = dragArea.Right;
            right = dragArea.Left;
        }

        var top = dragArea.Top;
        var bottom = dragArea.Bottom;
        if ( top > bottom) {
            top = dragArea.Bottom;
            bottom = dragArea.Top;
        }
        return new Rectangle(left, top, right - left, bottom - top);      
    }
    private Point ComputeDelta(int x, int y)
    {
        _Delta = new Point(x - _LastLocation.X, y - _LastLocation.Y);
        _LastLocation = new(x,y);
        return _Delta;
    }

    public virtual void Smash()
    {
        this._matrix = null;
        this._invMatrix = null;
    }

    public virtual Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            _matrix = new Matrix2D();
            _matrix.AppendTransform(_pan.X, _pan.Y, _zoom, _zoom, 0.0, 0.0, 0.0, 0.0, 0.0);
            FoGlyph2D.ResetHitTesting = true;
            //"PanZoomService GetMatrix recalculate".WriteLine(ConsoleColor.Yellow);
        }
        return _matrix;
    }

    public Matrix2D GetInvMatrix()
    {
        _invMatrix ??= this.GetMatrix().InvertCopy();
        return _invMatrix;
    }

    //for starting fence select
    public Rectangle HitRectStart(CanvasMouseArgs args)
    {
        //$"{_pan.X} {_pan.Y}".WriteLine(ConsoleColor.Blue);
        var pt = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);

        var hit = new Rectangle(pt.X, pt.Y, 1, 1);
        ComputeDelta(hit.Left, hit.Top);
        return hit;
    }

    // for doing shape select
    public Rectangle HitRectContinue(CanvasMouseArgs args, Rectangle rect)
    {
        var pt = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);
        rect.Width  = pt.X - rect.X;
        rect.Height = pt.Y - rect.Y;
        return rect;
    }

    //for resizing and image
    public Rectangle HitRectTopLeft(CanvasMouseArgs args, Rectangle rect)
    {
        var pt = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);

        var x1 = rect.Left;
        var y1 = rect.Top;
        var x2 = pt.X;
        var y2 = pt.Y;
        var hit = new Rectangle(x1,y1, x2-x1, y2-y1);
        _LastLocation = new Point(0, 0);
        ComputeDelta(hit.Left, hit.Top);
        return hit;
    }

    public Rectangle TransformRect(Rectangle rect)
    {
        var matrix = GetMatrix();
        var pt1 = matrix.TransformPoint(rect.Left, rect.Top);
        var pt2 = matrix.TransformPoint(rect.Right, rect.Bottom);

        var newRect = new Rectangle(pt1.X, pt1.Y, pt2.X-pt1.X, pt2.Y-pt1.Y);

        return newRect;
    }

    public Rectangle AntiScaleRect(Rectangle rect)
    {
               //anti scale this window
        var x = (rect.X-_pan.X) / _zoom;
        var y = (rect.Y-_pan.Y) / _zoom;
        var width = rect.Width / _zoom;
        var height = rect.Height / _zoom;  

        var newRect = new Rectangle((int)x, (int)y, (int)width, (int)height);
        //$"AntiScaleRect {x} {y} {width} {height}".WriteLine(ConsoleColor.Yellow);

        return newRect;
    }

    public double ZoomWheel(double delta)
    {
        _lastZoom = _zoom;
        _zoom *= delta < 0 ? 1.1 : 0.9;
        Smash();
        OnComplete?.Invoke();
        return _zoom;
    }

    public Point Movement()
    {
        return _Delta;

    }

    public void Deconstruct(out double zoom, out int panx, out int pany)
    {
        zoom = _zoom;
        panx = _pan.X;
        pany = _pan.Y;
    }

    public async Task<PanZoomService> TranslateAndScale(Canvas2DContext ctx, FoGlyph2D shape)
    {

        // var loc = shape.PinLocation();
        // var cx = loc.X;
        // var cy = loc.Y;

        var mtx = this.GetMatrix();
        //you must use Transform so the context can acumlate the positions
        await ctx.TransformAsync(mtx.a, mtx.b, mtx.c, mtx.d, mtx.tx, mtx.ty);

        // await ctx.TranslateAsync(cx, cy);
        // await ctx.ScaleAsync(_zoom, _zoom);
        // await ctx.TranslateAsync(_pan.X - cx, _pan.Y - cy);

        return this;
    }



    public double SetZoom(double zoom)
    {
        _lastZoom = _zoom;
        _zoom = zoom;
         Smash();
        OnComplete?.Invoke();
        return _zoom;
    }

    public double Zoom()
    {
        return _zoom;
    }
    public double LastZoom()
    {
        return _lastZoom;
    }
    public Point Pan()
    {
        return _pan;
    }

    public Point PanBy(int dx, int dy)
    {
        _pan.X += dx;
        _pan.Y += dy;
         Smash();
        OnComplete?.Invoke();
        return _pan;
    }


    public Point SetPan(int x, int y)
    {
        _pan = new(x, y);
         Smash();
        OnComplete?.Invoke();
        return _pan;
    }
}