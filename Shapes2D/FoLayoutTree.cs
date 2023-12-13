
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shape;

public static class TreeLayoutRules
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

    public static List<BoxLayoutStyle> HorizontalLayout { get; set; } = new()
     {
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
        BoxLayoutStyle.Horizontal,
    };
    public static List<BoxLayoutStyle> VerticalLayout { get; set; } = new()
     {
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
        BoxLayoutStyle.Vertical,
    };

    public static List<LineLayoutStyle> VerticalLineLayout { get; set; } = new()
    {
        LineLayoutStyle.VerticalFirst,
        LineLayoutStyle.VerticalFirst,
        LineLayoutStyle.VerticalFirst,
        LineLayoutStyle.VerticalFirst,
        LineLayoutStyle.VerticalFirst,
    };
    public static List<LineLayoutStyle> LineLayout { get; set; } = new()
    {
        LineLayoutStyle.None,
        LineLayoutStyle.None,
        LineLayoutStyle.None,
        LineLayoutStyle.None,
        LineLayoutStyle.None,
        LineLayoutStyle.None,
        LineLayoutStyle.None,
    };
};

public class FoLayoutTree<V> where V : FoGlyph2D
{
    public int level = 0;
    public int index = 0;
    public string path = "";

    public bool IsExpanded = true;
    public bool IsVisited = true;   
    private Size _branchSize = new(10, 10);
    private Point _branchULPoint = new(100, 100);
    private BoxLayoutStyle _layoutStyle = BoxLayoutStyle.None;
    private readonly string[] Colors = new string[] { "Red", "White", "Purple", "Green", "Grey", "Purple", "Pink", "Brown", "Grey", "Black", "White", "Crimson", "Indigo", "Violet", "Magenta", "Turquoise", "Teal", "SlateGray", "DarkSlateGray", "SaddleBrown", "Sienna", "DarkKhaki", "Goldenrod", "DarkGoldenrod", "FireBrick", "DarkRed", "RosyBrown", "DarkMagenta", "DarkOrchid", "DeepSkyBlue" };


    private V _item;
    private FoLayoutTree<V>? _parent;
    private List<FoLayoutTree<V>>? _children;

    public FoLayoutTree(V node)
    {
        _item = node;
        this.level = 0;
        this.index = 0;
    }

    public void ClearVisited()
    {
        IsVisited = false;
    }
    public void MarkVisited()
    {
        IsVisited = true;
    }

    public void ClearAll()
    {
        _item = null!;
        _parent = null;
        path = "";
        _children?.ForEach(item => item.ClearAll());
        _children?.Clear();
        _children = null;
    }

    public string GetGlyphId()
    {
        return _item.GetGlyphId();
    }

