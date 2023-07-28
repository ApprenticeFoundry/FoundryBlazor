
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

    private double alphaDamping = 0.9;

    private double velocityDecay = 0.6;
    private double springFactor = 0.1;
    private double chargeFactor = 0.1;
     
    private int iterations = 0;
    private int maxIterations = 0;


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

    private void ApplyCenterForces()
    {
        var strength = 0.1;

        // Calculate center of mass
        var sx = 0.0;
        var sy = 0.0;

        var cx = Boundary.X + Boundary.Width / 2;
        var cy = Boundary.Y + Boundary.Height / 2;

        foreach (var p in _nodes)
        {
            sx += p.X;
            sy += p.Y;
        }

        sx = (sx / _nodes.Count - cx) * strength;
        sy = (sy / _nodes.Count - cy) * strength;

        foreach (var node in _nodes)
        {
            node.X -= sx; 
            node.Y -= sy;
        }
    }

    public void ApplyBoundaryForces()
    {

        foreach(var node in _nodes)
        {
            // Check if node is outside x bounds
            node.X = Math.Clamp(node.X, Boundary.X, Boundary.X + Boundary.Width);
            node.Y = Math.Clamp(node.Y, Boundary.Y, Boundary.Y + Boundary.Height);
        }
    }

 

 
    private void  ApplyLocationToShape(int tick) 
    {
        foreach (var node in _nodes)
        {
            var shape = node.GetShape();
            $"{tick} Node {shape.Name} - X: {node.X}, Y: {node.Y}   {shape.PinX}  {shape.PinY}".WriteInfo();
            node.MoveTo((int)node.X, (int)node.Y);
        }
    }

  private void ApplySpringForces()
    {
        double springConstant = 0.1;

        foreach (var edge in _links)
        {
            var sourceNode = edge.GetSource();
            var targetNode = edge.GetSink();

            var dx = targetNode.X - sourceNode.X;
            var dy = targetNode.Y - sourceNode.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            var force = (distance - springConstant) / distance;

            var fx = force * dx;
            var fy = force * dy;

            sourceNode.ApplyForce(fx, fy);
            targetNode.ApplyForce(-fx, -fy);
        }
    }

    private void ResetNodes()
    {
        foreach (var item in _nodes)
        {
            item.Dx = 0;
            item.Dy = 0;
        }
    }
    private void UpdatePositions(double alpha)
    {
        foreach (var node in _nodes)
            node.UpdatePosition(alpha);
    }

    private void ApplyRepellingForces()
    {
        double repellingForce = 50.0;

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                var nodeA = _nodes[i];
                var nodeB = _nodes[j];

                var dx = nodeB.X - nodeA.X;
                var dy = nodeB.Y - nodeA.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > 0)
                {
                    var force = repellingForce / (distance * distance);

                    var fx = force * dx;
                    var fy = force * dy;

                    nodeA.ApplyForce(-fx, -fy);
                    nodeB.ApplyForce(fx, fy);
                }
            }
        }
    }

    public void DoIteration(int count)
    {
        iterations = 0;
        maxIterations = count;
    }

    public void DoLayoutStep(int tick)
    {

        if (_nodes.Count == 0) 
            return;

        iterations++;
        if (iterations > maxIterations)
            return;

        $"{tick} alpha {alpha} {iterations}".WriteNote();


        // Reset forces
        ResetNodes();

        // Apply forces
        ApplySpringForces();
        ApplyRepellingForces();

        // Update node positions
        UpdatePositions(alpha);


        ApplyCenterForces();
        ApplyBoundaryForces();

        ApplyLocationToShape(tick);
        
        // Reduce the simulation's effect (alpha damping)
        alpha *= alphaDamping;
        
    }

 


}
