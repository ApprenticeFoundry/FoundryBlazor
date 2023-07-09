
using System.Drawing;
using System.Linq;

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Units;
using Radzen.Blazor.Rendering;

namespace FoundryBlazor.Shape;


public interface IFoPage2D
{

    int MapToPageXScale(Length value);
    int MapToPageYScale(Length value);

    int MapToPageXLoc(Length value);
    int MapToPageYLoc(Length value);
    double MapToModelXLoc(int value);
    double MapToModelYLoc(int value);
}

public class FoPage2D : FoGlyph2D, IFoPage2D
{

    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;

    public Length PageMargin { get; set; } = new Length(1, "cm");  //inches
    public Length PageWidth { get; set; } = new Length(50.0, "cm");  //inches
    public Length PageHeight { get; set; } = new Length(30.0, "cm"); //inches

    public Length GridMajorH { get; set; } = new Length(1.0, "m"); //inches
    public Length GridMinorH { get; set; } = new Length(1, "cm"); //inches

    public Length GridMajorV { get; set; } = new Length(1.0, "m"); //inches
    public Length GridMinorV { get; set; } = new Length(1, "cm"); //inches


    public FoScale2D Scale2D { get; set; } = new FoScale2D()
    {
        Drawing = new Length(1.0, "cm"),
        World = new Length(1.0, "m")
    };

