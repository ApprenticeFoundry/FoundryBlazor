using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Units;

namespace FoundryBlazor.Shape;

public class LineSegment
{
    public LineIntersection Start { get; set; }
    public LineIntersection End { get; set; }

    public LineSegment(LineIntersection start, LineIntersection end)
    {
        Start = start;
        End = end;
    }

    public void Clear()
    {
        Start.Clear();
        End.Clear();
    }
}




public class LineIntersection
{
    public Point2D Center { get; set; } = new Point2D(0, 0);
    public List<LineSegment> Segments { get; init; } = new List<LineSegment>();

    public LineIntersection(Point2D loc)
    {
        Center = loc;
    }
    public LineIntersection(int x, int y, string name = "")
    {
        Center = new Point2D(x, y, name);
    }

    public void Clear()
    {
        Segments.Clear();
    }
    public void AddSegment(LineSegment segment)
    {
        Segments.Add(segment);
    }
    public double X => Center.U;
    public double Y => Center.V;
    public string Name => Center.Name;
    public int Count => Segments.Count;
}

public class FoLineRouter2D
{
    private static readonly Queue<LineIntersection> IntersectionCache = new();
    private static readonly Dictionary<Point2D, LineIntersection> IntersectionsByPoint = new();
    private static readonly Queue<LineSegment> SegmentCache = new();
    private static readonly List<LineSegment> Segments = new();

    public Length PageWidth => Page.PageWidth;
    public Length PageHeight => Page.PageHeight;
    public Length PageMargin => Page.PageMargin;

    public FoPage2D Page { get; set; }
    public IHitTestService? HitService { get; set; }

    public FoLineRouter2D(FoPage2D page)
    {
        Page = page;
    }

    public void MarkAsDirty(bool wasDirty, IHitTestService service)
    {
        if ( wasDirty ) Clear();
        HitService = service;
    }

    public void Clear()
    {
        $"Clear Smash Line Segments".WriteSuccess();
        foreach (var item in Segments)
            SmashLineSegment(item);

        $"Clear Smash Intersection".WriteSuccess();
        foreach (var item in IntersectionsByPoint.Values)
            SmashIntersection(item);


        Segments.Clear();
        IntersectionsByPoint.Clear();
    }

    public List<FoShape2D> AllShapes2D()
    {
        return Page.AllShapes2D();
    }

    public async Task RenderGrid(Canvas2DContext ctx, string lineColor = "Black", float lineWidth = 1.2f)
    {
        await ctx.SaveAsync();
        
        // Get line segments and intersections for the grid

        if (Segments.Count == 0)
            GenerateGridLineNetwork();

        // Set up styling for grid lines
        await ctx.SetLineWidthAsync(lineWidth);
        await ctx.SetStrokeStyleAsync(lineColor);

        // Draw all line segments
        foreach (var segment in Segments)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(segment.Start.X, segment.Start.Y);
            await ctx.LineToAsync(segment.End.X, segment.End.Y);
            await ctx.StrokeAsync();
        }
        
        // // Draw intersection points
        foreach (var intersection in IntersectionsByPoint.Values)
        {
            //select a color based on the number of segments there cam be as many as 4
            var color = intersection.Count switch
            {
                1 => "DarkBlue",
                2 => "DarkGreen",
                3 => "DarkOrange",
                4 => "DarkRed",
                _ => "Black"
            };

            await ctx.SetFillStyleAsync(color);
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(intersection.X, intersection.Y, 5, 0, 5 * Math.PI);
            await ctx.FillAsync();
        }

