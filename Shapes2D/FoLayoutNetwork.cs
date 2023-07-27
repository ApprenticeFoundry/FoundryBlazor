
using Blazor.Extensions.Canvas.Canvas2D;

using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using Radzen.Blazor.Rendering;
using System.Drawing;
using System.Runtime.ExceptionServices;

namespace FoundryBlazor.Shape;


public class FoLayoutNetwork<U,V> where V : FoShape2D where U : FoShape1D
{

    public Rectangle Boundary = new (100, 100, 700, 700);

    private readonly string[] Colors = new string[] { "Red", "White", "Purple", "Green", "Grey", "Purple", "Pink", "Brown", "Grey", "Black", "White", "Crimson", "Indigo", "Violet", "Magenta", "Turquoise", "Teal", "SlateGray", "DarkSlateGray", "SaddleBrown", "Sienna", "DarkKhaki", "Goldenrod", "DarkGoldenrod", "FireBrick", "DarkRed", "RosyBrown", "DarkMagenta", "DarkOrchid", "DeepSkyBlue" };

    private double alpha = 1.0;
    private double alphaMin = 0.001;
    private double alphaDecay = 1;
    private double alphaTarget = 0;

    private double velocityDecay = 0.6;
    private double springFactor = 0.1;
    private double chargeFactor = 0.1;



    private List<FoLayoutLink<U,V>> _links = new();
    private List<FoLayoutNode<V>> _nodes = new();

    public FoLayoutNetwork()
    {
        alphaDecay = 1 - Math.Pow(alphaMin, 1 / 300);
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

    private void ApplyCenterForces(List<FoLayoutNode<V>> allNodes)
    {
        var strength = 0.1;

        // Calculate center of mass
        var sx = 0.0;
        var sy = 0.0;

        var cx = Boundary.X + Boundary.Width / 2;
        var cy = Boundary.Y + Boundary.Height / 2;

        foreach (var p in allNodes)
        {
            sx += p.X;
            sy += p.Y;
        }

        sx = (sx / allNodes.Count - cx) * strength;
        sy = (sy / allNodes.Count - cy) * strength;

        foreach (var node in allNodes)
        {
            node.X -= sx; 
            node.Y -= sy;
        }
    }

    public void ApplyBoundaryForces(List<FoLayoutNode<V>> allNodes)
    {

        foreach(var node in allNodes)
        {
            // Check if node is outside x bounds
            node.X = Math.Clamp(node.X, Boundary.X, Boundary.X + Boundary.Width);
            node.Y = Math.Clamp(node.Y, Boundary.Y, Boundary.Y + Boundary.Height);
        }
    }

 

 
    private void  ApplyLocationToShape(List<FoLayoutNode<V>> allNodes, int tick) 
    {
        foreach (var node in allNodes)
        {
            var shape = node.GetShape();
            $"{tick} Node {shape.Name} - X: {node.X}, Y: {node.Y}   {shape.PinX}  {shape.PinY}".WriteInfo();
            node.MoveTo((int)node.X, (int)node.Y);
        }
    }

    private void ApplyCoulombForces(FoLayoutNode<V> node, List<FoLayoutNode<V>> allNodes)
    {
        foreach (var otherNode in allNodes)
        {
            if (node != otherNode)
            {
                double distance = CalculateDistance(node, otherNode);
                double force = chargeFactor * chargeFactor / distance;

                double dx = otherNode.X - node.X;
                double dy = otherNode.Y - node.Y;
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
            
        double distance = CalculateDistance(sink, source);
        double force = distance * distance / springFactor;

        double dx = sink.X - source.X;
        double dy = sink.Y - source.Y;
        double fx = dx / distance * force;
        double fy = dy / distance * force;
        source.Dx -= fx / source.Mass;
        source.Dy -= fy / source.Mass;
        sink.Dx -= fx / sink.Mass;
        sink.Dy -= fy / sink.Mass;
    }

    public void DoLayoutStep(int tick)
    {
        if (_nodes.Count == 0) 
            return;

        if (alpha < alphaMin)
            return;

        $"{tick} alpha {alpha} {alphaMin}  decay {alphaDecay}".WriteNote();

        foreach (var node in _nodes)
            node.Dx = node.Dy = 0;

        //foreach (var node in _nodes)
        //    ApplyCoulombForces(node, _nodes);
        
        foreach (var link in _links)
            ApplyHookesLaw(link);

        foreach (var node in _nodes)
        {
            // Damping factor helps to stabilize the layout
            node.X += node.Dx * velocityDecay;
            node.Y += node.Dy * velocityDecay;
        }

        alpha += (alphaTarget - alpha) * alphaDecay;

        ApplyCenterForces(_nodes);
        ApplyBoundaryForces(_nodes);

        ApplyLocationToShape(_nodes, tick);
    }



}
