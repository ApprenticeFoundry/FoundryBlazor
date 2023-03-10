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


    public override Point AttachTo()
    {
        var point = base.AttachTo();
     
        //you need to compute where that point is on the parent !!
        //do this for real using a matrix

        if (Level > 0 && GetParent() is FoGlyph2D parent)
        {
            //$"------------------------------++++++AttachTo OLD {point.X}  {point.Y}".WriteInfo(2);

            //var matrix = GetMatrix();
            //point = matrix.TransformPoint(point);

            var matrix = parent.GetMatrix();
            point = matrix.TransformPoint(point);
            //$"AttachTo NEW {point.X}  {point.Y}".WriteInfo(2);
        }
        //else
        //    $"No Parent AttachTo {Name}".WriteError();

        return point;
    }

}
