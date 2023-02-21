
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor;

namespace FoundryBlazor.Shape;
public class FoPage2D : FoGlyph2D
{
    
    public static bool RefreshMenus { get; set; } = true;

    public bool IsActive { get; set; } = false;
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



    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();


        //Members<FoGlyph2D>().ForEach(async child => await child.Render(ctx, tick, deep));
        GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
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

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Name}", PinX+5, PinY+5);

        await ctx.RestoreAsync();
        return true;
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
}
