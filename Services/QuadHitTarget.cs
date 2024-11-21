

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace FoundryBlazor.Shape;
public enum HitShape
{
    None,
    Rectangle,
    Circle,
    LineSegment,
}
public interface ICanHitTarget
{
    string GetName();
    Rectangle HitTestRect();
    Point[] HitTestSegment();
    bool IsSmashed();
}

public class QuadHitTarget 
{
    public Rectangle rectangle;
    public Point point;
    public ICanHitTarget target;
    public HitShape hitShape = HitShape.None;

    public Point LastProjection = Point.Empty;
    public Point LastMouseHit = Point.Empty;
    public double LastDistance = 0;

    public QuadHitTarget Purge()
    {
        rectangle = Rectangle.Empty;
        point = Point.Empty;
        target = null!;
        hitShape = HitShape.None;
        return this;
    }


    public QuadHitTarget(Rectangle rect, ICanHitTarget target)
    {
        Update(rect, target);
        this.target = target;
    }

    public QuadHitTarget Update(Rectangle rect, ICanHitTarget target)
    {
        this.rectangle = rect;
        this.point = new Point(rect.X + rect.Width, rect.Y + rect.Height);
        this.target = target;
        this.hitShape = HitShape.Rectangle;
        return this;
    }

    public QuadHitTarget(Point point1, Point point2, ICanHitTarget target)
    {
        Update(point1, point2, target);
        this.target = target;
    }

    public QuadHitTarget Update(Point point1, Point point2, ICanHitTarget target)
    {
        var width = Math.Abs(point2.X - point1.X);
        var height = Math.Abs(point2.Y - point1.Y);
        this.rectangle = new Rectangle(point1.X, point1.Y, width, height);
        this.point = point2;
        this.target = target;
        this.hitShape = HitShape.LineSegment;

        //$"QuadHitTarget Update {point1} {point2} {target.GetName()}".WriteInfo();
        return this;
    }

    public bool IsOpen()
    {
        return hitShape == HitShape.None;
    }
    public bool IsLineSegment()
    {
        return hitShape == HitShape.LineSegment;
    }
    public bool IsRectangle()
    {
        return hitShape == HitShape.Rectangle;
    }

//quadraticCurveTo(cpx, cpy, x, y)

