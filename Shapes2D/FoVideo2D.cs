using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
 
using FoundryBlazor.Shared;
using FoundryBlazor.Shared.SVG;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

//  https://learn.microsoft.com/en-us/aspnet/core/blazor/images?view=aspnetcore-7.0
//  https://www.mikesdotnetting.com/article/361/resize-images-before-uploading-in-blazor-web-assembly
namespace FoundryBlazor.Shape;

public class FoVideo2D : FoShape2D, IImage2D
{
    // TODO: can we figure out how to inject JsRuntime?
    public IJSRuntime? JsRuntime { get; set; }
    public static bool RefreshVideos { get; set; } = true;
    private double scaleX = 1.0;
    public double ScaleX
    {
        get { return this.scaleX; }
        set { this.scaleX = AssignDouble(value, scaleX); }
    }


    private double scaleY = 1.0;
    public double ScaleY
    {
        get { return this.scaleY; }
        set { this.scaleY = AssignDouble(value, scaleY); }
    }

    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }

    public string Id { get; set; } = "";
    private string imageUrl = "";
    public string ImageUrl
    {
        get { return this.imageUrl; }
        set { this.imageUrl = value; waitcount = 0; FoVideo2D.RefreshVideos = true; Id = Guid.NewGuid().ToString(); }
    }

    private ElementReference imageRef;
    public ElementReference ImageRef
    {
        get { return this.imageRef; }
        set { this.imageRef = value; }
    }

    private int waitcount = 0;

    public override FoVideo2D ZoomBy(double factor)
    {
        ShrinkBy(factor);
        return this;
    }
    public void ShrinkBy(double factor)
    {
        ScaleX *= factor;
        ScaleY *= factor;
    }

    public override FoGlyph2D ResizeToBox(Rectangle rect)
    {
        var dx = (double)rect.Width / (double)Width;
        var dy = (double)rect.Height / (double)Height;
        base.ResizeToBox(rect);
        ScaleX *= dx;
        ScaleY *= dy;

        //Console.WriteLine($"ScaleX {ScaleX} ScaleY {ScaleY}  {Width} width={rect.Width}, {Height} height={rect.Height}");

        return this;
    }

    public override List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        list.Add(this);
        return list;
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {

        await ctx.SaveAsync();
        if (!String.IsNullOrEmpty(ImageRef.Id))
        {
            await ctx.ScaleAsync(ScaleX, ScaleY);
            await ctx.DrawImageAsync(ImageRef, 0, 0);
        }
        else if (waitcount > 10)
        {

            await ctx.SetFillStyleAsync("Black");
            await ctx.FillRectAsync(0, 0, Width, Height);

            await ctx.SetTextAlignAsync(TextAlign.Center);
            await ctx.SetTextBaselineAsync(TextBaseline.Middle);

            var TextColor = "#98AFC7";
            var FontSpec = $"normal bold 45px sans-serif";

            await ctx.SetFontAsync(FontSpec);
            await ctx.SetFillStyleAsync(TextColor);
            await ctx.FillTextAsync(ImageUrl, Width / 2, Height / 2);
        }
        else
        {
            waitcount++;
        }
        await ctx.RestoreAsync();

    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (HoverDraw != null)
        {
            GetMembers<FoButton2D>()?.ForEach(async item => await item.RenderDetailed(ctx, tick, deep));
        }

        await ctx.RestoreAsync();
        return true;
    }
    public bool MouseHit(CanvasMouseArgs args)
    {
        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.HitTestRect().Contains(pt)).FirstOrDefault();

        if (found != null)
        {
            found.MarkSelected(true);
            //force a redraw after every command is executed
            // if ( PanZoom != null) Smash();
            return true;
        }
        return false;
    }

    public override bool LocalMouseHover(CanvasMouseArgs args, Rectangle loc, Action<Canvas2DContext, FoGlyph2D>? OnHover)
    {
        Members<FoButton2D>().ForEach(child => child.ClearHoverDraw());

        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.HitTestRect().Contains(pt)).FirstOrDefault();
        
        if (found != null && OnHover != null)
        {
            found.SetHoverDraw(OnHover);
            return true;
        }

        return false;
    }


    private void EstablishActions()
    {
        var dy = 250;
        var dx = 100;
        var color = "#67001A";

        var playBtn = new FoButton2D("Play", () => RunJavascript("play"))
        {
            Color = color
        };

        playBtn.MoveBy(dx * 0, dy);
        Add<FoButton2D>(playBtn);

        var pauseBtn = new FoButton2D("Pause", () => RunJavascript("pause"))
        {
            Color = color
        };
        pauseBtn.MoveBy(dx * 1, dy);
        Add<FoButton2D>(pauseBtn);

        var restartBtn = new FoButton2D("Restart", () => RunJavascript("restart"))
        {
            Color = color
        };
        restartBtn.MoveBy(dx * 2, dy);
        Add<FoButton2D>(restartBtn);
    }

    private async void RunJavascript(string action)
    {
        if (JsRuntime != null)
            await JsRuntime.InvokeVoidAsync($"window.VideoManager.{action}", Id);
    }

    public FoVideo2D() : base()
    {
        // EstablishActions();
    }
    public FoVideo2D(int width, int height, string color) : base("", width, height, color)
    {
        // ResetLocalPin((obj) => 0, (obj) => 0);
        EstablishActions();

    }

    public override FoDynamicRender GetDynamicRender()
    {
        foDynamicRender ??= new FoDynamicRender(typeof(Video2D), this);
        return foDynamicRender;
    }
}
