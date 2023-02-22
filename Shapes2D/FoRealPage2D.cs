
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor;


namespace FoundryBlazor.Shape;
public class FoRealPage2D : FoPage2D
{
    public double PageMargin { get; set; } = .50;  //inches
    public double PageWidth { get; set; } = 10.0;  //inches
    public double PageHeight { get; set; } = 4.0;  //inches

    private readonly IScaledDrawingHelpers _helper;


    public FoRealPage2D(string name, int width, int height, string color, IScaledDrawingHelpers helper) : base(name, width, height, color)
    {
        _helper = helper;
        _helper.SetPageDefaults(this);
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public int DrawingWidth()
    {
        return _helper.ToPixels(PageWidth);
    }
    public int DrawingHeight()
    {
        return _helper.ToPixels(PageHeight);
    }
    public int DrawingMargin()
    {
        return _helper.ToPixels(PageMargin);  //margin all around
    }



    public string DrawingWH()
    {
        return $"Drawing Size [{PageWidth}x{PageHeight} ({PageMargin})]in";
    }



    public async Task RenderGrid(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();

        await _helper.DrawHorizontalGrid(ctx, 0.5, 2.0);
        await _helper.DrawVerticalGrid(ctx, 0.5, 2.0);

        await ctx.RestoreAsync();
    }

    public async Task<bool> RenderNoItems(Canvas2DContext ctx, int tick)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        var margin = DrawingMargin();
        Width = DrawingWidth() + 2 * margin;
        Height = DrawingHeight() + 2 * margin;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);

        await ctx.SetFillStyleAsync("Grey");
        await ctx.SetGlobalAlphaAsync(0.75F);
        await ctx.FillRectAsync(margin, margin, DrawingWidth(), DrawingHeight());

        await RenderGrid(ctx);

        await ctx.RestoreAsync();
        return true;
    }

    public override async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, 0);

        var margin = DrawingMargin();
        Width = DrawingWidth() + 2 * margin;
        Height = DrawingHeight() + 2 * margin;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);

        await ctx.SetFillStyleAsync("Blue");
        await ctx.SetGlobalAlphaAsync(0.80F);
        await ctx.FillRectAsync(margin, margin, DrawingWidth(), DrawingHeight());

        await RenderGrid(ctx);

        //$"REC {region.X} {region.Y} {region.Width} {region.Height} ---".WriteLine(ConsoleColor.Blue);

        //only render members inside the region
        //GetMembers<FoHero2D>()?.ForEach(async child => await child.RenderConcise(ctx, scale, region));
   
        // draw the current window
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.SetLineWidthAsync(50.0F);

        var win = _helper.UserWindow();
        await ctx.StrokeRectAsync(win.X, win.Y, win.Width, win.Height);

        await ctx.RestoreAsync();
        return true;
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        var margin = DrawingMargin();
        Width = DrawingWidth() + 2 * margin;
        Height = DrawingHeight() + 2 * margin;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);

        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(0.75F);
        await ctx.FillRectAsync(margin, margin, DrawingWidth(), DrawingHeight());

        await RenderGrid(ctx);

        //await DrawFancyPin(ctx);


        //GetMembers<FoShape1D>()?.ForEach(async child => await child.Render(ctx, tick, deep));
        GetMembers<FoConnector1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetMembers<FoImage2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetMembers<FoVideo2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetMembers<FoGroup2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetMembers<FoText2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        
        //GetMembers<FoHero2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
          
        // Members<FoMenu2D>().ForEach(async child => await child.Render(ctx, tick, deep));
        GetMembers<FoCompound2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));

        GetMembers<FoDragTarget2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));

        await ctx.RestoreAsync();
        return true;
    }
}
