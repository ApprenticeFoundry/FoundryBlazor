
using Blazor.Extensions.Canvas.Canvas2D;

using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutSwimLanes<U,V> where V : FoShape2D where U : FoShape1D
{

    public Rectangle Boundary = new (100, 100, 700, 700);
     
    private int iterations = 0;
    private int maxIterations = 0;


    private readonly List<FoLayoutLink<U,V>> _links = new();
    private readonly List<FoLayoutNode<V>> _nodes = new();


    public FoLayoutSwimLanes()
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
  
        await ctx.RestoreAsync();
    }


 
    private void  ApplyLocationToShape(int tick) 
    {
        //"ApplyLocationToShape".WriteInfo();
        foreach (var node in _nodes)
        {
            node.MoveTo((int)node.X, (int)node.Y);
        }
    }


    private void Reset()
    {
       // "Reset".WriteInfo();
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
        //"UpdatePositions".WriteInfo();
        foreach (var node in _nodes)
            node.UpdatePositionUsingForceValues(alpha);
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

        if (iterations >= maxIterations)
            return;

        iterations++;

        // Reset forces
        ResetNodes();


        // Update node positions
        var alpha = 10 * (1 - iterations / maxIterations);
        $"{tick} alpha {alpha} {iterations}".WriteNote();
        UpdatePositions(alpha);


        ApplyLocationToShape(tick);
    }

    public void VerticalLayout()
    {
        var targets = _nodes.OrderBy(t => t.GetShape().PinY).ToList();
    }
}
