
using Blazor.Extensions.Canvas.Canvas2D;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutLink<U, V> where V : FoShape2D where U : FoShape1D
{

    private U _item;
    private FoLayoutNode<V>? _source;
    private FoLayoutNode<V>? _sink;

    public FoLayoutLink(U link)
    {
        _item = link;
    }

    public FoLayoutLink<U, V> Connect(FoLayoutNode<V> node1, FoLayoutNode<V> node2)
    {
        _source = node1;
        _sink = node2;
        _item.GlueStartTo(node1.GetShape());
        _item.GlueFinishTo(node2.GetShape());
        return this;
    }

    public double CalculateLength()
    {
        if (_source == null || _sink == null)
            return 0.0;

        var result = Math.Sqrt(Math.Pow(_source.X - _sink.X, 2) + Math.Pow(_source.Y - _sink.Y, 2));
        return result;
    }

    public (double dx, double dy, double distance) CalculateForceVector()
    {
        if (_source == null || _sink == null)
            return (0, 0, 0);

        var dx = _source.X - _sink.X;
        var dy = _source.Y - _sink.Y;
        var distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
        return (dx/distance, dy/distance, distance);
    }

    public void ClearAll()
    {
        _source = null;
        _sink = null;
    }

    public U GetShape()
    {
        return _item;
    }

    public FoLayoutNode<V> GetSource()
    {
        return _source!;
    }
    public FoLayoutNode<V> GetSink()
    {
        return _sink!;
    }
    public string GetSourceGlyphId()
    {
        return _source?.GetGlyphId() ?? "";
    }
    public string GetSinkGlyphId()
    {
        return _sink?.GetGlyphId() ?? "";
    }
    public async Task RenderLayoutNetwork(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(4);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });

        await ctx.RestoreAsync();
    }



    private static Point Relocate(Point pt, V shape)
    {
        return new Point(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape));
    }


}