    public async Task RenderLayoutTree(Canvas2DContext ctx, int tick)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);

        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(4);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });
        await ctx.SetStrokeStyleAsync(Colors[level]);

        var rect = new Rectangle(_branchULPoint, _branchSize);
        rect.Inflate(5 * level, 5 * level);
        await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);

        _children?.ForEach(async item => await item.RenderLayoutTree(ctx, tick));

        await ctx.RestoreAsync();
    }

    public FoLayoutTree<V>? GetParent()
    {
        return _parent;
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
            path = $"{_parent.ComputeName()}.{path}";
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
        if (string.IsNullOrEmpty(name)) return null;

        if (_item.Key == name) return this;

        if (_children != null)
            foreach (var child in _children)
            {
                var found = child.FindNodeWithName(name);
                if (found != null) return found;
            }

        return null;
    }

    public FoLayoutTree<V>? FindNodeWithGuid(string guid)
    {
        if (GetGlyphId() == guid) return this;

        if (_children != null)
            foreach (var child in _children)
            {
                var found = child.FindNodeWithGuid(guid);
                if (found != null) return found;
            }

        return null;
    }

    public void RemoveChild(FoLayoutTree<V> target)
    {
        if (_children == null) return;
        var found = _children.Find((item) => item == target);
        if (found != null)
        {
            found._parent = null;
            _children.Remove(found);
        }
    }

    public FoLayoutTree<V>? PurgeChildren()
    {
        _children?.ForEach(item =>
        {
            item._parent = null;
            item.PurgeChildren();
        });
        _children = null;
        return this;
    }

    public void VisitAllNodesInTree()
    {
        if (IsVisited) return;
        
        MarkVisited();
        _children?.ForEach(item => item.VisitAllNodesInTree());
    }

    public FoLayoutTree<V> FindRoot()
    {
        if (_parent == null) return this;
        return _parent.FindRoot();
    }

    public static FoLayoutTree<V> FindRootOf(FoLayoutTree<V> node)
    {
        var found = node;
        while (found._parent != null)
            found = found._parent;

        return found;
    }

    public List<V>? GetChildShapes(bool deep = true)
    {
        var list = _children?.Select(item => item.GetShape()).ToList();
        if (deep)
        {
            _children?.ForEach(item =>
            {
                var childList = item.GetChildShapes(deep);
                if (childList != null)
                    list?.AddRange(childList);
            });
        }
        return list;
    }

    public List<FoLayoutTree<V>>? GetChildren()
    {
        var list = _children?.ToList();
        return list;
    }

    public void ConnectParentChildShapeTree<U>(IPageManagement pageManager, string glueStart, List<LineLayoutStyle> styleList) where U : FoConnector1D
    {
        var parent = this.GetShape();
        parent.GetConnectionPoints();


        if (this.level >= styleList.Count) return;
        var style = styleList[this.level];
        // $"{style} for level {level} ConnectParentChild".WriteLine(ConsoleColor.DarkBlue);

        this.GetChildren()?.ForEach(child =>
        {
            var shape = child.GetShape();
            shape.GetConnectionPoints();
            //shape.Tag = $"Node: {child.ComputeName()}";

            var shape1D = Activator.CreateInstance<U>();
            if (shape1D.Layout == LineLayoutStyle.None)
                shape1D.Layout = style;

            pageManager.AddShape<U>(shape1D);


            if (glueStart.Matches("TOP") || glueStart.Matches("BOTTOM"))
            {
                if (FoShape1D.HasNoGlue(shape, "TOP"))
                {
                    shape1D.GlueStartTo(parent, "BOTTOM");
                    shape1D.GlueFinishTo(shape, "TOP");
                }
            }
            else if (glueStart.Matches("LEFT") || glueStart.Matches("RIGHT"))
            {
                if (FoShape1D.HasNoGlue(shape, "LEFT"))
                {
                    shape1D.GlueStartTo(parent, "RIGHT");
                    shape1D.GlueFinishTo(shape, "LEFT");
                }
            }
            else
            {
                shape1D.GlueStartTo(parent);
                shape1D.GlueFinishTo(shape);
            }

            child.ConnectParentChildShapeTree<U>(pageManager, glueStart, styleList);
        });

    }
    public void LayoutConnections<U>(IPageManagement pageManager, string glueStart, List<LineLayoutStyle> rules) where U : FoConnector1D
    {
        this.ConnectParentChildShapeTree<U>(pageManager, glueStart, rules);
    }

    public void HorizontalLayoutConnections<U>(IPageManagement pageManager) where U : FoConnector1D
    {
        this.ConnectParentChildShapeTree<U>(pageManager, "BOTTOM", TreeLayoutRules.LineLayout);
    }

    public void VerticalLayoutConnections<U>(IPageManagement pageManager) where U : FoConnector1D
    {
        this.ConnectParentChildShapeTree<U>(pageManager, "RIGHT", TreeLayoutRules.LineLayout);
    }

    public void HorizontalLayout(int PinX, int PinY, Point margin)
    {
        var point = new Point(PinX, PinY);
        this.ComputeNodeBranchSize(margin, TreeLayoutRules.HorizontalLayout);
        this.ComputeNodeBranchLocation(point, margin, TreeLayoutRules.HorizontalLayout);
    }

    public void VerticalLayout(int PinX, int PinY, Point margin)
    {
        var point = new Point(PinX, PinY);
        this.ComputeNodeBranchSize(margin, TreeLayoutRules.VerticalLayout);
        this.ComputeNodeBranchLocation(point, margin, TreeLayoutRules.VerticalLayout);
    }

    public void Layout(int PinX, int PinY, Point margin, List<BoxLayoutStyle>? rules = null)
    {
        var point = new Point(PinX, PinY);
        this.ComputeNodeBranchSize(margin, rules);
        this.ComputeNodeBranchLocation(point, margin, rules);
    }

    public void ComputeNodeBranchSize(Point margin, List<BoxLayoutStyle>? styleList = null)
    {
        if (!_item.IsVisible)
        {
            _branchSize = new Size(0, 0);
            return;
        }

        if (styleList != null)
        {
            if (this.level >= styleList.Count) return;
            _layoutStyle = styleList[this.level];
        }
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

            if (_layoutStyle == BoxLayoutStyle.Horizontal)
            {
                width = _children.Select(item => item._branchSize.Width).Sum();
                height = _children.Select(item => item._branchSize.Height).Max();
                width = Math.Max(width, _item.Width + margin.X);
                height = height + _item.Height + margin.Y;
            }
            else if (_layoutStyle == BoxLayoutStyle.Vertical)
            {
                width = _children.Select(item => item._branchSize.Width).Max();
                height = _children.Select(item => item._branchSize.Height).Sum();
                width = width + _item.Width + margin.X;
                height = Math.Max(height, _item.Height + margin.Y);
            }
            else if (_layoutStyle == BoxLayoutStyle.HorizontalStacked)
            {
                width = _children.Select(item => item._branchSize.Width).Max();
                height = _children.Select(item => item._branchSize.Height).Sum();
                width = width + _item.Width + margin.X;
                height = Math.Max(height, _item.Height + margin.Y);
            }
            else if (_layoutStyle == BoxLayoutStyle.VerticalStacked)
            {
                width = _children.Select(item => item._branchSize.Width).Sum();
                height = _children.Select(item => item._branchSize.Height).Max();
                width = Math.Max(width, _item.Width + margin.X);
                height = height + _item.Height + margin.Y;
            }
            _branchSize = new Size(width, height);
        }

        //$"NodeBranchSize {level}.{index}  W:{_branchSize.Width}  H:{_branchSize.Height}".WriteLine(ConsoleColor.DarkCyan);

    }

    private static Point Relocate(Point pt, V shape)
    {
        //return new Point(pt.X, pt.Y);
        //return new Point(pt.X , pt.Y + shape.LocPinY(shape));
        return new Point(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape));
    }

    public void ComputeNodeBranchLocation(Point pt, Point margin, List<BoxLayoutStyle>? styleList = null)
    {
        if (!_item.IsVisible) return;

        if (styleList != null)
        {
            if (this.level >= styleList.Count) return;
            _layoutStyle = styleList[this.level];
        }

        //$"{style} for level {level}  BranchLocation".WriteLine(ConsoleColor.DarkBlue);
        //this is a top down process since we can place the _branchSize for eact node
        //it contains the child nodes inside 


        int halfWidth = 0;
        int halfHeight = 0;

        int stepX = 0;
        int stepY = 0;

        int leftEdgeX = 0;
        int topEdgeY = 0;

        var shape = GetShape();
        var loc = FoLayoutTree<V>.Relocate(pt, shape);

        float delay = (float)((level + index / 10.0) / 2.0);

        if (_layoutStyle == BoxLayoutStyle.Horizontal)
        {
            //assume that for horizontal the Pt is center top
            topEdgeY = shape.Height + margin.Y;
            halfWidth = _branchSize.Width / 2;
            stepX = pt.X - halfWidth;
            _branchULPoint = new Point(stepX, pt.Y);
            shape.MoveTo(loc.X - shape.LocPinX(shape), loc.Y);
        }
        else if (_layoutStyle == BoxLayoutStyle.Vertical)
        {
            //assume that for vertical the Pt is center left
            leftEdgeX = shape.Width + margin.X;
            halfHeight = _branchSize.Height / 2;
            stepY = pt.Y - halfHeight;
            _branchULPoint = new Point(pt.X, stepY);
            shape.MoveTo(loc.X, loc.Y - shape.LocPinY(shape));
        }
        else if (_layoutStyle == BoxLayoutStyle.HorizontalStacked)
        {
            //assume that for horizontal the Pt is center top
            topEdgeY = shape.Height + margin.Y;
            leftEdgeX = 3 * shape.Height / 2;
            halfWidth = _branchSize.Width / 2;
            stepX = pt.X - halfWidth;
            stepY = pt.Y + margin.Y;
            _branchULPoint = new Point(stepX, pt.Y);
            shape.MoveTo(loc.X - shape.LocPinX(shape), loc.Y);
        }
        else if (_layoutStyle == BoxLayoutStyle.VerticalStacked)
        {
            //assume that for horizontal the Pt is center top
            topEdgeY = 3 * shape.Width / 2;
            leftEdgeX = shape.Width + margin.X;
            halfHeight = _branchSize.Height / 2;
            stepX = pt.X + margin.X;
            stepY = pt.Y - halfHeight;
            _branchULPoint = new Point(pt.X, stepY);
            shape.MoveTo(loc.X, loc.Y - shape.LocPinY(shape));
        }

        // shape.AnimatedMoveTo(pt.X + shape.LocPinX(shape), pt.Y + shape.LocPinY(shape), 2.0F, delay);


        _children?.ForEach(child =>
        {
            //for horizontal compute the center of this child 
            //and add it to the full width of the children before us

            if (_layoutStyle == BoxLayoutStyle.Horizontal)
            {
                var halfchild = child._branchSize.Width / 2;
                stepX += halfchild;
                var childPt = new Point(stepX, pt.Y + topEdgeY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepX += halfchild;  //move to the other side
            }
            else if (_layoutStyle == BoxLayoutStyle.Vertical)
            {
                var halfchild = child._branchSize.Height / 2;
                stepY += halfchild;
                var childPt = new Point(pt.X + leftEdgeX, stepY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepY += halfchild;  //move to the other side
            }
            else if (_layoutStyle == BoxLayoutStyle.HorizontalStacked)
            {
                var halfchild = child._branchSize.Height / 2;
                stepY += halfchild;
                var childPt = new Point(pt.X + leftEdgeX, stepY);
                child.ComputeNodeBranchLocation(childPt, margin, styleList);
                stepY += halfchild;  //move to the other side
            }

        });

    }



    public FoLayoutTree<V>? AddChildNode(FoLayoutTree<V>? child)
    {

        if (child != null)
        {
            this._children ??= new List<FoLayoutTree<V>>();
            if ( _children.Contains(child)) 
                return child;

            this._children.Add(child);
            child._parent = this;
            child.level = level + 1;
            child.index = this._children.Count;
            //var shape = child.GetShape();
            //var tag = shape.Tag;
            // $"Adding Shape {tag} {shape.Name}".WriteLine(ConsoleColor.Green);
        }
        else
        {
            $"Child is empty {child}".WriteSuccess();
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
