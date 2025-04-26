using System.Drawing;

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Units;

namespace FoundryBlazor.Shape;

/// <summary>
/// Represents a coordinate used for grid generation
/// </summary>
public record SortedCoord
{
    public double Value { get; init; }
    public bool IsFromShape { get; init; }
    public string Source { get; init; }

    public SortedCoord(double value, bool isFromShape = false, string source = "")
    {
        Value = value;
        IsFromShape = isFromShape;
        Source = source;
    }
    
    public static implicit operator double(SortedCoord coord) => coord.Value;
}

/// <summary>
/// Represents a line segment with start and end points
/// </summary>
public record LineSegment
{
    public LineIntersection Start { get; init; }
    public LineIntersection End { get; init; }

    public LineSegment(LineIntersection start, LineIntersection end)
    {
        Start = start;
        End = end;
    }
}

public record LineIntersection
{
    public Point Center { get; init; }
    public List<LineSegment> Segments { get; init; } = new List<LineSegment>();

    public LineIntersection(Point loc)
    {
        Center = loc;
    }
}

public interface IPage2D : ITreeNode
{
    int MapToPageXScale(Length value);
    int MapToPageYScale(Length value);

    int MapToPageXLoc(Length value);
    int MapToPageYLoc(Length value);
    double MapToModelXLoc(int value);
    double MapToModelYLoc(int value);
    string CalculateTitle(string title);
    (int, int) DefaultDropLocation(double fraction=1.0);

    FoPage2D ClearAll();

    void AddAction(string name, string color, Action action);

    V AddShape<V>(V shape) where V : FoGlyph2D;
    V RemoveShape<V>(V shape) where V : FoGlyph2D;
}

public class FoPage2D : FoGlyph2D, IPage2D
{

    public PanZoomState PanZoom { get; set; } = new();

    public Length PageMargin { get; set; } = new Length(1, "cm");  //inches
    public Length PageWidth { get; set; } = new Length(50.0, "cm");  //inches
    public Length PageHeight { get; set; } = new Length(30.0, "cm"); //inches

    public Length GridMajorH { get; set; } = new Length(5.0, "cm"); //inches
    public Length GridMinorH { get; set; } = new Length(1, "cm"); //inches

    public Length GridMajorV { get; set; } = new Length(5.0, "cm"); //inches
    public Length GridMinorV { get; set; } = new Length(1, "cm"); //inches

    public int ScaleAxisX { get; set; } = 1;
    public Length ZeroPointX { get; set; } = new Length(0.0, "cm");  //cm
    public int ScaleAxisY { get; set; } = 1;
    public Length ZeroPointY { get; set; } = new Length(0.0, "cm");  //cm
    public string Title { get; set; } = string.Empty;

    public FoScale2D Scale2D { get; set; } = new FoScale2D()
    {
        Drawing = new Length(1.0, "cm"),
        World = new Length(1.0, "m")
    };

    public FoHorizontalRuler2D HRuler2D { get; set; }
    public FoVerticalRuler2D VRuler2D { get; set; }

    protected FoCollection<FoGlyph2D> Shapes1D = new();
    protected FoCollection<FoGlyph2D> Shapes2D = new();


    public override Rectangle HitTestRect()
    {
        // var pt = new Point(PinX, PinY);
        // var sz = new Size(Width, Height);
        // var result = new Rectangle(pt, sz);
        var result = new Rectangle(PinX, PinY, Width, Height);
        return result;
    }