        await ctx.RestoreAsync();
    }

    public static LineSegment EstablishLineSegment(LineIntersection start, LineIntersection end)
    {
        LineSegment found;
                        
        if (IntersectionCache.Count > 0)
        {
            found = SegmentCache.Dequeue();
            found.Start = start;
            found.End = end;
        }
        else 
        {
            found = new LineSegment(start, end);
        }

        Segments.Add(found);
        start.AddSegment(found);
        end.AddSegment(found);
        return found;
    }



    public static LineSegment? SmashLineSegment(LineSegment segment)
    {
        if (segment is null) return null;
        segment.Clear();
        SegmentCache.Enqueue(segment);
        return segment;
    }



    public static LineIntersection EstablishIntersection(Point2D loc)
    {
        if (IntersectionsByPoint.TryGetValue(loc, out var found))
            return found;

        if (IntersectionCache.Count > 0)
        {
            found = IntersectionCache.Dequeue();
            found.Center = loc;
        }
        else 
        {
            found = new LineIntersection(loc);
            $"New Intersection Created {found.Center.U} {found.Center.V}".WriteLine(ConsoleColor.Red);
        }
        IntersectionsByPoint[found.Center] = found;
        return found;
    }

    public static LineIntersection? SmashIntersection(LineIntersection intersection)
    {
        if (intersection is null) return null;
        intersection.Clear();
        IntersectionCache.Enqueue(intersection);
        return intersection;
    }











    private (List<int> sortedXCoords, List<int> sortedYCoords) SortedCoordsShape2D()
    {
        // Collect all unique X and Y coordinates from the boundaries of all shapes
        HashSet<int> xCoordinates = new();
        HashSet<int> yCoordinates = new();

        // Get all 2D shapes on the page
        var shapes = AllShapes2D();

        foreach (var shape in shapes)
        {
            var matrix = shape.GetMatrix();
            var pin = matrix.TransformToPoint(shape.Width / 2, shape.Height / 2);
            
            // Add center point coordinates
            xCoordinates.Add(pin.X);
            yCoordinates.Add(pin.Y);
            EstablishIntersection(new Point2D(pin.X, pin.Y, shape.GetGlyphId()));

            // Get all boundary points from the SpacialBox2D
            var boundaryPoints = shape.HitTestSegment(10);

            foreach (var point in boundaryPoints)
            {
                xCoordinates.Add(point.X);
                yCoordinates.Add(point.Y);
                EstablishIntersection(new Point2D(point.X, point.Y, shape.GetGlyphId()));
            }
        }

        // Sort coordinates for consistent grid generation
        var sortedXCoords = xCoordinates.OrderBy(coord => coord).ToList();
        var sortedYCoords = yCoordinates.OrderBy(coord => coord).ToList();
        return (sortedXCoords, sortedYCoords);
    }
    


    public void GenerateGridLineNetwork()
    {
        // Get sorted coordinates from shape boundaries
        var (sortedXCoords, sortedYCoords) = SortedCoordsShape2D();
        
        // Add page boundaries if they're not already included
        var margin = PageMargin.AsPixels();
        var leftBound = margin;
        var rightBound = margin + PageWidth.AsPixels();
        var topBound = margin;
        var bottomBound = margin + PageHeight.AsPixels();
        
        // // Make sure we include page boundaries
        if (sortedXCoords.Count == 0 || sortedXCoords.Min(c => c) > leftBound)
            sortedXCoords.Insert(0, leftBound);
        if (sortedXCoords.Count == 0 || sortedXCoords.Max(c => c) < rightBound)
            sortedXCoords.Add(rightBound);
            
        if (sortedYCoords.Count == 0 || sortedYCoords.Min(c => c) > topBound)
            sortedYCoords.Insert(0, topBound);
        if (sortedYCoords.Count == 0 || sortedYCoords.Max(c => c) < bottomBound)
            sortedYCoords.Add(bottomBound);
        
        
        // Generate horizontal line segments (lines that go from left to right)
        foreach (var y in sortedYCoords)
        {
            for (int i = 0; i < sortedXCoords.Count - 1; i++)
            {
                var startX = sortedXCoords[i];
                var endX = sortedXCoords[i + 1];

                if (HitService!.FindGlyph(startX,y) || HitService.FindGlyph(endX,y))
                {
                    $"Skip Intersection Hit Test Passed {startX} {y} {endX}".WriteSuccess();
                    continue;
                }


                var point1 = new Point2D(startX, y);
                var point2 = new Point2D(endX, y);

                var start = EstablishIntersection(point1);
                var end = EstablishIntersection(point2);
                EstablishLineSegment(start, end);

            }
        }
        
        // Generate vertical line segments (lines that go from top to bottom)
        foreach (var x in sortedXCoords)
        {
            for (int i = 0; i < sortedYCoords.Count - 1; i++)
            {
                var startY = sortedYCoords[i];
                var endY = sortedYCoords[i + 1];
                            
                if (HitService!.FindGlyph(x,startY) || HitService.FindGlyph(x,endY))
                {
                    $"Skip Intersection Hit Test Passed {x} {startY} {endY}".WriteSuccess();
                    continue;
                }
                
                var point1 = new Point2D(x, startY);
                var point2 = new Point2D(x, endY);
                
                var start = EstablishIntersection(point1);
                var end = EstablishIntersection(point2);
                EstablishLineSegment(start, end);
            }
        }
    }

    
}