    public FoHorizontalRuler2D HRuler2D { get; set; }
    public FoVerticalRuler2D VRuler2D { get; set; }

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
        HRuler2D = new FoHorizontalRuler2D(Scale2D, this);
        VRuler2D = new FoVerticalRuler2D(Scale2D, this);
    }

    public FoPage2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
        HRuler2D = new FoHorizontalRuler2D(Scale2D, this);
        VRuler2D = new FoVerticalRuler2D(Scale2D, this);
    }


    public int MapToPageXScale(Length value)
    {
        var scale = Scale2D.ScaleToDrawing();
        var pos = scale * value;
        var result = pos.AsPixels();
       // $"PageXScale PW: {PageWidth} W: {value} D: {pos} [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;    
    }

    public int MapToPageXLoc(Length value)
    {
        var m = PageMargin.AsPixels();
        var result = m + MapToPageXScale(value);
       // $"PageXLoc PW: {PageWidth} W: {value} M: {m}  [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;    
    }

    public int MapToPageYScale(Length value)
    {
        var scale = Scale2D.ScaleToDrawing();
        var pos = scale * value;
        var result = pos.AsPixels();
        //$"PageYScale PH: {PageHeight} W: {value} D: {pos}  [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;    
    }

    public int MapToPageYLoc(Length value)
    {
        var m = PageMargin.AsPixels();
        var loc = MapToPageYScale(value);
        var result = m + PageHeight.AsPixels() - loc;
       // $"PageYLoc {PageHeight} {PageHeight.AsPixels()} W: {value} M: {m} L: {loc} [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);

        return result;    
    }

    public double MapToModelXLoc(int value)
    {
        var m = PageMargin.AsPixels();
        var size = Scale2D.PixelToDrawing(value - m);
        var scale = Scale2D.ScaleToWorld();
        var result = size * scale;
        return result;
    }
    public double MapToModelYLoc(int value)
    {
        var pHeight = PageHeight.AsPixels();
        var m = PageMargin.AsPixels();
        var size = Scale2D.PixelToDrawing(value - pHeight - m);
        var scale = Scale2D.ScaleToWorld();
        var result = size * scale;
        return result;
    }
    public void SetPageLandscape()
    {
        if (PageWidth < PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
    }
    public void SetPagePortrait()
    {
        if (PageWidth > PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
    }

    public void SetPageSizeInches(double width, double height)
    {
        PageWidth.Assign(width, "in");
        PageHeight.Assign(height, "in");
    }
    public void SetPageSizeMM(double width, double height)
    {
        PageWidth.Assign(width, "mm");
        PageHeight.Assign(height, "mm");
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
            if (item.IsSelectable()) 
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


        await DrawHorizontalGrid(ctx, GridMinorH, false);
        //await DrawHorizontalGrid(ctx, GridMajorH, true);
        await HRuler2D.DrawRuler(ctx, GridMinorH, false);

        await DrawVerticalGrid(ctx, GridMinorV, false);
        await VRuler2D.DrawRuler(ctx, GridMinorV, false);

        await ctx.RestoreAsync();
    }


    public async Task DrawHorizontalGrid(Canvas2DContext ctx, Length step, bool major)
    {
        await ctx.SaveAsync();

        var dStep = step.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        if ( !major) {            
            await ctx.SetLineWidthAsync(1);
            await ctx.SetLineDashAsync(new float[] { 5, 1 });
            await ctx.SetStrokeStyleAsync("White");
        } else {
            await ctx.SetLineDashAsync(Array.Empty<float>());
            await ctx.SetStrokeStyleAsync("Black");
        }

        var x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dStep;
        }

        await ctx.RestoreAsync();
    }


    public async Task DrawVerticalGrid(Canvas2DContext ctx, Length step, bool major)
    {
        await ctx.SaveAsync();

        var dStep = step.AsPixels();

        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        if (!major)
        {
            await ctx.SetLineWidthAsync(1);
            await ctx.SetLineDashAsync(new float[] { 5, 1 });
            await ctx.SetStrokeStyleAsync("White");
        }
        else
        {
            await ctx.SetLineDashAsync(Array.Empty<float>());
            await ctx.SetStrokeStyleAsync("Black");
        }

        var x = dHeight; //left;
        while (x >= dMargin)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x -= dStep;
        }

        await ctx.RestoreAsync();
    }

    public async Task<bool> RenderNoItems(Canvas2DContext ctx, int tick)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        var margin = PageMargin.AsPixels();
        Width = (PageWidth + 2 * margin).AsPixels();
        Height = (PageHeight + 2 * margin).AsPixels();

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);

        await ctx.SetFillStyleAsync("Grey");
        await ctx.SetGlobalAlphaAsync(0.75F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);

        await ctx.RestoreAsync();
        return true;
    }

    public override async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, 0);

        var margin = PageMargin.AsPixels();
        Width = PageWidth.AsPixels() + 2 * margin;
        Height = PageHeight.AsPixels() + 2 * margin;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX + 5, PinY + 5);

        await ctx.SetFillStyleAsync("Blue");
        await ctx.SetGlobalAlphaAsync(0.80F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);

        //$"REC {region.X} {region.Y} {region.Width} {region.Height} ---".WriteLine(ConsoleColor.Blue);

        //only render members inside the region

        Shapes2D.ForEach(async child => await child.RenderConcise(ctx, scale, region));

        // draw the current window
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.SetLineWidthAsync(50.0F);


        //var win = CurrentScale.UserWindow();
        //await ctx.StrokeRectAsync(win.X, win.Y, win.Width, win.Height);


        await ctx.RestoreAsync();
        return true;
    }

    public async Task DrawPageName(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();
        var text = $"Page: {Name} | {Scale2D.Display()} | W:{PageWidth.AsString("cm")} x H:{PageHeight.AsString("cm")}  ({PageMargin.AsString("cm")}) |";
        text += $"  px {PageWidth.AsPixels()} x {PageHeight.AsPixels()} ({PageMargin.AsPixels()})";


        //Draw the page name at the top
        await ctx.SetFontAsync("16px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync(text, PinX + 5, PinY + 5);
        await ctx.RestoreAsync();
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

  
        var margin = PageMargin.AsPixels();
        Width = (PageWidth + 2.0 * margin).AsPixels();
        Height = (PageHeight + 2.0 * margin).AsPixels();

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        await DrawPageName(ctx);

        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(0.75F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);

        //await DrawFancyPin(ctx);


        Shapes1D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        Shapes2D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));


        await ctx.RestoreAsync();
        return true;
    }

}
