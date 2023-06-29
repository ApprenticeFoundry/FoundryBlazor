using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public class FoPanZoomWindow : FoGlyph2D
{
    public double ViewScale { get; set; } = .1;
    public Point ViewPan { get; set; } = new(0, 0);

    private readonly IPageManagement PageManager;
    private readonly IPanZoomService PanZoomService;
    private readonly IHitTestService _hitTestService;
    private readonly IDrawing _scaled;


    public FoPanZoomWindow(IPageManagement manager, IPanZoomService panzoom, IHitTestService hitTest, IDrawing scaled, string color) : base("Pan Zoom", color)
    {
        PageManager = manager;
        PanZoomService = panzoom;
        _hitTestService = hitTest;
        _scaled = scaled;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public override Rectangle Rect()
    {
        //anti scale this window
        var pan = PanZoomService.Pan();
        var zoom = PanZoomService.Zoom();
        var pt = GetMatrix().TransformPoint(0, 0);
        var sz = new Size((int)(Width / zoom), (int)(Height / zoom));
        var result = new Rectangle(pt, sz);
        return result;
    }

    public override Matrix2D GetMatrix()
    {
        //must do anti scaling to prevent this from moving 
        if (_matrix == null)
        {

            var pan = PanZoomService.Pan();
            var zoom = PanZoomService.Zoom();

            var x = PinX / zoom;
            var y = PinY / zoom;

            _matrix = Matrix2D.NewMatrix();
            x -= pan.X;
            y -= pan.Y;
            zoom = 1.0 / zoom;
            _matrix.AppendTransform(x, y, zoom, zoom, RotationZ(this), LocPinX(this), LocPinY(this));
            FoGlyph2D.ResetHitTesting = true;
        }
        return _matrix;
    }



    public override async Task<bool> RenderConcise(Canvas2DContext ctx, double zoom, Rectangle region)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, 0);


        // this is the background of the window it is anti-scaled
        await ctx.SetFillStyleAsync(Color);
        await ctx.FillRectAsync(0, 0, Width, Height);

        PreDraw?.Invoke(ctx, this);

        await ctx.SaveAsync();

        //create a transform to fit the canvas in the window
        await ctx.TranslateAsync(ViewPan.X, ViewPan.Y);
        await ctx.ScaleAsync(ViewScale, ViewScale);

        //this is the size of the canvas the window should scale to fit
        var canvas = _scaled.TrueCanvasSize();
        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.FillRectAsync(0, 0, canvas.Width, canvas.Height);
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.StrokeRectAsync(0, 0, canvas.Width, canvas.Height);

        var page = PageManager.CurrentPage();
        // this is the magic to render the page in the right zoom and loc 

        var (zoom1, panx, pany) = await PanZoomService.TranslateAndScale(ctx, page);
        await page.RenderConcise(ctx, zoom, region);

        await _hitTestService.RenderQuadTree(ctx, false);

        await ctx.RestoreAsync();

        PostDraw?.Invoke(ctx, this);

        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);
        else
            ShapeDrawSelected?.Invoke(ctx, this);

        await ctx.RestoreAsync();
        return true;
    }


    public async Task DrawDetails(Canvas2DContext ctx, int tick, bool deep = true)
    {
        //this is the background of the window it is anti-scaled
        await ctx.SetFillStyleAsync(Color);
        await ctx.FillRectAsync(0, 0, Width, Height);

        await ctx.SaveAsync();

        //create a transform to fit the canvas in the window
        await ctx.TranslateAsync(ViewPan.X, ViewPan.Y);
        await ctx.ScaleAsync(ViewScale, ViewScale);

        //this is the size of the canvas the window should scale to fit
        var canvas = _scaled.TrueCanvasSize();
        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.FillRectAsync(0, 0, canvas.Width, canvas.Height);
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.StrokeRectAsync(0, 0, canvas.Width, canvas.Height);

        var page = PageManager.CurrentPage();
        // this is the magic to render the page in the right zoom and loc 
        await PanZoomService.TranslateAndScale(ctx, page);

        //await page.RenderConcise(ctx, zoom, page.Rect());       
        await page.RenderDetailed(ctx, tick, deep);

        await _hitTestService.RenderQuadTree(ctx, true);
        await ctx.RestoreAsync();
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await DrawDetails(ctx, tick, deep);
        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        await ctx.RestoreAsync();
        return true;
    }


    public void SizeToFit()
    {
        var size = _scaled.TrueCanvasSize();
        var max = Math.Max(size.Width, size.Height);
        if (max != 0)
        {
            var scale = 300.0 / max;  //this is the view scale
            var width = (int)(size.Width * scale);
            var height = (int)(size.Height * scale);
            ResizeTo(20 + width, 20 + height);
            ViewScale = 0.97 * scale;
            var dx = (int)(20 + width - 0.97 * width) / 2;
            var dy = (int)(20 + height - 0.97 * height) / 2;
            ViewPan = new Point(dx, dy);
        }
        else
        {
            ResizeTo(250, 250);
            ViewScale = .1;
        }

    }
}
