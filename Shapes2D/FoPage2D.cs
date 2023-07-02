
using System.Drawing;
using System.Linq;

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Units;
using Radzen.Blazor.Rendering;

namespace FoundryBlazor.Shape;

public class FoScale2D
{
    public Length Drawing { get; set; } = new Length(1.0, "cm");  //cm
    public Length World { get; set; } = new Length(1.0, "m");  //cm

    public string Display ()
    {
        var result = World / Drawing;
        return $"{World} = {Drawing} scale {result}:1";
    }
}

public class FoHorizontalRuler2D
{
    public FoScale2D Scale { get; set; } 
    public FoPage2D Page { get; set; }

    public  FoHorizontalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }
    public async Task DrawRuler(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dWidth = Page.PageWidth.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dMargin, dHalf, dWidth, dHalf);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetFontAsync("8 px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Bottom);


        var x = dMargin; //left;
        int i = 0;
        var cnt = 0.0;
        while (x <= dWidth)
        {
            if (i % 10 != 0) {
                await ctx.SetStrokeStyleAsync("White");
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(x, dHalf);
                await ctx.LineToAsync(x, dMargin);
                await ctx.StrokeAsync();
                await ctx.SetFillStyleAsync("Black");
                await ctx.FillTextAsync($"{cnt:F1}", x, dMargin-5);
            }
            x += dMinor;
            cnt += 0.1;
            i++;
        }
        await ctx.SetFontAsync("Bold 22 px Segoe UI");

        x = dMargin; //left;
        cnt = 0.0;
        while (x <= dWidth)
        {
            await ctx.SetStrokeStyleAsync("Black");
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin - 10);
            await ctx.LineToAsync(x, dMargin);
            await ctx.StrokeAsync();
            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync($"{cnt:F1}", x, dMargin - 10);
            x += dMajor;
            cnt += 1.0;
        }

        await ctx.RestoreAsync();
    }
}

public class FoVerticalRuler2D
{
    public FoScale2D Scale { get; set; }
    public FoPage2D Page { get; set; }

    public FoVerticalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }
    public async Task DrawRuler(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dHeight = Page.PageHeight.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dHalf, dMargin, dHalf, dHeight);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetFontAsync("8 px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);


        var y = dMargin; //top;
        int i = 0;
        var cnt = 0.0;
        while (y <= dHeight)
        {
            if (i % 10 != 0)
            {
                await ctx.SetStrokeStyleAsync("White");
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(dHalf,y);
                await ctx.LineToAsync(dMargin,y);
                await ctx.StrokeAsync();
                await ctx.SetFillStyleAsync("Black");
                await ctx.FillTextAsync($"{cnt:F1}", dHalf+15, y);
            }
            y += dMinor;
            cnt += 0.1;
            i++;
        }
        await ctx.SetFontAsync("Bold 22 px Segoe UI");

        y = dMargin; //top;
        cnt = 0.0;
        while (y <= dHeight)
        {
            await ctx.SetStrokeStyleAsync("Black");
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin - 10,y);
            await ctx.LineToAsync(dMargin,y);
            await ctx.StrokeAsync();
            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync($"{cnt:F1}", dHalf+10, y);
            y += dMajor;
            cnt += 1.0;
        }

        await ctx.RestoreAsync();
    }
}

public interface IFoPage2D
{
    //void SetScale(ScaledCanvas scale);
    // int DrawingWidth();
    // int DrawingHeight();
    // int DrawingMargin();
}

public class FoPage2D : FoGlyph2D, IFoPage2D
{

    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;

    public Length PageMargin { get; set; } = new Length(1, "cm");  //inches
    public Length PageWidth { get; set; } = new Length(50.0, "cm");  //inches
    public Length PageHeight { get; set; } = new Length(30.0, "cm"); //inches

    public Length GridMajorH { get; set; } = new Length(10.0, "cm"); //inches
    public Length GridMinorH { get; set; } = new Length(1, "cm"); //inches

    public Length GridMajorV { get; set; } = new Length(10.0, "cm"); //inches
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


        await DrawHorizontalGrid(ctx, GridMinorH, GridMajorH);
        await HRuler2D.DrawRuler(ctx, GridMinorH, GridMajorH);

        await DrawVerticalGrid(ctx, GridMinorV, GridMajorV);
        await VRuler2D.DrawRuler(ctx, GridMinorH, GridMajorH);

        await ctx.RestoreAsync();
    }


    public async Task DrawHorizontalGrid(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        await ctx.SetLineWidthAsync(1);
        await ctx.SetLineDashAsync(new float[] { 5, 1 });


        await ctx.SetStrokeStyleAsync("White");

        var x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dMinor;
        }

        await ctx.SetFillStyleAsync("RED");
        await ctx.FillTextAsync($"DW {dWidth}", x + 1, 200);


        await ctx.SetLineDashAsync(Array.Empty<float>());
        await ctx.SetStrokeStyleAsync("Black");

        x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dMajor;
        }

        await ctx.RestoreAsync();
    }


    public async Task DrawVerticalGrid(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        await ctx.SetLineWidthAsync(1);
        await ctx.SetLineDashAsync(new float[] { 5, 1 });


        await ctx.SetStrokeStyleAsync("White");

        var x = dMargin; //left;
        while (x <= dHeight)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x += dMinor;
        }


        await ctx.SetLineDashAsync(Array.Empty<float>());
        await ctx.SetStrokeStyleAsync("Black");

        x = dMargin; //left;
        while (x <= dHeight)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x += dMajor;
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