    public async Task DrawQuadHitTarget(Canvas2DContext ctx)
    {
        if (IsLineSegment())
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(8);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(rectangle.Location.X, rectangle.Location.Y);
            await ctx.LineToAsync(point.X, point.Y);
            await ctx.StrokeAsync();

            await ctx.SetStrokeStyleAsync("yellow");
            await ctx.SetLineDashAsync([]);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(LastMouseHit.X, LastMouseHit.Y);
            await ctx.LineToAsync(LastProjection.X, LastProjection.Y);
            await ctx.StrokeAsync();

            //$"{target.GetName()} Distance {LastDistance}  M:{LastMouseHit.X}  {LastMouseHit.Y} P: {LastProjection.X} {LastProjection.Y}".WriteInfo();
        }
        else if (IsRectangle())
        {
            await ctx.SetStrokeStyleAsync("black");
            await ctx.SetLineWidthAsync(8);
            await ctx.BeginPathAsync();
            await ctx.StrokeRectAsync(rectangle.X-2, rectangle.Y-2, rectangle.Width+4, rectangle.Height+4);
            await ctx.StrokeAsync();
        }
    }

    public bool IsContainedBy(Rectangle rect)
    {
        var found = false;
        if (IsLineSegment())
        {
            found = rect.Contains(this.rectangle.Location) && rect.Contains(point);
            //$"LineSegment {found}  {rectangle.Location}  {point}".WriteInfo();
        }
        else if (IsRectangle())
        {
            found = rect.Contains(this.rectangle);
        }
        return found;
    }



    public bool IsIntersectedBy(Rectangle rect, double minRequired)

    {
        if (IsLineSegment())
        {
            LastMouseHit = rect.Location;
            var distance = DistancePointToLineSegment(LastMouseHit);
            var success = distance < minRequired;
            //$"Distance {success}  {distance} type {hitShape}".WriteInfo();
            return success;
        }
        else if (IsRectangle())
        {
            var success = rect.IntersectsWith(rectangle);
            //$"Distance {success} type {hitShape}".WriteInfo();
            return success;
        }
        return false;
    }

   public bool SegmentIntersects(Rectangle rect)
    {
        // Cohen–Sutherland algorithm for line clipping
        int OutCode(Point p)
        {
            int code = 0;
            if (p.X < rect.X) code |= 1;
            else if (p.X > rect.X + rect.Width) code |= 2;
            if (p.Y < rect.Y) code |= 4;
            else if (p.Y > rect.Y + rect.Height) code |= 8;
            return code;
        }

        var Start = rect.Location;
        var End = point;
        int outcode0 = OutCode(Start);
        int outcode1 = OutCode(End);

        while (true)
        {
            if ((outcode0 | outcode1) == 0) return true; // Both points inside
            if ((outcode0 & outcode1) != 0) return false; // Line is outside

            int outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;
            Point p = new Point(0, 0);

            if ((outcodeOut & 8) != 0) // Above
            {
                p.X = Start.X + (End.X - Start.X) * (rect.Y + rect.Height - Start.Y) / (End.Y - Start.Y);
                p.Y = rect.Y + rect.Height;
            }
            else if ((outcodeOut & 4) != 0) // Below
            {
                p.X = Start.X + (End.X - Start.X) * (rect.Y - Start.Y) / (End.Y - Start.Y);
                p.Y = rect.Y;
            }
            else if ((outcodeOut & 2) != 0) // Right
            {
                p.Y = Start.Y + (End.Y - Start.Y) * (rect.X + rect.Width - Start.X) / (End.X - Start.X);
                p.X = rect.X + rect.Width;
            }
            else if ((outcodeOut & 1) != 0) // Left
            {
                p.Y = Start.Y + (End.Y - Start.Y) * (rect.X - Start.X) / (End.X - Start.X);
                p.X = rect.X;
            }

            if (outcodeOut == outcode0)
            {
                Start = p;
                outcode0 = OutCode(Start);
            }
            else
            {
                End = p;
                outcode1 = OutCode(End);
            }
        }
    }

    public bool SegmentContainsPoint(Point point, double tolerance = 0.1)
    {
        var Start = rectangle.Location;
        var End = point;

        double crossProduct = (point.Y - Start.Y) * (End.X - Start.X) - (point.X - Start.X) * (End.Y - Start.Y);
        if (Math.Abs(crossProduct) > tolerance) return false;

        double dotProduct = (point.X - Start.X) * (End.X - Start.X) + (point.Y - Start.Y) * (End.Y - Start.Y);
        if (dotProduct < 0) return false;

        double squaredLength = (End.X - Start.X) * (End.X - Start.X) + (End.Y - Start.Y) * (End.Y - Start.Y);
        if (dotProduct > squaredLength) return false;

        return true;
    }
  
    public double DistancePointToLineSegment(Point p)
    {

        Point v = rectangle.Location;
        Point w = point;

        var lengthSquared = Math.Pow(w.X - v.X, 2) + Math.Pow(w.Y - v.Y, 2);
        if (lengthSquared == 0) {
            
            LastProjection = v;
            LastDistance = DistanceBetweenPoints(p, w);
            return LastDistance;
        }

        var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / lengthSquared;

        if (t < 0) 
        {
            LastProjection = v;
            LastDistance = DistanceBetweenPoints(p, v);
        }
        else if (t > 1) 
        {
            LastProjection = w;
            LastDistance = DistanceBetweenPoints(p, w);
        }
        else 
        {
            var xx = v.X + t * (w.X - v.X);
            var yy = v.Y + t * (w.Y - v.Y);
           // $"t {t} xx {xx} yy {yy}".WriteInfo();
            LastProjection = new Point((int)xx, (int)yy);
            LastDistance = DistanceBetweenPoints(p, LastProjection);

        }
        //$"DistancePointToLineSegment {LastDistance} {t} {LastProjection}  M{p}".WriteInfo();
        
        return LastDistance;
    }

    public double DistanceBetweenPoints(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    public (Point, double) CalculateIntersectionAndDistance(Point p3)
    {
        Point p1 = rectangle.Location;
        Point p2 = point;

        // Compute the direction vector components
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        // Since Y increases downward, the normal vector components are (dy, -dx)
        double A = dy;   // Y-component of the normal vector
        double B = -dx;  // X-component of the normal vector
        double C = dx * p1.Y - dy * p1.X;

        // Calculate the perpendicular distance from p3 to the line
        double numerator = Math.Abs(A * p3.X + B * p3.Y + C);
        double denominator = Math.Sqrt(A * A + B * B);  // This is used to compute the distance

        // Calculate the projection of p3 onto the line
        double tNumerator = (p3.X - p1.X) * dx + (p3.Y - p1.Y) * dy;
        double tDenominator = dx * dx + dy * dy;
        double t = tNumerator / tDenominator;

        // Find the intersection point (projection of p3 onto the line)
        var xx = p1.X + t * dx;
        var yy = p1.Y + t * dy;
        Point intersectionPoint = new Point((int)xx, (int)yy);

        // Calculate the squared distance between p3 and the intersection point
        int dx1 = p3.X - intersectionPoint.X;
        int dy1 = p3.Y - intersectionPoint.Y;
        double distanceSquared = dx1 * dx1 + dy1 * dy1;
        double distance = Math.Sqrt(distanceSquared);

        $"{target.GetName()} Distance {distance} {t}  M:{p3.X}  {p3.Y} P: {intersectionPoint.X} {intersectionPoint.Y}".WriteInfo();
        return (intersectionPoint, distance);
    }

}