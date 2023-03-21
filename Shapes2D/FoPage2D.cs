
using System.Drawing;
using System.Linq;

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

    protected FoCollection<FoGlyph2D> Shapes1D = new();
    protected FoCollection<FoGlyph2D> Shapes2D = new();


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

    public override List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        Shapes2D.ForEach(item => item.CollectImages(list, deep));
        return list;
    }

    public override List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        Shapes2D.ForEach(item => item.CollectVideos(list, deep));
        return list;
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

    public U EstablishMenu2D<U>(string name, bool clear) where U : FoMenu2D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshMenus = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public override bool Smash(bool force)
    {
        if ( _matrix == null && !force) return false;
        $"Smashing Page {Name} {GetType().Name}".WriteInfo(2);

        return base.Smash(force);
    }

    public T AddShape<T>(T value) where T : FoGlyph2D
    {
        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Name))
        {
            value.Name = collection.NextItemName();
        }

        collection.AddObject(value.Name, value);

        if ( value is IShape2D)
        {
            Shapes2D.Add(value);   
            //$"Shapes2D Added {value.Name}".WriteSuccess();
        }
        else if ( value is IShape1D)
        {
            Shapes1D.Add(value);
            // $"Shapes1D Added {value.Name}".WriteSuccess();
        }


        return value;
    }


    public void InsertShapesToQuadTree(QuadTree<FoGlyph2D> tree) 
    {
        //Shapes1D.ForEach(child => tree.Insert(child)); 
        Shapes2D.ForEach(child => tree.Insert(child)); 
    }

    public FoPage2D ClearAll()
    {
        Shapes1D.Clear();
        Shapes2D.Clear();
        return this;
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var found = Shapes1D.FindWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);

        found = Shapes2D.FindWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);

        return result;
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var found = Shapes1D.ExtractWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);

        found = Shapes2D.ExtractWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);


        return result;
    }


   public new bool ComputeShouldRender(Rectangle region)
    {
        Shapes1D.ForEach(child => child.ComputeShouldRender(region));
        Shapes2D.ForEach(child => child.ComputeShouldRender(region));
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

        Shapes2D.ForEach(async child => await child.RenderConcise(ctx, scale, region));
   
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

    public async Task DrawPageName(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();
        //Draw the page name at the top
        await ctx.SetFontAsync("16px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);
        await ctx.RestoreAsync();
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

        await DrawPageName(ctx);

        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(0.75F);
        await ctx.FillRectAsync(margin, margin, DrawingWidth(), DrawingHeight());

        await RenderGrid(ctx);

        //await DrawFancyPin(ctx);


        Shapes1D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        Shapes2D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));


        await ctx.RestoreAsync();
        return true;
    }

}
