
using System.Drawing;
using System.Linq;

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;

public interface IFoPage2D
{
    void SetScale(ScaledCanvas scale);
    int DrawingWidth();
    int DrawingHeight();
    int DrawingMargin();
}

public class FoPage2D : FoGlyph2D, IFoPage2D
{


    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;
    public Length PageMargin { get; set; } = new Length(.50, "in");  //inches
    public Length PageWidth { get; set; } = new Length(10.0, "in");  //inches
    public Length PageHeight { get; set; } = new Length(4.0, "in"); //inches

    public Length GridMajorH { get; set; } = new Length(2.0, "in"); //inches
    public Length GridMinorH { get; set; } = new Length(0.5, "in"); //inches

    public Length GridMajorV { get; set; } = new Length(4.0, "in"); //inches
    public Length GridMinorV { get; set; } = new Length(0.5, "in"); //inches

    protected ScaledCanvas CurrentScale = new();

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

    public virtual void SetScale(ScaledCanvas scale)
    {
        CurrentScale = scale;
        CurrentScale.SetPageDefaults(this);
    }


    public int DrawingWidth()
    {
        return PageWidth.AsPixels();
    }
    public int DrawingHeight()
    {
        return PageHeight.AsPixels();
    }
    public int DrawingMargin()
    {
        return PageMargin.AsPixels();  //margin all around
    }

    public string DrawingWH()
    {
        return $"Drawing Size [{PageWidth.As("in")}x{PageHeight.As("in")} ({PageMargin.As("in")})]in";
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



    public override bool Smash(bool force)
    {
        if (_matrix == null && !force) return false;
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

        if (value is IShape2D)
        {
            Shapes2D.Add(value);
            //$"IShape2D Added {value.Name}".WriteSuccess();
        }
        else if (value is IShape1D)
        {
            Shapes1D.Add(value);
            // $"IShape1D Added {value.Name}".WriteSuccess();
        }


        return value;
    }

    public List<FoGlyph2D> ExtractSelected(List<FoGlyph2D> list)
    {
        var shapes2d = Shapes2D.ExtractWhere(child => child.IsSelected);
        var shapes1d = Shapes1D.ExtractWhere(child => child.IsSelected);
        list.AddRange(shapes2d);
        list.AddRange(shapes1d);
        return list;
    }

    public List<FoGlyph2D> CollectSelected(List<FoGlyph2D> list)
    {
        var shapes2d = Shapes2D.FindWhere(child => child.IsSelected);
        var shapes1d = Shapes1D.FindWhere(child => child.IsSelected);
        list.AddRange(shapes2d);
        list.AddRange(shapes1d);
        return list;
    }

    public void DeleteShape(FoGlyph2D shape)
    {
        shape.MarkSelected(false);
        ExtractShapes(shape.GlyphId);
        shape.UnglueAll();
    }

    public void InsertShapesToQuadTree(QuadTree<FoGlyph2D> tree)
    {
        //Shapes1D.ForEach(child => tree.Insert(child)); 
        // var count = Shapes2D.Count();
        foreach (var item in Shapes2D.Values())
        {
            tree.Insert(item);
        }

    }

    public FoPage2D ClearAll()
    {
        FoGlyph2D.ResetHitTesting = true;
        Shapes1D.Clear();
        Shapes2D.Clear();
        return this;
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var found = Shapes1D.FindWhere(child => child.GlyphIdCompare(GlyphId));
        if (found != null && found.Count > 0)
            result.AddRange(found);

        found = Shapes2D.FindWhere(child => child.GlyphIdCompare(GlyphId));
        if (found != null && found.Count > 0)
            result.AddRange(found);

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


        await CurrentScale.DrawHorizontalGrid(ctx, GridMinorH, GridMajorH);
        await CurrentScale.DrawVerticalGrid(ctx, GridMinorV, GridMajorV);

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


        var win = CurrentScale.UserWindow();
        await ctx.StrokeRectAsync(win.X, win.Y, win.Width, win.Height);


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

        var text = $"Page: {Name} W:{PageWidth.AsString("in")} x H:{PageHeight.AsString("in")}  ({PageMargin.AsString("in")})";
        text += $"  px {PageWidth.AsPixels()} x {PageHeight.AsPixels()} ({PageMargin.AsPixels()})";

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync(text, PinX + 5, PinY + 5);
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
