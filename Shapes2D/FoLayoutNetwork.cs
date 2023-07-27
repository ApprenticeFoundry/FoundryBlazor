
using Blazor.Extensions.Canvas.Canvas2D;

using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutNetwork<U,V> where V : FoShape2D where U : FoShape1D
{

    public Rectangle Boundary = new (100, 100, 700, 700);

    private readonly string[] Colors = new string[] { "Red", "White", "Purple", "Green", "Grey", "Purple", "Pink", "Brown", "Grey", "Black", "White", "Crimson", "Indigo", "Violet", "Magenta", "Turquoise", "Teal", "SlateGray", "DarkSlateGray", "SaddleBrown", "Sienna", "DarkKhaki", "Goldenrod", "DarkGoldenrod", "FireBrick", "DarkRed", "RosyBrown", "DarkMagenta", "DarkOrchid", "DeepSkyBlue" };

    private double springFactor = 0.1;
    private double temperature = 200;
    private double coolingFactor = 0.99;

    private List<FoLayoutLink<U,V>> _links = new();
    private List<FoLayoutNode<V>> _nodes = new();

    public FoLayoutNetwork()
    {
    }

    public void AddLink(FoLayoutLink<U,V> link)
    {
        _links.Add(link);
    }
    public void AddNode(FoLayoutNode<V> node)
    {
        _nodes.Add(node);
    }

    public void ClearAll()
    {
        _links.Clear();
        _nodes.Clear();
    }

    public FoLayoutNode<V>? FindTarget(string guid)
    {
        var found  = _nodes?.FirstOrDefault(t => t.GetGlyphId().Matches(guid));
        return found;
    }

    public async Task RenderLayoutNetwork(Canvas2DContext ctx, int tick)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);
        await ctx.SaveAsync();

        await ctx.BeginPathAsync();
        await ctx.SetLineDashAsync(new float[] { 10, 10 });
        await ctx.SetLineWidthAsync(3);
        await ctx.SetStrokeStyleAsync("Red");
        await ctx.StrokeRectAsync(Boundary.X, Boundary.Y, Boundary.Width, Boundary.Height);
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }

    private static double CalculateDistance(FoLayoutNode<V> a, FoLayoutNode<V> b)
    {
        return Math.Max(Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2)), 0.001);
    }

    private void ApplyCoulombForces(FoLayoutNode<V> node, List<FoLayoutNode<V>> allNodes)
    {
        foreach (var otherNode in allNodes)
        {
            if (node != otherNode)
            {
                double dx = otherNode.X - node.X;
                double dy = otherNode.Y - node.Y;
                double distance = FoLayoutNetwork<U, V>.CalculateDistance(node, otherNode);
                double force = springFactor * springFactor / distance;
                node.Dx += dx / distance * force / node.Mass;
                node.Dy += dy / distance * force / node.Mass;
            }
        }
    }

    private void ApplyHookesLaw(FoLayoutLink<U,V> link)
    {
        var source = link.GetSource();
        var sink = link.GetSink();
        if (source == null || sink == null)
            return;
            
        double dx = sink.X - source.X;
        double dy = sink.Y - source.Y;
        double distance = CalculateDistance(sink, source);
        double force = distance * distance / springFactor;
        double fx = dx / distance * force;
        double fy = dy / distance * force;
        source.Dx += fx / source.Mass;
        source.Dy += fy / source.Mass;
        sink.Dx -= fx / sink.Mass;
        sink.Dy -= fy / sink.Mass;
    }

    public void DoLayoutStep(int tick)
    {
    }

   public void DoLayoutStepXXX(int tick)
    {

        if (temperature <= 0.1)
            return;
        
        foreach (var node in _nodes)
        {
            ApplyCoulombForces(node, _nodes);
        }

        foreach (var link in _links)
        {
            ApplyHookesLaw(link);
        }

            // Move nodes based on the accumulated forces
        foreach (var node in _nodes)
        {
            // Damping factor helps to stabilize the layout
            node.X += node.Dx * temperature;
            node.Y += node.Dy * temperature;

            node.MoveTo((int)node.X, (int)node.Y);

            // Avoid node collisions
            foreach (var otherNode in _nodes)
            {
                if (node != otherNode)
                {
                    double dx = otherNode.X - node.X;
                    double dy = otherNode.Y - node.Y;
                    double distance = CalculateDistance(node, otherNode);
                    double collisionDistance = 0.1; // Minimum distance to avoid collision
                    if (distance < collisionDistance)
                    {
                        double moveDistance = (collisionDistance - distance) / distance * 0.5;
                        node.X -= dx * moveDistance;
                        node.Y -= dy * moveDistance;
                        node.MoveTo((int)node.X, (int)node.Y);
                    }
                }
            }
        }

            // Cool down the system by reducing the temperature
        temperature *= coolingFactor;

        foreach (var node in _nodes)
        {
            var shape = node.GetShape();
            $"{tick} Node {shape.Name} - X: {node.X}, Y: {node.Y}   {shape.PinX}  {shape.PinY}".WriteInfo();
        }
    }
    
}
