
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shape;


public class FoLayoutGroup<V,U> : ICanHitTarget where V : FoGlyph2D where U : FoGlyph2D
{

    public double X { get; set; } = 110.0;
    public double Y { get; set; } = 110.0;
    public double Radius { get; set; } = 50.0;
    public double Mass { get; } = 1.0;
    public double Dx { get; set; } = 0.0;
    public double Dy { get; set; } = 0.0;

    private V _item;
    private List<FoLayoutNode<U>> _members;

    public FoLayoutGroup(V node, int x, int y)
    {
        _item = node;
        _members = new();
        MoveTo(x, y);
    }

    public string GetName()
    {
        return _item.Name ?? "No Name";
    }

    public void PurgeMembers()
    {
        _members = new();
    }

    public string GroupName()
    {
        return _item.Key;
    }
    
    public string SetGroupName(string name)
    {
        _item.Key = name;
        return _item.Key;
    }

    public List<FoLayoutNode<U>> GetMembers()
    {
        _members ??= new List<FoLayoutNode<U>>();
        var list = _members.ToList();
        return list;
    }

    public FoLayoutNode<U>? AddMemberNode(FoLayoutNode<U> member)
    {

        if ( member != null)
        {
            this._members ??= new List<FoLayoutNode<U>>();
            this._members.Add(member);
           //var shape = child.GetShape();
            //var tag = shape.Tag;
            // $"Adding Shape {tag} {shape.Name}".WriteLine(ConsoleColor.Green);
        } else 
        {
            $"Member is empty {member}".WriteSuccess();
        }
        return member;
    }
    public void HorizontalLayout(int PinX, int PinY, Point margin)
    {
        var point = new Point(PinX, PinY);
        //this.ComputeNodeBranchSize(margin, TreeLayoutRules.HorizontalLayout);
        //this.ComputeNodeBranchLocation(point, margin, TreeLayoutRules.HorizontalLayout);
    }

    public void VerticalLayout(int PinX, int PinY, Point margin)
    {
        var point = new Point(PinX, PinY);
        //this.ComputeNodeBranchSize(margin, TreeLayoutRules.VerticalLayout);
        //this.ComputeNodeBranchLocation(point, margin, TreeLayoutRules.VerticalLayout);
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

    public Point[] HitTestSegment()
    {
        throw new NotImplementedException();
    }
}
