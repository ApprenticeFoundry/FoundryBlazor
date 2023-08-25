using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public class FoConnectionPoint2D : FoHandle2D
{

    public FoConnectionPoint2D() : base()
    {

    }
    public FoConnectionPoint2D(string name, int x, int y, string color) : base(name, x, y, color)
    {
    }

    public override FoGlyph2D MarkSelected(bool value)
    {
        var glue = GetMembers<FoGlue2D>();
            glue?.ForEach(item => item.MarkSelected(value));
            
        return base.MarkSelected(value);
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
         if ( CannotRender() ) return false;

        await ctx.SaveAsync();
        await ctx.SetFillStyleAsync(Color);
        await ctx.FillRectAsync(PinX-LocPinX(this), PinY-LocPinY(this), Width, Height);     
        await ctx.RestoreAsync();
        return true;
    }


}
