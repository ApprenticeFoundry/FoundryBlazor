
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutNode<V> : ICanHitTarget where V : FoGlyph2D
{

    public double X { get; set; } = 110.0;
    public double Y { get; set; } = 110.0;
    public double Radius { get; set; } = 50.0;
    public double Mass { get; } = 1.0;
    public double Dx { get; set; } = 0.0;
    public double Dy { get; set; } = 0.0;

    private V _item;

    public FoLayoutNode(V node, int x, int y)
    {
        _item = node;
        MoveTo(x, y);
    }

    public double CalculateLength(FoLayoutNode<V> b)
    {
        return Math.Max(Math.Sqrt(Math.Pow(b.X - X, 2) + Math.Pow(b.Y - Y, 2)), 0.001);
    }

    public (double dx, double dy, double distance) CalculateForceVector(FoLayoutNode<V> b)
    {
        var dx = X - b.X;
        var dy = Y - b.Y;
        var distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
        return (dx/distance, dy/distance, distance);
    }

    public Rectangle HitTestRect()
    {
        return _item.HitTestRect();
    }
    public Point[] HitTestSegment()
    {
        return new Point[0];
    }

    public string GetName()
    {
        return _item.Name ?? "No Name";
    }

    public bool IsSmashed()
    {
        return _item.IsSmashed();
    }

    public void ApplyForce(double fx, double fy)
    {
        Dx += fx / Mass;
        Dy += fy / Mass;
    }

    public void UpdatePositionUsingForceValues(double damping)
    {
        Dx *= damping;
        Dy *= damping;
        X += Dx;
        Y += Dy;
        Dx = Dy = 0;
    }

    public void ClearAll()
    {
        _item = null!;
    }

    public void MoveTo(int x, int y) 
    { 
        (X, Y) = (x, y); 
        GetShape().MoveTo(x, y);
    }

    public void MoveBy(int dx, int dy)
    { 
        (X, Y) = (X+dx, Y+dy); 
        GetShape().MoveBy(dx, dy);
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
