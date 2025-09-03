
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor.Extensions;
using System.Drawing;
using System.Xml.Linq;


namespace FoundryBlazor.Shape;

public class SpacialFrame3D : SpacialBox3D
{
    public Transform3 Transform { get; set; } = new Transform3();

    // Transformation parameters
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Z { get; set; } = 0;
    public double Rx { get; set; } = 0;
    public double Ry { get; set; } = 0;
    public double Rz { get; set; } = 0;
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public double ScaleZ { get; set; } = 1;
    
    public SpacialFrame3D(FoSpec3D spec, string units = "m")
    : base(spec.W, spec.H, spec.D, units)
    {
        Pivot = new Point3D(spec.Px, spec.Py, spec.Pz);
        X = spec.X;
        Y = spec.Y;
        Z = spec.Z;
        Rx = spec.Rx;
        Ry = spec.Ry;
        Rz = spec.Rz;
        UpdateTransform();
    }
    
    // Helper method to convert Point3D to Vector3D
    private FoVector3D ToVector3D(Point3D point)
    {
        return new FoVector3D(point.X, point.Y, point.Z);
    }
    
    // Helper method to convert Vector3D to Point3D
    private Point3D ToPoint3D(FoVector3D vector, string name = "")
    {
        return new Point3D(vector.X, vector.Y, vector.Z, name);
    }
    
    // Helper method to transform a point using the current Transform matrix
    private Point3D TransformPoint(Point3D point)
    {
        var vector = ToVector3D(point);
        
        // Convert FoVector3D to BlazorThreeJS.Vector3
        var blazorVector = new BlazorThreeJS.Maths.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        
        // Transform using Transform3
        var transformedBlazorVector = Transform.TransformPoint(blazorVector);
        
        // Convert back to FoVector3D
        var transformedFoVector = new FoVector3D(transformedBlazorVector.X, transformedBlazorVector.Y, transformedBlazorVector.Z);
        
        return ToPoint3D(transformedFoVector, point.Name);
    }
    
    // Helper method to transform a list of points
    private List<Point3D> TransformPoints(List<Point3D> points)
    {
        return points.Select(TransformPoint).ToList();
    }
    
    // Update the transformation matrix based on current parameters
    // Order: Scale -> Rotate -> Translate (correct 3D transformation order)
    public void UpdateTransform()
    {
        Transform.Identity()
            .SetScale(ScaleX, ScaleY, ScaleZ)
            .RotateEuler(Rx, Ry, Rz)
            .Translate(X, Y, Z);
    }
    
    // Call this when any transformation parameter changes
    public void SetTransform(double x, double y, double z, double rx = 0, double ry = 0, double rz = 0, 
                            double scaleX = 1, double scaleY = 1, double scaleZ = 1)
    {
        X = x; Y = y; Z = z;
        Rx = rx; Ry = ry; Rz = rz;
        ScaleX = scaleX; ScaleY = scaleY; ScaleZ = scaleZ;
        UpdateTransform();
    }


    // Override all geometric properties to apply transformation
    public override List<Point3D> Vertices => TransformPoints(LocalVertices.Select(v => v - Pivot).ToList());

    public override List<Point3D> LocalLeftFace => new List<Point3D>
    {
        LeftTopFront,
        LeftTopBack,
        LeftBottomBack,
        LeftBottomFront,
    }; 

    public override List<Point3D> LeftFace => TransformPoints(LocalLeftFace.Select(v => v - Pivot).ToList());

    public override Mesh3D LeftFaceMesh(double thickness, string color)
    {
        var transformedPosition = TransformPoint(new Point3D(-Width/2, 0, 0));
        var mesh = new Mesh3D
        {
            Name = "LeftFace",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(thickness, Height, Depth),
            Material = new MeshStandardMaterial() { Color = color },
            Transform = new Transform3() { Position = new Vector3(transformedPosition.X, transformedPosition.Y, transformedPosition.Z) },
        };
        return mesh;
    }

    public override Mesh3D RightFaceMesh(double thickness, string color)
    {
        var transformedPosition = TransformPoint(new Point3D(Width/2, 0, 0));
        var mesh = new Mesh3D
        {
            Name = "RightFace",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(thickness, Height, Depth),
            Material = new MeshStandardMaterial() { Color = color },
            Transform = new Transform3() { Position = new Vector3(transformedPosition.X, transformedPosition.Y, transformedPosition.Z) },
        };
        return mesh;
    }

    public override List<Point3D> LocalRightFace => new List<Point3D>
    {
        RightTopFront,
        RightTopBack,
        RightBottomBack,
        RightBottomFront,
    };

    public override List<Point3D> RightFace => TransformPoints(LocalRightFace.Select(v => v - Pivot).ToList());

