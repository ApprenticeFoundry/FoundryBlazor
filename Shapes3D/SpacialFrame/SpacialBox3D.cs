using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor.Shape;
using System.Drawing;
using System.Xml.Linq;



namespace FoundryBlazor.Shape;


public class SpacialBox3D
{
    public string Units { get; set; } = "m";
    public double Width { get; set; } = 1.0;
    public double Height { get; set; } = 1.0;
    public double Depth { get; set; } = 1.0;

    private double HalfWidth { get; set; }
    private double HalfHeight { get; set; }
    private double HalfDepth { get; set; }

    public Point3D Pivot { get; set; } = new Point3D(0, 0, 0);

    public double Volume => Width * Height * Depth;

    public double SurfaceArea => 2 * (Width * Height + Height * Depth + Width * Depth);

    public SpacialBox3D(double width, double height, double depth, string units = "m")
    {
        Width = width;
        Height = height;
        Depth = depth;
        Units = units;
        HalfWidth = Width / 2;
        HalfHeight = Height / 2;
        HalfDepth = Depth / 2;
        Pivot = new Point3D(HalfWidth, HalfHeight, HalfDepth);
    }



    public Point3D Center => new(HalfWidth, HalfHeight, HalfDepth);

    public Point3D LeftTopFront => new(0, Height, Depth);
    public Point3D RightTopFront => new(Width, Height, Depth);
    public Point3D LeftBottomFront => new(0, 0, Depth);
    public Point3D RightBottomFront => new(Width, 0, Depth);
    public Point3D LeftTopBack => new(0, Height, 0);
    public Point3D RightTopBack => new(Width, Height, 0);
    public Point3D LeftBottomBack => new(0, 0, 0);
    public Point3D RightBottomBack => new(Width, 0, 0);

