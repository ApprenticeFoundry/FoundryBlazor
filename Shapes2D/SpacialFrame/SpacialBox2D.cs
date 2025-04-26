using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor.Extensions;
using System.Drawing;
using System.Xml.Linq;

namespace FoundryBlazor.Shape;

public class SpacialBox2D
{
    public string Units { get; set; } = "m";
    public double Width { get; set; } = 1.0;
    public double Height { get; set; } = 1.0;

    private double HalfWidth { get; set; }
    private double HalfHeight { get; set; }

    public Point2D Pivot { get; set; } = new Point2D(0, 0);

    public double Area => Width * Height;

    public SpacialBox2D(double width, double height, string units = "m")
    {
        Width = width;
        Height = height;
        Units = units;
        HalfWidth = Width / 2;
        HalfHeight = Height / 2;
        Pivot = new Point2D(HalfWidth, HalfHeight);
    }

    //public SpacialBox2D(FoShape2D source, string units = "m")
    //{
    //    Width = source.Width;
    //    Height = source.Height;
    //    Units = units;
    //    HalfWidth = Width / 2;
    //    HalfHeight = Height / 2;
    //    Pivot = new Point2D(source.LocPinX(source), source.LocPinY(source));
    //}

    public Point2D Center => new(HalfWidth, HalfHeight);
    public Point2D Zero => new(0, 0);

    public Point2D LeftTop => new(0, Height);
    public Point2D RightTop => new(Width, Height);
    public Point2D LeftBottom => new(0, 0);
    public Point2D RightBottom => new(Width, 0);

    public List<Point2D> LocalVertices => new List<Point2D>
    {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    };

    public List<Point2D> Vertices => LocalVertices.Select(v => v - Pivot).ToList();

    public List<Point2D> LocalLeftEdge => new List<Point2D>
    {
        LeftTop,
        LeftBottom
    };

    public List<Point2D> LeftEdge => LocalLeftEdge.Select(v => v - Pivot).ToList();

    public List<Point2D> LocalRightEdge => new List<Point2D>
    {
        RightTop,
        RightBottom
    };

    public List<Point2D> RightEdge => LocalRightEdge.Select(v => v - Pivot).ToList();

    public List<Point2D> LocalTopEdge => new List<Point2D>
    {
        LeftTop,
        RightTop
    };

    public List<Point2D> TopEdge => LocalTopEdge.Select(v => v - Pivot).ToList();

    public List<Point2D> LocalBottomEdge => new List<Point2D>
    {
        LeftBottom,
        RightBottom
    };

    public List<Point2D> BottomEdge => LocalBottomEdge.Select(v => v - Pivot).ToList();

    public Point2D LeftEdgeCenter => new(0, HalfHeight, "left");
    public Point2D RightEdgeCenter => new(Width, HalfHeight, "right");
    public Point2D TopEdgeCenter => new(HalfWidth, Height, "top");
    public Point2D BottomEdgeCenter => new(HalfWidth, 0, "bottom");

    public List<Point2D> LocalEdgeCenters => new List<Point2D>
    {
        LeftEdgeCenter,
        RightEdgeCenter,
        TopEdgeCenter,
        BottomEdgeCenter
    };

    public List<Point2D> EdgeCenters => LocalEdgeCenters.Select(v => v - Pivot).ToList();

    public List<Point2D> AllBoundaries => new List<Point2D>
    {
        //Zero
        //Center,
        LeftTop,
         RightTop,
         LeftBottom,
         RightBottom,
        LeftEdgeCenter,
        RightEdgeCenter,
         TopEdgeCenter,
         BottomEdgeCenter
    };
}