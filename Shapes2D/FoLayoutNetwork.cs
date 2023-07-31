
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


    private double alpha = 1.0;

    private double alphaDamping = 0.9;

    private double RepellingForce = 500000.0;
    private double AttractionForce = 10000.0;
    private double CenterForce = 1.0;
     
    private int iterations = 0;
    private int maxIterations = 0;


    private readonly List<FoLayoutLink<U,V>> _links = new();
    private readonly List<FoLayoutNode<V>> _nodes = new();


    public FoLayoutNetwork()
    {
    }

    public List<FoLayoutLink<U,V>> GetLinks()
    {
        return _links;
    }
    public List<FoLayoutNode<V>> GetNodes()
    {
        return _nodes;
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

    public int XCenter()
    {
        return Boundary.X + Boundary.Width / 2;
    }
    public int YCenter()
    {
        return Boundary.Y + Boundary.Height / 2;
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

        var offsetY = 300;
        var offsetX = 1000;

        await ctx.SetFontAsync("18px consolas");
        await ctx.FillTextAsync($"Center X: {XCenter()} Y: {YCenter()}", offsetX, offsetY);
        await ctx.FillTextAsync($"Repelling {RepellingForce:0.00} Attraction {AttractionForce:0.00} alpha {alpha}", offsetX, offsetY + 25);

        await ctx.RestoreAsync();
    }

    public void DoRandomRule()
    {
        ResetNodes();
        ApplyRandomForces();
        ApplyLocationToShape(0);
    }

    public void DoBoundryRule()
    {
        ResetNodes();
        ApplyBoundaryForces();
        ApplyLocationToShape(0);
    }

    public void DoCenterRule()
    {
        ResetNodes();
        ApplyCenterForces();
        ApplyLocationToShape(0);
    }
    public void DoAttractRule()
    {
        ResetNodes();
        // Apply forces
        ApplySpringForces();

        // Update node positions
        UpdatePositions(1);
        ApplyLocationToShape(0);
    }
    public void DoRepellRule()
    {
        ResetNodes();

        // Apply forces
        ApplyRepellingForces();

        // Update node positions
        UpdatePositions(1);
        ApplyLocationToShape(0);
    }

    private void ApplyCenterForces()
    {
        "ApplyCenterForces".WriteInfo();
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

        sx = (sx / _nodes.Count - cx) * CenterForce;
        sy = (sy / _nodes.Count - cy) * CenterForce;

        foreach (var node in _nodes)
        {
            node.X -= sx; 
            node.Y -= sy;
        }
    }

    public void ApplyBoundaryForces()
    {
        "ApplyBoundaryForces".WriteInfo();
        foreach(var node in _nodes)
        {
            // Check if node is outside x bounds
            node.X = Math.Clamp(node.X, Boundary.X, Boundary.X + Boundary.Width);
            node.Y = Math.Clamp(node.Y, Boundary.Y, Boundary.Y + Boundary.Height);
        }
    }

     public void ApplyRandomForces()
    {
        "ApplyRandomForces".WriteInfo();
        var rand = new Random();
        foreach(var node in _nodes)
        {
            node.X = rand.Next(Boundary.X, Boundary.X + Boundary.Width);
            node.Y = rand.Next(Boundary.Y, Boundary.Y + Boundary.Height);
        }
    }

 
    private void  ApplyLocationToShape(int tick) 
    {
        "ApplyLocationToShape".WriteInfo();
        foreach (var node in _nodes)
        {
            var shape = node.GetShape();
           // $"{tick} Node {shape.Name} - X: {node.X}, Y: {node.Y}   {shape.PinX}  {shape.PinY}".WriteWarning();
            node.MoveTo((int)node.X, (int)node.Y);
            $"{tick} {shape.PinX}  {shape.PinY}".WriteSuccess();
        }
    }

  private void ApplySpringForces()
    {
        //"ApplySpringForces".WriteInfo();
        foreach (var edge in _links)
        {
            var sourceNode = edge.GetSource();
            var targetNode = edge.GetSink();


            var (dx, dy, distance) = edge.CalculateForceVector();

            if ( distance == 0 )
                continue;

            var force = AttractionForce / distance;

            var fx = force * dx;
            var fy = force * dy;

            //$"Sprin Forces {force} {fx} {fy}".WriteNote();

            sourceNode.ApplyForce(-fx, -fy);
            targetNode.ApplyForce(+fx, +fy);
        }
    }

    private void Reset()
    {
        "Reset".WriteInfo();
        alpha = 1.0;
        alphaDamping = 0.94;
        ResetNodes();
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
        "UpdatePositions".WriteInfo();
        foreach (var node in _nodes)
            node.UpdatePositionUsingForceValues(alpha);
    }

    private void ApplyRepellingForces()
    {
        //"ApplyRepellingForces".WriteInfo();
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = 0; j < _nodes.Count; j++)
            {
                if ( i == j ) 
                    continue;

                var nodeA = _nodes[i];
                var nodeB = _nodes[j];

                var (dx,dy,distance) = nodeB.CalculateForceVector(nodeA);

                if (distance > 0)
                {
                    var force = RepellingForce / (distance * distance);

                    var fx = force * dx;
                    var fy = force * dy;


                    //$"Repel Forces {force} {fx} {fy}".WriteNote();

                    nodeA.ApplyForce(-fx, -fy);
                    nodeB.ApplyForce(fx, fy);
                }
            }
        }
    }

    public void DoIteration(int count)
    {
        alpha = 1.0;
        iterations = 0;
        maxIterations = count;
    }

    public void DoLayoutStep(int tick)
    {

        if (_nodes.Count == 0) 
            return;

        if (iterations >= maxIterations)
            return;

        iterations++;
        if (alpha < 0.002)
            return;

        $"{tick} alpha {alpha} {iterations}".WriteNote();

        // Reset forces
        ResetNodes();

        // Apply forces
        ApplyRepellingForces();
        ApplySpringForces();


        // Update node positions
        UpdatePositions(alpha);
        // Reduce the simulation's effect (alpha damping)
        alpha *= alphaDamping;


        ApplyCenterForces();

        ApplyBoundaryForces();

        ApplyLocationToShape(tick);
    }

 


}
