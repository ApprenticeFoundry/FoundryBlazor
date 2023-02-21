
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Models;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public static class LayoutRules
{
    public static List<BoxLayoutStyle> ProcessLayout { get; set; } = new()
    {
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.HorizontalStacked,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
    };

    public static List<BoxLayoutStyle> VertLayout { get; set; } = new()
     {
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.VerticalStacked,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
    };

    public static List<LineLayoutStyle> LineLayout { get; set; } = new()
    {
        LineLayoutStyle.Straight,
        LineLayoutStyle.Straight,
        LineLayoutStyle.Straight,
        LineLayoutStyle.Straight,
        LineLayoutStyle.VerticalFirst,
    };
};

public class FoLayoutTree<V> where V : FoGlyph2D
{
    private readonly V _item;
    public int level = -1;
    public int index = -1;
    public string path = "";


    private Size _branchSize = new(10, 10);
    private Point _branchULPoint = new(100, 100);

    private FoLayoutTree<V>? _parent;
    private List<FoLayoutTree<V>>? _children;

    public FoLayoutTree(V node)
    {
        _item = node;
        this.level = 0;
        this.index = 0;
    }



    public async Task RenderTree(Canvas2DContext ctx)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);

        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(4);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });
        await ctx.SetStrokeStyleAsync("Red");

        var rect = new Rectangle(_branchULPoint, _branchSize);
        await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);

        _children?.ForEach(async item => await item.RenderTree(ctx));

        await ctx.RestoreAsync();
    }

    public void SetParent(FoLayoutTree<V> node)
    {
        _parent = node;
    }

    public string ComputeName()
    {
        if (_parent == null) return "01";

        path = index.ToString().PadLeft(2, '0');
        if (_parent != null)
        {
            path = $"{_parent.ComputePath()}.{path}";
        }
        return path;
    }

    public string ComputePath()
    {
        if (!string.IsNullOrWhiteSpace(path)) return path;

        path = index.ToString().PadLeft(2, '0');
        if (_parent != null)
        {
            path = $"{_parent.ComputePath()}.{path}";
        }
        return path;
    }
    public V GetShape()
    {
        return _item;
    }


    public FoLayoutTree<V>? FindNodeWithName(string name)
    {

        if ( _item.Name == name) return this;
        
        if ( _children != null) 
            foreach (var child in _children)
            {
                var found = child.FindNodeWithName(name);
                if ( found != null) return found;
            }

        return null;
    }

    public FoLayoutTree<V> FindRoot(FoLayoutTree<V> node)
    {
        var found = node;
        while ( found._parent != null)
            found = found._parent;

        return found;
    }

    public List<V>? GetChildShapes()
    {
        var list = _children?.Select(item => item.GetShape()).ToList();
        return list;
    }

    public List<FoLayoutTree<V>>? GetChildren()
    {
        var list = _children?.ToList();
        return list;
    }

    public void ConnectParentChildShapeTree<U>(IPageManagement pageManager, List<LineLayoutStyle> styleList) where U : FoConnector1D
    {
        var parent = this.GetShape();

        if (this.level >= styleList.Count) return;
        var style = styleList[this.level];
        // $"{style} for level {level} ConnectParentChild".WriteLine(ConsoleColor.DarkBlue);

        this.GetChildren()?.ForEach(child =>
        {
            var shape = child.GetShape();
            //shape.Tag = $"Node: {child.ComputeName()}";

            var shape1D = Activator.CreateInstance<U>();
            shape1D.Layout = style;
            shape1D.Thickness = 5;
            shape1D.Color = "Green";

            pageManager.Add<U>(shape1D);

            shape1D.GlueStartTo(parent);
            shape1D.GlueFinishTo(shape);
            child.ConnectParentChildShapeTree<U>(pageManager, styleList);
        });

    }

    public void HorizontalLayoutConnections<U>(IPageManagement pageManager) where U : FoConnector1D
    {
        this.ConnectParentChildShapeTree<U>(pageManager, LayoutRules.LineLayout);
    }

    public void HorizontalLayout(int PinX, int PinY, Point margin, List<BoxLayoutStyle> rules)
    {
        var point = new Point(PinX, PinY);
        this.ComputeNodeBranchSize(margin, rules);
        this.ComputeNodeBranchLocation(point, margin, rules);
    }

    public void VerticalLayout(int PinX, int PinY, Point margin, List<BoxLayoutStyle> rules)
    {
        var point = new Point(PinX, PinY);
        this.ComputeNodeBranchSize(margin, rules);
        this.ComputeNodeBranchLocation(point, margin, rules);
    }



    public void ComputeNodeBranchSize(Point margin, List<BoxLayoutStyle> styleList)
    {
        if (!_item.IsVisible)
        {
            _branchSize = new Size(0, 0);
            return;
        }

        if (this.level >= styleList.Count) return;

        var style = styleList[this.level];
        //$"{style} for level {level} BranchSize".WriteLine(ConsoleColor.DarkBlue);

        var width = _item.Width + margin.X;
        var height = _item.Height + margin.Y;

        if (_children == null || _children.Count == 0)
        {
            _branchSize = new Size(width, height);
        }
        else
        {
            _children.ForEach(child => child.ComputeNodeBranchSize(margin, styleList));

            //now we calculate the full box based for node and children

            if (style == BoxLayoutStyle.Horizontal)
            {
                width = _children.Select(item => item._branchSize.Width).Sum();
                height = _children.Select(item => item._branchSize.Height).Max();
                width = Math.Max(width, _item.Width + margin.X);
                height = height + _item.Height + margin.Y;
            }
            else if (style == BoxLayoutStyle.Vertical)
            {
                width = _children.Select(item => item._branchSize.Width).Max();
                height = _children.Select(item => item._branchSize.Height).Sum();
                width = width + _item.Width + margin.X;
                height = Math.Max(height, _item.Height + margin.Y);
            }
            else if (style == BoxLayoutStyle.HorizontalStacked)
            {
                width = _children.Select(item => item._branchSize.Width).Max();
                height = _children.Select(item => item._branchSize.Height).Sum();
                width = width + _item.Width + margin.X;
                height = Math.Max(height, _item.Height + margin.Y);
            }
            _branchSize = new Size(width, height);
        }

        //$"NodeBranchSize {level}.{index}  W:{_branchSize.Width}  H:{_branchSize.Height}".WriteLine(ConsoleColor.DarkCyan);

    }

    public void ComputeNodeBranchLocation(Point pt, Point margin, List<BoxLayoutStyle> styleList)
    {
        if (!_item.IsVisible) return;

        if (this.level >= styleList.Count) return;

        var style = styleList[this.level];
        //$"{style} for level {level}  BranchLocation".WriteLine(ConsoleColor.DarkBlue);
        //this is a top down process since we can place the _branchSize for eact node
        //it contains the child nodes inside 

        Point _branchULPoint;
        int halfWidth = 0;
        //int halfHeight = 0;

        int stepX = 0;
        int stepY = 0;

        int leftEdgeX = 0;
        int topEdgeY = 0;

        var shape = GetShape();

        float delay = (float)((level + index / 10.0) / 2.0);
        if (style == BoxLayoutStyle.Horizontal)
        {
            //assume that for horizontal the Pt is center top
            topEdgeY = shape.Height + margin.Y;
            halfWidth = _branchSize.Width / 2;
            stepX = pt.X - halfWidth;
            _branchULPoint = new Point(stepX, pt.Y);
            // shape.AnimatedMoveTo(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape), 2.0F, delay);
            shape.MoveTo(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape));
        }
        else if (style == BoxLayoutStyle.Vertical)
        {
        }
        else if (style == BoxLayoutStyle.HorizontalStacked)
        {
            //assume that for horizontal the Pt is center top
            topEdgeY = shape.Height + margin.Y;
            leftEdgeX = 3 * shape.Height / 2;
            halfWidth = _branchSize.Width / 2;
            stepX = pt.X - halfWidth;
            stepY = pt.Y + margin.Y;
            _branchULPoint = new Point(stepX, pt.Y);
            // shape.AnimatedMoveTo(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape), 2.0F, delay);
            shape.MoveTo(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape));
        }



        _children?.ForEach(child =>
        {
            //for horizontal compute the center of this child 
            //and add it to the full width of the children before us

            if (style == BoxLayoutStyle.Horizontal)
            {
                var halfchild = child._branchSize.Width / 2;
                stepX += halfchild;
                var childPt = new Point(stepX, pt.Y + topEdgeY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepX += halfchild;  //move to the other side
            }
            else if (style == BoxLayoutStyle.Vertical)
            {
                var halfchild = child._branchSize.Height / 2;
                stepY += halfchild;
                var childPt = new Point(pt.X + leftEdgeX, stepY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepY += halfchild;  //move to the other side
            }
            else if (style == BoxLayoutStyle.HorizontalStacked)
            {
                var halfchild = child._branchSize.Height / 2;
                stepY += halfchild;
                var childPt = new Point(pt.X + leftEdgeX, stepY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepY += halfchild;  //move to the other side
            }

        });

    }

    // public void NodeCenterChildren(LayoutStyle style)
    // {
    //     if ( !_item.IsVisible) return;

    //     var width = _branchSize.Width / 2;
    //     _children?.ForEach(child => 
    //     {
    //         var shape = child.GetShape();
    //         shape.MoveBy(-width, 0);
    //         child.NodeCenterChildren(style);
    //     });   
    // }

    public FoLayoutTree<V>? AddChildNode(FoLayoutTree<V>? child)
    {

        if ( child != null)
        {
            this._children ??= new List<FoLayoutTree<V>>();
            this._children.Add(child);
            child._parent = this;
            child.level = level + 1;
            child.index = this._children.Count;
           //var shape = child.GetShape();
            //var tag = shape.Tag;
            // $"Adding Shape {tag} {shape.Name}".WriteLine(ConsoleColor.Green);
        } else 
        {
            $"Child is empty {child}".WriteLine(ConsoleColor.Green);
        }
        return child;
    }

    public List<FoLayoutTree<V>> CollectLeafNodes(List<FoLayoutTree<V>> list)
    {
        if (_children == null || _children.Count == 0)
            list.Add(this);
        else
            _children?.ForEach(child => child.CollectLeafNodes(list));

        return list;
    }


    public List<FoLayoutTree<V>> CollectAllNodes(List<FoLayoutTree<V>> list)
    {
        list.Add(this);
        _children?.ForEach(child => child.CollectAllNodes(list));

        return list;
    }

    public List<V> CollectAllShapes(List<V> list)
    {
        list.Add(this._item);
        _children?.ForEach(child => child.CollectAllShapes(list));

        return list;
    }


}
