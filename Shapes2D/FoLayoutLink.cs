
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using System.Drawing;
using System;

namespace FoundryBlazor.Shape;


public class FoLayoutLink<U,V> where V : FoShape2D where U : FoShape1D
{

    private U _item;
    private FoLayoutNode<V>? _source;
    private FoLayoutNode<V>? _sink;

    public FoLayoutLink(FoLayoutNode<V> node1, FoLayoutNode<V> node2)
    {
        _item = Activator.CreateInstance<U>();
        _source = node1;
        _sink = node2;
    }

    public void ClearAll()
    {
        _source = null;
        _sink = null;
    }

    public string GetSourceGlyphId()
    {
        return _source.GetGlyphId();
    }
    public string GetSinkGlyphId()
    {
        return _sink.GetGlyphId();
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