    public List<Point3D> LocalVertices => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        LeftBottomFront,
        RightBottomFront,
        LeftTopBack,
        RightTopBack,
        LeftBottomBack,
        RightBottomBack
    };

    public virtual List<Point3D> Vertices => LocalVertices.Select(v => v - Pivot).ToList();

    public virtual List<Point3D> LocalLeftFace => new List<Point3D>
    {
        LeftTopFront,
        LeftTopBack,
        LeftBottomBack,
        LeftBottomFront,
    };

    public virtual List<Face3D> GetFacesWithNormals()
    {
        return new List<Face3D>
            {
                new Face3D("Front", FrontFace, new Vector3(0,0,1)),
                new Face3D("Back", BackFace, new Vector3(0,0,-1)),
                new Face3D("Left", LeftFace, new Vector3(-1,0,0)),
                new Face3D("Right", RightFace, new Vector3(1,0,0)),
                new Face3D("Top", TopFace, new Vector3(0,1,0)),
                new Face3D("Bottom", BottomFace, new Vector3(0,-1,0))
            };
    }

    public virtual List<Point3D> LeftFace => LocalLeftFace.Select(v => v - Pivot).ToList();

    public virtual Mesh3D LeftFaceMesh(double thickness, string color)
    {
        var geometry = new BoxGeometry(thickness, Height, Depth);
        var mesh = new Mesh3D
        {
            Name = "LeftFace",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(thickness, Height, Depth),
            Material = new MeshStandardMaterial() { Color = color },
            Transform = new Transform3() { Position = new Vector3(-HalfWidth, 0, 0) },
        };
        return mesh;
    }

    public virtual Mesh3D RightFaceMesh(double thickness, string color)
    {
        var geometry = new BoxGeometry(thickness, Height, Depth);
        var mesh = new Mesh3D
        {
            Name = "RightFace",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(thickness, Height, Depth),
            Material = new MeshStandardMaterial() { Color = color },
            Transform = new Transform3() { Position = new Vector3(HalfWidth, 0, 0) },
        };
        return mesh;
    }

    public virtual List<Point3D> LocalRightFace => new List<Point3D>
    {
        RightTopFront,
        RightTopBack,
        RightBottomBack,
        RightBottomFront,
    };

    public virtual List<Point3D> RightFace => LocalRightFace.Select(v => v - Pivot).ToList();

    public virtual List<Point3D> LocalFrontFace => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        RightBottomFront,
        LeftBottomFront,
    };

    public virtual List<Point3D> FrontFace => LocalFrontFace.Select(v => v - Pivot).ToList();

    public virtual List<Point3D> LocalBackFace => new List<Point3D>
    {
        LeftTopBack,
        RightTopBack,
        RightBottomBack,
        LeftBottomBack,
    };

    public virtual List<Point3D> BackFace => LocalBackFace.Select(v => v - Pivot).ToList();

    public virtual List<Point3D> LocalTopFace => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        RightTopBack,
        LeftTopBack,
    };

    public virtual List<Point3D> TopFace => LocalTopFace.Select(v => v - Pivot).ToList();

    public virtual List<Point3D> LocalBottomFace => new List<Point3D>
    {
        LeftBottomFront,
        RightBottomFront,
        RightBottomBack,
        LeftBottomBack,
    };

    public virtual List<Point3D> BottomFace => LocalBottomFace.Select(v => v - Pivot).ToList();

    public virtual Point3D FrontFaceCenter => new(HalfWidth, HalfHeight, 0, "front");
    public virtual Point3D RearFaceCenter => new(HalfWidth, HalfHeight, Depth, "rear");
    public virtual Point3D LeftFaceCenter => new(0, HalfHeight, HalfDepth, "left");
    public virtual Point3D RightFaceCenter => new(Width, HalfHeight, HalfDepth, "right");
    public virtual Point3D TopFaceCenter => new(HalfWidth, Height, HalfDepth, "top");
    public virtual Point3D BottomFaceCenter => new(HalfWidth, 0, HalfDepth, "bottom");

    public virtual List<Point3D> LocalFaceCenters => new List<Point3D>
    {
        FrontFaceCenter,
        RearFaceCenter,
        LeftFaceCenter,
        RightFaceCenter,
        TopFaceCenter,
        BottomFaceCenter
    };

    public virtual List<Point3D> FaceCenters => LocalFaceCenters.Select(v => v - Pivot).ToList();



    public virtual Point3D EdgeCenterTopFront => new Point3D(HalfWidth, Height, Depth);
    public virtual Point3D EdgeCenterTopBack => new Point3D(HalfWidth, Height, 0);

    public virtual Point3D EdgeCenterTopLeft => new Point3D(0, Height, HalfDepth);
    public virtual Point3D EdgeCenterTopRight => new Point3D(Width, Height, HalfDepth);

    public virtual Point3D EdgeCenterBottomFront => new Point3D(HalfWidth, 0, Depth);
    public virtual Point3D EdgeCenterBottomBack => new Point3D(HalfWidth, 0, 0);

    public virtual Point3D EdgeCenterBottomLeft => new Point3D(0, 0, HalfDepth);
    public virtual Point3D EdgeCenterBottomRight => new Point3D(Width, 0, HalfDepth);

    public virtual Point3D EdgeCenterFrontLeft => new Point3D(0, HalfHeight, Depth);
    public virtual Point3D EdgeCenterFrontRight => new Point3D(Width, HalfHeight, Depth);

    public virtual Point3D EdgeCenterBackLeft => new Point3D(0, HalfHeight, 0);
    public virtual Point3D EdgeCenterBackRight => new Point3D(Width, HalfHeight, 0);

    public virtual Point3D EdgeCenterTop => new Point3D(HalfWidth, Height, HalfDepth);
    public virtual Point3D EdgeCenterBottom => new Point3D(HalfWidth, 0, HalfDepth);

    public virtual Point3D EdgeCenterFront => new Point3D(HalfWidth, HalfHeight, Depth);
    public virtual Point3D EdgeCenterBack => new Point3D(HalfWidth, HalfHeight, 0);


    public virtual List<Point3D> LocalEdgeCenters => new List<Point3D>
    {
        EdgeCenterTopFront,
        EdgeCenterTopBack,
        EdgeCenterTopLeft,
        EdgeCenterTopRight,
        EdgeCenterBottomFront,
        EdgeCenterBottomBack,
        EdgeCenterBottomLeft,
        EdgeCenterBottomRight,
        EdgeCenterFrontLeft,
        EdgeCenterFrontRight,
        EdgeCenterBackLeft,
        EdgeCenterBackRight,
        EdgeCenterTop,
        EdgeCenterBottom,
        EdgeCenterFront,
        EdgeCenterBack
    };

    public virtual List<Point3D> EdgeCenters => LocalEdgeCenters.Select(v => v - Pivot).ToList();

    // Returns all 12 box edges as named Edge3D objects
    public virtual List<Edge3D> GetEdgesWithNames()
    {
        var verts = Vertices;
        var edgeDefs = new (int, int, string)[]
        {
            (0,1,"TopFront"), (1,3,"TopRight"), (3,2,"TopBack"), (2,0,"TopLeft"),
            (4,5,"BottomFront"), (5,7,"BottomRight"), (7,6,"BottomBack"), (6,4,"BottomLeft"),
            (0,4,"LeftFront"), (1,5,"RightFront"), (2,6,"LeftBack"), (3,7,"RightBack")
        };
        var edges = new List<Edge3D>();
        foreach (var (i, j, name) in edgeDefs)
            edges.Add(new Edge3D(name, verts[i], verts[j]));
        return edges;
    }
}