    public override List<Point3D> LocalFrontFace => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        RightBottomFront,
        LeftBottomFront,
    };

    public override List<Point3D> FrontFace => TransformPoints(LocalFrontFace.Select(v => v - Pivot).ToList());

    public override List<Point3D> LocalBackFace => new List<Point3D>
    {
        LeftTopBack,
        RightTopBack,
        RightBottomBack,
        LeftBottomBack,
    };

    public override List<Point3D> BackFace => TransformPoints(LocalBackFace.Select(v => v - Pivot).ToList());

    public override List<Point3D> LocalTopFace => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        RightTopBack,
        LeftTopBack,
    };

    public override List<Point3D> TopFace => TransformPoints(LocalTopFace.Select(v => v - Pivot).ToList());

    public override List<Point3D> LocalBottomFace => new List<Point3D>
    {
        LeftBottomFront,
        RightBottomFront,
        RightBottomBack,
        LeftBottomBack,
    };

    public override List<Point3D> BottomFace => TransformPoints(LocalBottomFace.Select(v => v - Pivot).ToList());

    public override Point3D FrontFaceCenter => TransformPoint(new Point3D(Width/2, Height/2, 0, "front"));
    public override Point3D RearFaceCenter => TransformPoint(new Point3D(Width/2, Height/2, Depth, "rear"));
    public override Point3D LeftFaceCenter => TransformPoint(new Point3D(0, Height/2, Depth/2, "left"));
    public override Point3D RightFaceCenter => TransformPoint(new Point3D(Width, Height/2, Depth/2, "right"));
    public override Point3D TopFaceCenter => TransformPoint(new Point3D(Width/2, Height, Depth/2, "top"));
    public override Point3D BottomFaceCenter => TransformPoint(new Point3D(Width/2, 0, Depth/2, "bottom"));

    public override List<Point3D> LocalFaceCenters => new List<Point3D>
    {
        FrontFaceCenter,
        RearFaceCenter,
        LeftFaceCenter,
        RightFaceCenter,
        TopFaceCenter,
        BottomFaceCenter
    };

    public override List<Point3D> FaceCenters => TransformPoints(LocalFaceCenters.Select(v => v - Pivot).ToList());

   
    // Edge centers with transformation applied
    public override Point3D EdgeCenterTopFront => TransformPoint(new Point3D(Width/2, Height, Depth));
    public override Point3D EdgeCenterTopBack => TransformPoint(new Point3D(Width/2, Height, 0));

    public override Point3D EdgeCenterTopLeft => TransformPoint(new Point3D(0, Height, Depth/2));
    public override Point3D EdgeCenterTopRight => TransformPoint(new Point3D(Width, Height, Depth/2));

    public override Point3D EdgeCenterBottomFront => TransformPoint(new Point3D(Width/2, 0, Depth));
    public override Point3D EdgeCenterBottomBack => TransformPoint(new Point3D(Width/2, 0, 0));

    public override Point3D EdgeCenterBottomLeft => TransformPoint(new Point3D(0, 0, Depth/2));
    public override Point3D EdgeCenterBottomRight => TransformPoint(new Point3D(Width, 0, Depth/2));

    public override Point3D EdgeCenterFrontLeft => TransformPoint(new Point3D(0, Height/2, Depth));
    public override Point3D EdgeCenterFrontRight => TransformPoint(new Point3D(Width, Height/2, Depth));

    public override Point3D EdgeCenterBackLeft => TransformPoint(new Point3D(0, Height/2, 0));
    public override Point3D EdgeCenterBackRight => TransformPoint(new Point3D(Width, Height/2, 0));

    public override Point3D EdgeCenterTop => TransformPoint(new Point3D(Width/2, Height, Depth/2));
    public override Point3D EdgeCenterBottom => TransformPoint(new Point3D(Width/2, 0, Depth/2));

    public override Point3D EdgeCenterFront => TransformPoint(new Point3D(Width/2, Height/2, Depth));
    public override Point3D EdgeCenterBack => TransformPoint(new Point3D(Width/2, Height/2, 0));
    

    public override List<Point3D> LocalEdgeCenters => new List<Point3D>
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

    public override List<Point3D> EdgeCenters => TransformPoints(LocalEdgeCenters.Select(v => v - Pivot).ToList());

    // Override to ensure edges use properly transformed coordinates
    public override List<Edge3D> GetEdgesWithNames()
    {
        var verts = Vertices; // These are already transformed - simple!
        var edgeDefs = new (int, int, string)[]
        {
            (0,1,"TopFront"), (1,3,"TopRight"), (3,2,"TopBack"), (2,0,"TopLeft"),
            (4,5,"BottomFront"), (5,7,"BottomRight"), (7,6,"BottomBack"), (6,4,"BottomLeft"),
            (0,4,"LeftFront"), (1,5,"RightFront"), (2,6,"LeftBack"), (3,7,"RightBack")
        };
        
        var edges = new List<Edge3D>();
        foreach (var (i, j, name) in edgeDefs)
        {
            // Simple: just use the transformed vertices, let Edge3D calculate its own midpoint
            edges.Add(new Edge3D(name, verts[i], verts[j]));
        }
        return edges;
    }

    // Override to ensure faces use properly transformed coordinates and rotated normals
    public override List<Face3D> GetFacesWithNormals()
    {
        // Get transformed face vertices
        var frontFace = FrontFace;
        var backFace = BackFace;
        var leftFace = LeftFace;
        var rightFace = RightFace;
        var topFace = TopFace;
        var bottomFace = BottomFace;
        
        // Transform the normal vectors according to the frame's rotation
        var rotMatrix = Matrix3.NewMatrix().Identity().RotateEuler(Rx, Ry, Rz);
        var frontNormal = rotMatrix.TransformPoint(new Vector3(0, 0, 1));
        var backNormal = rotMatrix.TransformPoint(new Vector3(0, 0, -1));
        var leftNormal = rotMatrix.TransformPoint(new Vector3(-1, 0, 0));
        var rightNormal = rotMatrix.TransformPoint(new Vector3(1, 0, 0));
        var topNormal = rotMatrix.TransformPoint(new Vector3(0, 1, 0));
        var bottomNormal = rotMatrix.TransformPoint(new Vector3(0, -1, 0));
        
        return new List<Face3D>
        {
            new Face3D("Front", frontFace, frontNormal),
            new Face3D("Back", backFace, backNormal),
            new Face3D("Left", leftFace, leftNormal),
            new Face3D("Right", rightFace, rightNormal),
            new Face3D("Top", topFace, topNormal),
            new Face3D("Bottom", bottomFace, bottomNormal)
        };
    }

}