
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutNetwork<U,V> where V : FoShape2D where U : FoShape1D
{

     private readonly string[] Colors = new string[] { "Red", "White", "Purple", "Green", "Grey", "Purple", "Pink", "Brown", "Grey", "Black", "White", "Crimson", "Indigo", "Violet", "Magenta", "Turquoise", "Teal", "SlateGray", "DarkSlateGray", "SaddleBrown", "Sienna", "DarkKhaki", "Goldenrod", "DarkGoldenrod", "FireBrick", "DarkRed", "RosyBrown", "DarkMagenta", "DarkOrchid", "DeepSkyBlue" };


    private List<FoLayoutLink<U,V>>? _links;
    private List<FoLayoutNode<V>>? _nodes;

    public FoLayoutNetwork(DT_System system)
    {
    }

    public void ClearAll()
    {
    }



    public async Task RenderLayoutNetwork(Canvas2DContext ctx, int tick)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);

        await ctx.SaveAsync();


        
        await ctx.SetLineWidthAsync(4);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });
       // await ctx.SetStrokeStyleAsync(Colors[level]);


        await ctx.RestoreAsync();
    }

}
