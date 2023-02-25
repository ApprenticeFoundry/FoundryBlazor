
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor;

namespace FoundryBlazor.Shape;
public class FoPage2D : FoGlyph2D
{
    
    public static bool RefreshMenus { get; set; } = true;

    public bool IsActive { get; set; } = false;

    protected IScaledDrawingHelpers? _ScaledDrawing;

    public double PageMargin { get; set; } = .50;  //inches
    public double PageWidth { get; set; } = 10.0;  //inches
    public double PageHeight { get; set; } = 4.0;  //inches

    public override Rectangle Rect()
    {
        var pt = new Point(PinX, PinY);
        var sz = new Size(Width, Height);
        var result = new Rectangle(pt, sz);
        return result;
    }

    public virtual void SetScaledDrawing(IScaledDrawingHelpers scaledDrawing)
    {
        _ScaledDrawing = scaledDrawing;
        scaledDrawing.SetPageDefaults(this);
    }

    public FoPage2D(string name, string color) : base(name, color)
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public FoPage2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public int DrawingWidth()
    {
        return _ScaledDrawing?.ToPixels(PageWidth) ?? 0;
    }
    public int DrawingHeight()
    {
        return _ScaledDrawing?.ToPixels(PageHeight) ?? 0;
    }
    public int DrawingMargin()
    {
        return _ScaledDrawing?.ToPixels(PageMargin) ?? 0;  //margin all around
    }

     public string DrawingWH()
    {
        return $"Drawing Size [{PageWidth}x{PageHeight} ({PageMargin})]in";
    }   

    public override List<T> CollectMembers<T>(List<T> list, bool deep = true)
    {
        base.CollectMembers<T>(list, deep);

        if (deep)
        {
            GetMembers<FoCompound2D>()?.ForEach(item => item.CollectMembers<T>(list, deep));
        }

        return list;
    }

    public FoPage2D ClearAll()
    {
        GetSlot<FoShape2D>()?.Flush();
        GetSlot<FoShape1D>()?.Flush();
        //GetSlot<FoHero2D>()?.Flush();

        GetSlot<FoConnector1D>()?.Flush();
        GetSlot<FoText2D>()?.Flush();
        GetSlot<FoGroup2D>()?.Flush();
        GetSlot<FoImage2D>()?.Flush();
        GetSlot<FoDragTarget2D>()?.Flush();
        return this;
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var Shape2D = FindWhere<FoShape2D>(child => child.GlyphId == GlyphId);
        if (Shape2D != null) result.AddRange(Shape2D);

        var Shape1D = FindWhere<FoShape1D>(child => child.GlyphId == GlyphId);
        if (Shape1D != null) result.AddRange(Shape1D);

        var Connector1D = FindWhere<FoConnector1D>(child => child.GlyphId == GlyphId);
        if (Connector1D != null) result.AddRange(Connector1D);

        var Text2D = FindWhere<FoText2D>(child => child.GlyphId == GlyphId);
        if (Text2D != null) result.AddRange(Text2D);

        var Group2D = FindWhere<FoGroup2D>(child => child.GlyphId == GlyphId);
        if (Group2D != null) result.AddRange(Group2D);

        var Image2D = FindWhere<FoImage2D>(child => child.GlyphId == GlyphId);
        if (Image2D != null) result.AddRange(Image2D);


        //var Hero2D = FindWhere<FoHero2D>(child => child.GlyphId == GlyphId);
        //if (Hero2D != null) result.AddRange(Hero2D);

        var IDragTarget2D = FindWhere<FoDragTarget2D>(child => child.GlyphId == GlyphId);
        if (IDragTarget2D != null) result.AddRange(IDragTarget2D);

        return result;
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var Shape2D = ExtractWhere<FoShape2D>(child => child.GlyphId == GlyphId);
        if (Shape2D != null) result.AddRange(Shape2D);

        var Shape1D = ExtractWhere<FoShape1D>(child => child.GlyphId == GlyphId);
        if (Shape1D != null) result.AddRange(Shape1D);

        var Connector1D = FindWhere<FoConnector1D>(child => child.GlyphId == GlyphId);
        if (Connector1D != null) result.AddRange(Connector1D);

        var Text2D = ExtractWhere<FoText2D>(child => child.GlyphId == GlyphId);
        if (Text2D != null) result.AddRange(Text2D);

        var Group2D = ExtractWhere<FoGroup2D>(child => child.GlyphId == GlyphId);
        if (Group2D != null) result.AddRange(Group2D);

        var Image2D = ExtractWhere<FoImage2D>(child => child.GlyphId == GlyphId);
        if (Image2D != null) result.AddRange(Image2D);

        // var Hero2D = ExtractWhere<FoHero2D>(child => child.GlyphId == GlyphId);
        // if (Hero2D != null) result.AddRange(Hero2D);

        var DragTarget2D = ExtractWhere<FoDragTarget2D>(child => child.GlyphId == GlyphId);
        if (DragTarget2D != null) result.AddRange(DragTarget2D);
        return result;
    }




   public new bool ComputeShouldRender(Rectangle region)
    {  
        GetMembers<FoShape1D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoConnector1D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoImage2D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoVideo2D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoGroup2D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoShape2D>()?.ForEach(child => child.ComputeShouldRender(region));
        GetMembers<FoText2D>()?.ForEach(child => child.ComputeShouldRender(region));
 
        //GetMembers<FoHero2D>()?.ForEach(child => child.ComputeShouldRender(region));
         return true;
    }

    public async Task RenderGrid(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();

        if (_ScaledDrawing != null)
        {
            await _ScaledDrawing.DrawHorizontalGrid(ctx, 0.5, 2.0);
            await _ScaledDrawing.DrawVerticalGrid(ctx, 0.5, 2.0);
        }

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

        if (_ScaledDrawing != null)
        {
            var win = _ScaledDrawing.UserWindow();
            await ctx.StrokeRectAsync(win.X, win.Y, win.Width, win.Height);
        }

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