    public FoPage2D(string name, string color) : base(name, color)
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
        HRuler2D = new FoHorizontalRuler2D(Scale2D, this);
        VRuler2D = new FoVerticalRuler2D(Scale2D, this);
    }

    public FoPage2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
        HRuler2D = new FoHorizontalRuler2D(Scale2D, this);
        VRuler2D = new FoVerticalRuler2D(Scale2D, this);
        CalculateTitle();
    }

    public FoPage2D ResetScale(Length drawing, Length world)
    {
        Scale2D.Drawing = drawing;
        Scale2D.World = world;
        HRuler2D = new FoHorizontalRuler2D(Scale2D, this);
        VRuler2D = new FoVerticalRuler2D(Scale2D, this);
        CalculateTitle();
        return this;
    }

    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        foreach (var item in Shapes1D.Values())
        {
            list.Add(item);
        }
        foreach (var item in Shapes2D.Values())
        {
            list.Add(item);
        }
        return list;
    }

    public int MapToPageXScale(Length value)
    {
        var scale = Scale2D.ScaleToDrawing();
        var pos = scale * value;
        var result = pos.AsPixels();
        $"PageXScale PW: {PageWidth} W: {value} D: {pos} [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;
    }

    public int MapToPageXLoc(Length value)
    {
        var m = PageMargin.AsPixels();
        var loc = m + MapToPageXScale(value);
        var result = m + ZeroPointX.AsPixels() + (ScaleAxisX * loc);
        $"PageXLoc PW: {PageWidth} W: {value} M: {m}  [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;
    }

    public int MapToPageYScale(Length value)
    {
        var scale = Scale2D.ScaleToDrawing();
        var pos = scale * value;
        var result = pos.AsPixels();
        $"PageYScale PH: {PageHeight} W: {value} D: {pos}  [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);
        return result;
    }

    public int MapToPageYLoc(Length value)
    {
        var m = PageMargin.AsPixels();
        var loc = MapToPageYScale(value);
        var result = m + ZeroPointY.AsPixels() + (ScaleAxisY * loc);
        $"PageYLoc {PageHeight} {PageHeight.AsPixels()} W: {value} M: {m} L: {loc} [{result} px]  {Scale2D.Display()}".WriteLine(ConsoleColor.Blue);

        return result;
    }

    public double MapToModelXLoc(int value)
    {
        var m = PageMargin.AsPixels();
        var pWidth = PageHeight.AsPixels(); //measure from the left
        var size = Scale2D.PixelToDrawing(value - m);
        var scale = Scale2D.ScaleToWorld();
        var result = size * scale;
        return result;
    }
    public double MapToModelYLoc(int value)
    {
        var pHeight = PageHeight.AsPixels(); //measure from the bottom
        var m = PageMargin.AsPixels();
        var size = Scale2D.PixelToDrawing(pHeight + m - value);
        var scale = Scale2D.ScaleToWorld();
        var result = size * scale;
        return result;
    }
    public FoPage2D SetPageLandscape()
    {
        if (PageWidth < PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
        return this;
    }
    public FoPage2D SetPagePortrait()
    {
        if (PageWidth > PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
        return this;
    }


    public FoPage2D SetPageSize(double width, double height, string units)
    {
        PageWidth.Assign(width, units);
        PageHeight.Assign(height, units);
        //set the zero point to the bottom left
        //SetPageAxisMM(0, -height);

        SetPageAxisX(1, 0, units);
        SetPageAxisY(-1, height, units);
        CalculateTitle();
        return this;
    }
    public FoPage2D SetPageAxisX(int scale, double loc, string units)
    {
        //set the zero point to the bottom left
        ScaleAxisX = scale;
        ZeroPointX.Assign(loc, units);
        return this;
    }

    public FoPage2D SetPageAxisY(int scale, double loc, string units)
    {
        //set the zero point to the bottom left
        ScaleAxisY = scale;
        ZeroPointY.Assign(loc, units);
        return this;
    }

    public List<FoShape1D> AllShapes1D()
    {
        var result = new List<FoShape1D>();
        foreach (var value in Shapes1D.Values())
        {
            if (value is FoShape1D shape)
            {
                result.Add(shape);
            }
        }
        return result;
    }

    public List<FoShape2D> AllShapes2D()
    {
        var result = new List<FoShape2D>();
        foreach (var value in Shapes2D.Values())
        {
            if (value is FoShape2D shape)
            {
                result.Add(shape);
            }
        }
        return result;
    }


    public override List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        Shapes2D.ForEach(item => item.CollectImages(list, deep));
        return list;
    }

    public override List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        Shapes2D.ForEach(item => item.CollectVideos(list, deep));
        return list;
    }

    public override List<T> CollectMembers<T>(List<T> list, bool deep = true)
    {
        base.CollectMembers<T>(list, deep);

        if (deep)
        {
            GetMembers<FoCompound2D>()?.ForEach(item => item.CollectMembers<T>(list, deep));
        }
        return list;
    }



    public override bool Smash(bool force)
    {
        if (_matrix == null && !force) return false;
        //$"Smashing Page {Name} {GetType().Name} force {force}".WriteInfo(2);

        return base.Smash(force);
    }

    public override T CaptureShape<T>(T source, bool inPosition = false)
    {
        if (inPosition)
        {
            var dx = -LeftEdge();
            var dy = -TopEdge();
            source.MoveBy(dx, dy);
        }

        return AddShape<T>(source);
    }

    public T AddShape<T>(T value) where T : FoGlyph2D
    {
        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Key))
        {
            value.Key = collection.NextItemName();
        }

        collection.AddObject(value.Key, value);

        if (value is IShape2D)
        {
            Shapes2D.Add(value);
            //$"IShape2D Added {value.Name}".WriteSuccess();
        }
        else if (value is IShape1D)
        {
            Shapes1D.Add(value);
            // $"IShape1D Added {value.Name}".WriteSuccess();
        }

        FoGlyph2D.ResetHitTesting(true, $"FoPage2D AddShape {value.Key}");

        return value;
    }

    public T RemoveShape<T>(T value) where T : FoGlyph2D
    {
        value.MarkSelected(false);
        ExtractShapes(value.GlyphId);
        value.UnglueAll();

        if (value is IShape2D)
        {
            Shapes2D.Remove(value);
            //$"IShape2D Added {value.Name}".WriteSuccess();
        }
        else if (value is IShape1D)
        {
            Shapes1D.Remove(value);
            // $"IShape1D Added {value.Name}".WriteSuccess();
        }

        FoGlyph2D.ResetHitTesting(true, $"FoPage2D DeleteShape {value.Key}");
        return value;
    }

    public List<FoGlyph2D> ExtractSelected(List<FoGlyph2D> list)
    {
        var shapes2d = Shapes2D.ExtractWhere(child => child.IsSelected);
        var shapes1d = Shapes1D.ExtractWhere(child => child.IsSelected);
        list.AddRange(shapes2d);
        list.AddRange(shapes1d);
        return list;
    }

    public List<FoGlyph2D> CollectSelected(List<FoGlyph2D> list)
    {
        var shapes2d = Shapes2D.FindWhere(child => child.IsSelected);
        var shapes1d = Shapes1D.FindWhere(child => child.IsSelected);
        list.AddRange(shapes2d);
        list.AddRange(shapes1d);
        return list;
    }

    public void DeleteShape(FoGlyph2D shape)
    {
        shape.MarkSelected(false);
        ExtractShapes(shape.GlyphId);
        shape.UnglueAll();

        FoGlyph2D.ResetHitTesting(true, $"FoPage2D DeleteShape {shape.Key}");
    }

    public virtual void InsertShapesToQuadTree(QuadTree<QuadHitTarget> tree, IPanZoomService panzoom)
    {
        //Shapes1D.ForEach(child => tree.Insert(child)); 

        var count = Shapes2D.Count();
        // $"PAGE:: InsertShapesToQuadTree {Name} {count} items".WriteInfo(2);
        foreach (var item in Shapes2D.Values())
        {
            if (!item.IsSelectable())
                continue;

            var rect = item.HitTestRect();
            // $"Inserting1  {item.Name} {rect} ".WriteSuccess(1);
            rect = panzoom.TransformRect(rect);
            var target = QuadTargetExtensions.NewHitTarget(item, rect);
            // $"Inserting2  {item.Name} {rect} ".WriteSuccess(1);
            tree.Insert(target);
        }
        count = Shapes1D.Count();
        //$"PAGE:: InsertShapesToQuadTree {Name} Shapes1D {count} items".WriteInfo(2);
        foreach (var item in Shapes1D.Values())
        {
            if (!item.IsSelectable())
                continue;

            var list = item.HitTestSegment();
            // $"Inserting1  {item.Name} {rect} ".WriteSuccess(1);
            list = panzoom.TransformPoint(list);
            for (int i = 0; i < list.Count()-1; i++)
            {
                var target = QuadTargetExtensions.NewHitTarget(item, list[i], list[i+1]);
                tree.Insert(target);
            }
        }
    }

    public FoPage2D ClearAll()
    {
        ResetHitTesting(true, "FoPage ClearAll");
        var menus = Shapes2D.ExtractWhere(child => child is FoMenu2D);

        Shapes1D.Clear();
        Shapes2D.Clear();

        foreach (var item in menus)
            Shapes2D.Add(item);

        return this;
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var found = Shapes1D.FindWhere(child => child.GlyphIdCompare(GlyphId));
        if (found != null && found.Count > 0)
            result.AddRange(found);

        found = Shapes2D.FindWhere(child => child.GlyphIdCompare(GlyphId));
        if (found != null && found.Count > 0)
            result.AddRange(found);

        return result;
    }

    public FoGlyph2D? LookupShape2D(string GlyphId)
    {

        if ( Shapes2D.TryGetValue(GlyphId, out var found))
            return found;

        var list = Shapes2D.FindWhere(child => child.GlyphIdCompare(GlyphId));

        return list.FirstOrDefault();
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        var result = new List<FoGlyph2D>();

        var found = Shapes1D.ExtractWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);

        found = Shapes2D.ExtractWhere(child => child.GlyphId == GlyphId);
        if (found != null) result.AddRange(found);


        return result;
    }


    public new bool ComputeShouldRender(Rectangle region)
    {
        Shapes1D.ForEach(child => child.ComputeShouldRender(region));
        Shapes2D.ForEach(child => child.ComputeShouldRender(region));
        return true;
    }

    public async Task RenderGrid(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();


        await DrawHorizontalGrid(ctx, GridMinorH, false);
        await HRuler2D.DrawRuler(ctx, GridMinorH, false);

        await DrawVerticalGrid(ctx, GridMinorV, false);
        await VRuler2D.DrawRuler(ctx, GridMinorV, false);

        await ctx.RestoreAsync();
    }




    public async Task DrawHorizontalGrid(Canvas2DContext ctx, Length step, bool major)
    {
        await ctx.SaveAsync();

        var dStep = step.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        if (!major)
        {
            await ctx.SetLineWidthAsync(1);
            await ctx.SetLineDashAsync(new float[] { 5, 1 });
            await ctx.SetStrokeStyleAsync("White");
        }
        else
        {
            await ctx.SetLineDashAsync(Array.Empty<float>());
            await ctx.SetStrokeStyleAsync("Black");
        }

        var x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dStep;
        }

        await ctx.RestoreAsync();
    }


    public async Task DrawVerticalGrid(Canvas2DContext ctx, Length step, bool major)
    {
        await ctx.SaveAsync();

        var dStep = step.AsPixels();

        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        if (!major)
        {
            await ctx.SetLineWidthAsync(1);
            await ctx.SetLineDashAsync(new float[] { 5, 1 });
            await ctx.SetStrokeStyleAsync("White");
        }
        else
        {
            await ctx.SetLineDashAsync(Array.Empty<float>());
            await ctx.SetStrokeStyleAsync("Black");
        }

        var x = dHeight; //left;
        while (x >= dMargin)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x -= dStep;
        }

        await ctx.RestoreAsync();
    }

    /// <summary>
    /// Generates intersection points between all horizontal and vertical grid lines
    /// </summary>
    /// <returns>A list of points representing all grid intersections</returns>
    public List<Point> GenerateGridIntersectionPoints()
    {
        // Get sorted coordinates from shape boundaries
        var (sortedXCoords, sortedYCoords) = SortedCoordsShape2D();
        
        // Add page boundaries if they're not already included
        var margin = PageMargin.AsPixels();
        var leftBound = margin;
        var rightBound = margin + PageWidth.AsPixels();
        var topBound = margin;
        var bottomBound = margin + PageHeight.AsPixels();
        
        // Make sure we include page boundaries
        if (sortedXCoords.Count == 0 || sortedXCoords.Min(c => c.Value) > leftBound)
            sortedXCoords.Insert(0, new SortedCoord(leftBound, false, "Left Page Boundary"));
        if (sortedXCoords.Count == 0 || sortedXCoords.Max(c => c.Value) < rightBound)
            sortedXCoords.Add(new SortedCoord(rightBound, false, "Right Page Boundary"));
            
        if (sortedYCoords.Count == 0 || sortedYCoords.Min(c => c.Value) > topBound)
            sortedYCoords.Insert(0, new SortedCoord(topBound, false, "Top Page Boundary"));
        if (sortedYCoords.Count == 0 || sortedYCoords.Max(c => c.Value) < bottomBound)
            sortedYCoords.Add(new SortedCoord(bottomBound, false, "Bottom Page Boundary"));
        
        // Generate all intersection points
        var intersectionPoints = new List<Point>();
        foreach (var x in sortedXCoords)
        {
            foreach (var y in sortedYCoords)
            {
                intersectionPoints.Add(new Point((int)x.Value, (int)y.Value));
            }
        }
        
        return intersectionPoints;
    }

    /// <summary>
    /// Draws a small circle at each grid intersection point
    /// </summary>
    public async Task RenderGridIntersectionPoints(Canvas2DContext ctx, string circleColor = "Blue", int radius = 2)
    {
        // Get all intersection points
        var intersectionPoints = GenerateGridIntersectionPoints();
        
        await ctx.SaveAsync();
        
        // Set fill style for circles
        await ctx.SetFillStyleAsync(circleColor);
        
        // Draw a circle at each intersection point
        foreach (var point in intersectionPoints)
        {
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(point.X, point.Y, radius, 0, 2 * Math.PI);
            await ctx.FillAsync();
        }
        
        await ctx.RestoreAsync();
    }

    public async Task RenderShapeBoundaryGrid(Canvas2DContext ctx, string lineColor = "Yellow", float lineWidth = 3.5f)
    {
        await ctx.SaveAsync();

        // Get line segments and intersections for the grid
        var (segments, intersections) = GenerateGridLineNetwork();

        // Set up styling for grid lines
        await ctx.SetLineWidthAsync(lineWidth);
        await ctx.SetLineDashAsync(new float[] { 3, 2 });
        await ctx.SetStrokeStyleAsync(lineColor);

        // Draw all line segments
        foreach (var segment in segments)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(segment.Start.Center.X, segment.Start.Center.Y);
            await ctx.LineToAsync(segment.End.Center.X, segment.End.Center.Y);
            await ctx.StrokeAsync();
        }
        
        // Draw intersection points
        await ctx.SetFillStyleAsync("DarkBlue");
        foreach (var intersection in intersections)
        {
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(intersection.Center.X, intersection.Center.Y, 5, 0, 5 * Math.PI);
            await ctx.FillAsync();
        }

        await ctx.RestoreAsync();
    }

    // public virtual Point[] HitTestSegment(int margin = 0)
    // {
    //     var mat = GetMatrix();
    //     var p1 = mat.TransformToPoint(0-margin, 0-margin);
    //     var p2 = mat.TransformToPoint(Width+margin, 0-margin);
    //     var p3 = mat.TransformToPoint(Width+margin, Height+margin);
    //     var p4 = mat.TransformToPoint(0-margin, Height+margin);
    //     return new Point[] { p1, p2, p3, p4 };
    // }

    private (List<SortedCoord> sortedXCoords, List<SortedCoord> sortedYCoords) SortedCoordsShape2D()
    {
        // Collect all unique X and Y coordinates from the boundaries of all shapes
        HashSet<SortedCoord> xCoordinates = new(new SortedCoordComparer());
        HashSet<SortedCoord> yCoordinates = new(new SortedCoordComparer());

        // Get all 2D shapes on the page
        var shapes = AllShapes2D();

        foreach (var shape in shapes)
        {
            var matrix = shape.GetMatrix();
            var pin = matrix.TransformToPoint(shape.Width / 2, shape.Height / 2);
            
            // Add center point coordinates
            xCoordinates.Add(new SortedCoord(pin.X, true, $"Center:{shape.Name}"));
            yCoordinates.Add(new SortedCoord(pin.Y, true, $"Center:{shape.Name}"));

            // Get all boundary points from the SpacialBox2D
            var boundaryPoints = shape.HitTestSegment(10);

            foreach (var point in boundaryPoints)
            {
                xCoordinates.Add(new SortedCoord(point.X, true, $"Boundary of {shape.Name}"));
                yCoordinates.Add(new SortedCoord(point.Y, true, $"Boundary of {shape.Name}"));
            }
        }

        // Sort coordinates for consistent grid generation
        var sortedXCoords = xCoordinates.OrderBy(coord => coord.Value).ToList();
        var sortedYCoords = yCoordinates.OrderBy(coord => coord.Value).ToList();
        return (sortedXCoords, sortedYCoords);
    }
    
    // Comparer for SortedCoord to ensure we don't add duplicates to the HashSet
    private class SortedCoordComparer : IEqualityComparer<SortedCoord>
    {
        public bool Equals(SortedCoord x, SortedCoord y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return Math.Abs(x.Value - y.Value) < 0.001;
        }

        public int GetHashCode(SortedCoord obj)
        {
            return obj.Value.GetHashCode();
        }
    }

    /// <summary>
    /// Generates line segments for a grid that aligns with shape boundaries.
    /// Creates horizontal and vertical line segments between all adjacent intersections,
    /// but skips any line segments where either coordinate name starts with "Center".
    /// </summary>
    /// <returns>A tuple with lists of line segments and intersection points</returns>
    public (List<LineSegment> Segments, List<LineIntersection> Intersections) GenerateGridLineNetwork()
    {
        // Get sorted coordinates from shape boundaries
        var (sortedXCoords, sortedYCoords) = SortedCoordsShape2D();
        
        // Add page boundaries if they're not already included
        var margin = PageMargin.AsPixels();
        var leftBound = margin;
        var rightBound = margin + PageWidth.AsPixels();
        var topBound = margin;
        var bottomBound = margin + PageHeight.AsPixels();
        
        // Make sure we include page boundaries
        if (sortedXCoords.Count == 0 || sortedXCoords.Min(c => c.Value) > leftBound)
            sortedXCoords.Insert(0, new SortedCoord(leftBound, false, "Left Page Boundary"));
        if (sortedXCoords.Count == 0 || sortedXCoords.Max(c => c.Value) < rightBound)
            sortedXCoords.Add(new SortedCoord(rightBound, false, "Right Page Boundary"));
            
        if (sortedYCoords.Count == 0 || sortedYCoords.Min(c => c.Value) > topBound)
            sortedYCoords.Insert(0, new SortedCoord(topBound, false, "Top Page Boundary"));
        if (sortedYCoords.Count == 0 || sortedYCoords.Max(c => c.Value) < bottomBound)
            sortedYCoords.Add(new SortedCoord(bottomBound, false, "Bottom Page Boundary"));
        
        // Lists to store the resulting line segments
        var Segments = new List<LineSegment>();
        var Intersections = new Dictionary<(int x,int y), LineIntersection>();
        
        // Generate horizontal line segments (lines that go from left to right)
        foreach (var y in sortedYCoords)
        {

            for (int i = 0; i < sortedXCoords.Count - 1; i++)
            {
                var startX = sortedXCoords[i];
                var endX = sortedXCoords[i + 1];
                

                
                var start = FindOrCreateIntersection(startX.Value, y.Value, Intersections);
                var end = FindOrCreateIntersection(endX.Value, y.Value, Intersections);
                var segment = new LineSegment(start, end);
                start.Segments.Add(segment);
                end.Segments.Add(segment);
                Segments.Add(segment);
            }
            
        }
        
        // Generate vertical line segments (lines that go from top to bottom)
        foreach (var x in sortedXCoords)
        {

            for (int i = 0; i < sortedYCoords.Count - 1; i++)
            {
                var startY = sortedYCoords[i];
                var endY = sortedYCoords[i + 1];
                

                
                var start = FindOrCreateIntersection(x.Value, startY.Value, Intersections);
                var end = FindOrCreateIntersection(x.Value, endY.Value, Intersections);
                var segment = new LineSegment(start, end);
                start.Segments.Add(segment);
                end.Segments.Add(segment);
                Segments.Add(segment);
            }
            
        }
        
        // Filter out intersections that don't have any segments
        var filteredIntersections = Intersections.Values.Where(i => i.Segments.Count > 0).ToList();
        
        return (Segments, filteredIntersections);
    }
    
    private LineIntersection FindOrCreateIntersection(double startX, double y, Dictionary<(int x, int y), LineIntersection> intersections)
    {
        // Use the dictionary to find the intersection point
        var key = ((int)startX, (int)y);
        if (!intersections.TryGetValue(key, out var intersection))
        {
            intersection = new LineIntersection(new Point((int)startX, (int)y));
            intersections[key] = intersection;
        }
        return intersection;
    }

    public async Task<bool> RenderNoItems(Canvas2DContext ctx, int tick)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        var margin = PageMargin.AsPixels();
        Width = (PageWidth + (2 * margin)).AsPixels();
        Height = (PageHeight + (2 * margin)).AsPixels();

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Key}", PinX + 5, PinY + 5);

        //$"RenderNoItems Color={Color}".WriteInfo();
        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(1.0F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);
        await RenderShapeBoundaryGrid(ctx);

        await ctx.RestoreAsync();
        return true;
    }

    public override async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, 0);

        var margin = PageMargin.AsPixels();
        Width = PageWidth.AsPixels() + (2 * margin);
        Height = PageHeight.AsPixels() + (2 * margin);

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, Width, Height);

        //Draw the page name at the top
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync($"Page: {Key}", PinX + 5, PinY + 5);

        //$"RenderConcise Color={Color}".WriteInfo();
        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(1.0F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);
        await RenderShapeBoundaryGrid(ctx);

        //$"REC {region.X} {region.Y} {region.Width} {region.Height} ---".WriteLine(ConsoleColor.Blue);

        //only render members inside the region

        Shapes2D.ForEach(async child => await child.RenderConcise(ctx, scale, region));

        // draw the current window
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.SetLineWidthAsync(50.0F);


        //var win = CurrentScale.UserWindow();
        //await ctx.StrokeRectAsync(win.X, win.Y, win.Width, win.Height);

        await ctx.RestoreAsync();
        return true;
    }

    public string CalculateTitle(string title = "")
    {
        if (!string.IsNullOrEmpty(title))
            Title = title;
        else if (string.IsNullOrEmpty(Title))
        {
            var text = $"Page: {Key} {Color} | {Scale2D.Display()} | W:{PageWidth.AsString("cm")} x H:{PageHeight.AsString("cm")}  ({PageMargin.AsString("cm")}) |";
            text += $"  px {PageWidth.AsPixels()} x {PageHeight.AsPixels()} ({PageMargin.AsPixels()})";
            Title = text;
        }
        return Title;
    }

    public async Task DrawPageName(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();

        //Draw the page name at the top
        await ctx.SetFontAsync("16px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync("Black");
        await ctx.FillTextAsync(Title, PinX + 5, PinY + 5);
        await ctx.RestoreAsync();
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);


        var margin = PageMargin.AsPixels();
        var width = PageWidth.AsPixels() + (2.0 * margin);
        var height = PageHeight.AsPixels() + (2.0 * margin);

        Width = (int)width;
        Height = (int)height;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, width, height);


        await DrawPageName(ctx);

        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(1.0F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);
        await RenderShapeBoundaryGrid(ctx);

        //await DrawFancyPin(ctx);


        Shapes1D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        Shapes2D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));


        await ctx.RestoreAsync();
        return true;
    }

    public (int, int) DefaultDropLocation(double factor = 1.0)
    {
        var pWidth = PageWidth.AsPixels();
        var pHeight = PageHeight.AsPixels();
        var m = PageMargin.AsPixels();
        var size = Scale2D.PixelToDrawing(pHeight - m);

        return (
            m + (int)(factor * pWidth / 2),
            m + (int)(factor * pHeight / 2)
        );
    }

    
}