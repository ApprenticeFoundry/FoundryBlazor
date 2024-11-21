
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;


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

    Matrix2D GetMatrix();
    PanZoomService AfterMatrixSmash(Action<PanZoomState> action);
    PanZoomService AfterMatrixRefresh(Action<PanZoomState> action);

    Point MouseDeltaMovement();
    Rectangle HitLocation(CanvasMouseArgs args);
    Rectangle HitRectStart(CanvasMouseArgs args);
    Rectangle HitRectTopLeft(CanvasMouseArgs args, Rectangle rect);
    Rectangle HitRectContinue(CanvasMouseArgs args, Rectangle rect);
    Task<PanZoomService> TranslateAndScale(Canvas2DContext ctx, FoGlyph2D page);
    PanZoomService TranslateAndScale(FoGlyph2D page);
    Rectangle TransformRect(Rectangle rect);
    Point[] TransformPoint(Point[] points);
    Rectangle AntiScaleRect(Rectangle rect);
    Rectangle Normalize(Rectangle rect);
    public void Deconstruct(out double zoom, out int panx, out int pany);

    public void SetOnEventComplete(Action action);

    public PanZoomState ReadFromPage(FoPage2D page);
    public PanZoomState WriteToPage(FoPage2D page);

    public bool IsFenceSelecting();
    public bool SetFenceSelecting(bool value);

}

public class PanZoomState
{
    public double Zoom = 1.0;
    public double LastZoom = 1.0;
    public Point Pan = new(0, 0);
    public Point Delta = new(0, 0);
    public Point LastLocation = new(0, 0);

    public void SetState(PanZoomState state)
    {
        Zoom = state.Zoom;
        LastZoom = state.LastZoom;
        Pan = state.Pan;
        Delta = state.Delta;
        LastLocation = state.LastLocation;
    }
    public void Reset()
    {
        LastZoom = Zoom;
        Zoom = 1.0;
        Pan = new(0, 0);
    }
    public Point ComputeDelta(int x, int y)
    {
        Delta = new Point(x - LastLocation.X, y - LastLocation.Y);
        LastLocation = new(x, y);
        return Delta;
    }
}

public class PanZoomService : IPanZoomService
{
    private readonly PanZoomState State = new();
    private bool isFenceSelecting = false;

    protected Matrix2D? _matrix;
    protected Matrix2D? _invMatrix;

    private Action? OnEventComplete;

    private Action<PanZoomState>? OnMatrixRefresh;
    private Action<PanZoomState>? OnMatrixSmash;

    public PanZoomService()
    {
    }

    public bool IsFenceSelecting()
    {
        return isFenceSelecting;
    }
    public bool SetFenceSelecting(bool value)
    {
        isFenceSelecting = value;
        return isFenceSelecting;
    }

    public PanZoomService AfterMatrixRefresh(Action<PanZoomState> action)
    {
        OnMatrixRefresh = action;
        return this;
    }

    public PanZoomService AfterMatrixSmash(Action<PanZoomState> action)
    {
        OnMatrixSmash = action;
        return this;
    }

    public void SetOnEventComplete(Action action)
    {
        OnEventComplete = action;
    }

    public void Reset()
    {
        State.Reset();
        Smash(true);
        OnEventComplete?.Invoke();
    }

    public PanZoomState ReadFromPage(FoPage2D page)
    {
        State.SetState(page.PanZoom);
        Smash(true);
        OnEventComplete?.Invoke();
        return State;
    }
    public PanZoomState WriteToPage(FoPage2D page)
    {
        page.PanZoom.SetState(State);
        return State;
    }

    public Rectangle Normalize(Rectangle dragArea)
    {
        var left = dragArea.Left;
        var right = dragArea.Right;
        if (left > right)
        {
            left = dragArea.Right;
            right = dragArea.Left;
        }

        var top = dragArea.Top;
        var bottom = dragArea.Bottom;
        if (top > bottom)
        {
            top = dragArea.Bottom;
            bottom = dragArea.Top;
        }
        return new Rectangle(left, top, right - left, bottom - top);
    }
    private Point ComputeDelta(int x, int y)
    {
        return State.ComputeDelta(x, y);
    }

    public virtual bool Smash(bool force)
    {
        // $"PanZoomService Smash {force}".WriteWarning();
        if (_matrix == null && !force) return false;

        OnMatrixSmash?.Invoke(State);
        // $"PanZoomService Smash".WriteWarning();
        this._matrix = Matrix2D.SmashMatrix(this._matrix);
        this._invMatrix = Matrix2D.SmashMatrix(this._invMatrix);

        //SRS SET THIS IN ORDER TO Do ANY HITTEST!!!!
        FoGlyph2D.ResetHitTesting(true);
        return true;
    }

