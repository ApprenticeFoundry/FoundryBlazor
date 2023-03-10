using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public class FoHandle2D : FoGlyph2D
{

    public FoHandle2D() : base()
    {

    }
    public FoHandle2D(string name, int x, int y, string color) : base(name, 20, 20, color)
    {
        this.x = x;
        this.y = y;
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
