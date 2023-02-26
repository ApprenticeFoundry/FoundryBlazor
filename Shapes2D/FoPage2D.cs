
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;
public class FoPage2D : FoGlyph2D
{
    
    public static bool RefreshMenus { get; set; } = true;
    public bool IsActive { get; set; } = false;
    public double PageMargin { get; set; } = .50;  //inches
    public double PageWidth { get; set; } = 10.0;  //inches
    public double PageHeight { get; set; } = 4.0;  //inches

    protected IScaledDrawingHelpers? _ScaledDrawing;
    protected List<IFoCollection>? _RenderLayers;


    public override Rectangle Rect()
    {
        var pt = new Point(PinX, PinY);
        var sz = new Size(Width, Height);
        var result = new Rectangle(pt, sz);
        return result;
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

    public virtual void SetScaledDrawing(IScaledDrawingHelpers scaledDrawing)
    {
        _ScaledDrawing = scaledDrawing;
        scaledDrawing.SetPageDefaults(this);
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

    public void SmashLayers() 
    {
        if ( _RenderLayers != null)
            $"SmashLayers".WriteInfo(1);

        _RenderLayers = null;
    }

    public List<IFoCollection> Layers()
    {
        if ( _RenderLayers == null)
        {
            $"getting Layers".WriteInfo(1);
            _RenderLayers = AllGlyphSlots();

            foreach (var item in _RenderLayers)
            {
                $"key = {item.GetKey()}".WriteInfo();
            }

            $"getting Layers {_RenderLayers.Count}".WriteInfo(1);
        }


        return _RenderLayers;
    }

    public FoPage2D ClearAll()
    {
        Layers().ForEach(item => item.Clear());
        return this;
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();
        foreach (var item in Layers())
        {
            if (item is FoCollection<FoGlyph2D> col)
            {
                var found = col.FindWhere(child => child.GlyphId == GlyphId);
                if (found != null) result.AddRange(found);
            }
        }
        return result;
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();
        foreach (var item in Layers())
        {
             if ( item is FoCollection<FoGlyph2D> col) 
             {
                var found = col.ExtractWhere(child => child.GlyphId == GlyphId);
                if (found != null) result.AddRange(found);
             }       
        }
        return result;
    }


   public new bool ComputeShouldRender(Rectangle region)
    {
        foreach (var item in Layers())
        {
            if ( item is FoCollection<FoGlyph2D> col) 
                col.Values().ForEach(child => child.ComputeShouldRender(region));
        }
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

        foreach (var item in Layers())
        {
            if ( item is IFoCollection col) {

                //$"RenderDetailed Layer {col.GetKey()} {values.Count}".WriteInfo();
                foreach (var shape in col.AllItem())
                {
                    await ((FoGlyph2D)shape).RenderDetailed(ctx, tick, deep);
                }
            }
        }

        await ctx.RestoreAsync();
        return true;
    }

}