    public virtual Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            _matrix = Matrix2D.NewMatrix();
            _matrix.AppendTransform(State.Pan.X, State.Pan.Y, State.Zoom, State.Zoom, 0.0, 0.0, 0.0);
            FoGlyph2D.ResetHitTesting(true, "Pan Zoom");
            OnMatrixRefresh?.Invoke(State);
            // $"PanZoomService GetMatrix recalculate".WriteWarning();
        }
        return _matrix;
    }

    public Matrix2D GetInvMatrix()
    {
        _invMatrix ??= this.GetMatrix().InvertCopy();
        return _invMatrix;
    }

    public Rectangle HitLocation(CanvasMouseArgs args)
    {
        //$"{_pan.X} {_pan.Y}".WriteLine(ConsoleColor.Blue);
        var (x, y) = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);

        var hit = new Rectangle(x, y, 1, 1);
        return hit;
    }

    //for starting fence select
    public Rectangle HitRectStart(CanvasMouseArgs args)
    {
        var hit = HitLocation(args);
        ComputeDelta(hit.Left, hit.Top);
        return hit;
    }

    // for doing shape select
    public Rectangle HitRectContinue(CanvasMouseArgs args, Rectangle rect)
    {
        var (x, y) = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);
        rect.Width = x - rect.X;
        rect.Height = y - rect.Y;
        return rect;
    }

    //for resizing and image
    public Rectangle HitRectTopLeft(CanvasMouseArgs args, Rectangle rect)
    {
        var (x, y) = GetInvMatrix().TransformPoint(args.OffsetX, args.OffsetY);

        var x1 = rect.Left;
        var y1 = rect.Top;
        var x2 = x;
        var y2 = y;
        var hit = new Rectangle(x1, y1, x2 - x1, y2 - y1);
        State.LastLocation = new Point(0, 0);
        ComputeDelta(hit.Left, hit.Top);
        return hit;
    }

    public Rectangle TransformRect(Rectangle rect)
    {
        var matrix = GetMatrix();
        var pt1 = matrix.TransformToPoint(rect.Left, rect.Top);
        var pt2 = matrix.TransformToPoint(rect.Right, rect.Bottom);

        var newRect = new Rectangle(pt1.X, pt1.Y, pt2.X - pt1.X, pt2.Y - pt1.Y);

        return newRect;
    }
    public Point[] TransformPoint(Point[] points)
    {
        var matrix = GetMatrix();
        var newPoints = new Point[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            var oldPoint = points[i];
            var pt = matrix.TransformToPoint(oldPoint.X, oldPoint.Y);
            newPoints[i] = pt;
        }

        return newPoints;
    }

    public Rectangle AntiScaleRect(Rectangle rect)
    {
        //anti scale this window
        var zoom = State.Zoom;
        var x = (rect.X - State.Pan.X) / zoom;
        var y = (rect.Y - State.Pan.Y) / zoom;
        var width = rect.Width / zoom;
        var height = rect.Height / zoom;

        var newRect = new Rectangle((int)x, (int)y, (int)width, (int)height);
        //$"AntiScaleRect {x} {y} {width} {height}".WriteLine(ConsoleColor.Yellow);

        return newRect;
    }



    public double ZoomWheel(double delta)
    {
        State.LastZoom = State.Zoom;
        State.Zoom *= delta < 0 ? 1.1 : 0.9;
        Smash(true);
        OnEventComplete?.Invoke();
        return State.Zoom;
    }

    public Point MouseDeltaMovement()
    {
        return State.Delta;

    }

    public void Deconstruct(out double zoom, out int panx, out int pany)
    {
        zoom = State.Zoom;
        panx = State.Pan.X;
        pany = State.Pan.Y;
    }

    public PanZoomService TranslateAndScale(FoGlyph2D shape)
    {
        var mtx = this.GetMatrix();
        return this;
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
        State.LastZoom = State.Zoom;
        State.Zoom = zoom;
        Smash(true);
        OnEventComplete?.Invoke();
        return State.Zoom;
    }

    public double Zoom()
    {
        return State.Zoom;
    }
    public double LastZoom()
    {
        return State.LastZoom;
    }
    public Point Pan()
    {
        return State.Pan;
    }

    public Point PanBy(int dx, int dy)
    {
        State.Pan.X += dx;
        State.Pan.Y += dy;
        Smash(true);
        OnEventComplete?.Invoke();
        return State.Pan;
    }


    public Point SetPan(int x, int y)
    {
        State.Pan = new(x, y);
        Smash(true);
        OnEventComplete?.Invoke();
        return State.Pan;
    }
}