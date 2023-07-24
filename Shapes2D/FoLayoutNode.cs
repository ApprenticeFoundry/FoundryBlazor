
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutNode<V> where V : FoGlyph2D
{

    private readonly string[] Colors = new string[] { "Red", "White", "Purple", "Green", "Grey", "Purple", "Pink", "Brown", "Grey", "Black", "White", "Crimson", "Indigo", "Violet", "Magenta", "Turquoise", "Teal", "SlateGray", "DarkSlateGray", "SaddleBrown", "Sienna", "DarkKhaki", "Goldenrod", "DarkGoldenrod", "FireBrick", "DarkRed", "RosyBrown", "DarkMagenta", "DarkOrchid", "DeepSkyBlue" };
    public double X { get; set; }
    public double Y { get; set; }
    public double Mass { get; }
    public double Dx { get; set; }
    public double Dy { get; set; }

    private V _item;

    public FoLayoutNode(V node)
    {
        _item = node;
    }

    public void ClearAll()
    {
        _item = null!;
    }

    public string GetGlyphId()
    {
        return _item.GetGlyphId();
    }

    public V GetShape()
    {
        return _item;
    }

    public async Task RenderLayoutNetwork(Canvas2DContext ctx, int tick)
    {


        await ctx.SaveAsync();
        
        await ctx.SetLineWidthAsync(4);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });
        //await ctx.SetStrokeStyleAsync(Colors[level]);

  
        await ctx.RestoreAsync();
    }



    private static Point Relocate(Point pt, V shape)
    {
        return new Point(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape));
    }

}
