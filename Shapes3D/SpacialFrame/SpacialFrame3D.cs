
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
    public double Px { get; set; } = 0;
    public double Py { get; set; } = 0;
    public double Pz { get; set; } = 0;
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public double ScaleZ { get; set; } = 1;

 
    public SpacialFrame3D(FoSpec3D spec, string units = "m")
    : base(spec, units)
    {
        X = spec.X;
        Y = spec.Y;
        Z = spec.Z;
        Rx = spec.Rx;
        Ry = spec.Ry;
        Rz = spec.Rz;
        Px = spec.Px;
        Py = spec.Py;
        Pz = spec.Pz;
    }

    public SpacialFrame3D(FoShape3D shape, string units = "m")
    : base(shape, units)
    {
        X = shape.Width;
        Y = shape.Height;
        Z = shape.Depth;
        Transform = shape.GetTransform();
        // Rx = shape.Rx;
        // Ry = shape.Ry;
        // Rz = shape.Rz;
        // Px = shape.Px;
        // Py = shape.Py;
        // Pz = shape.Pz;
    }

    // Update the transformation matrix based on current parameters
    // Order: Scale -> Rotate -> Translate (correct 3D transformation order)
    // With proper pivot point handling
    public void UpdateTransform()
    {
        Transform.Identity()
            .SetScale(ScaleX, ScaleY, ScaleZ)
            .SetPivot(new Vector3(Px, Py, Pz))
            .RotateEuler(Rx, Ry, Rz)
            .Translate(X, Y, Z);
    }
    
    // Helper method to transform a point using the current Transform matrix
    private Point3D TransformPoint(Point3D point)
    {
        // Convert Point3D directly to BlazorThreeJS.Vector3
        var blazorVector = new Vector3(point.X, point.Y, point.Z);
        
        // Transform using Transform3
        var transformedVector = Transform.TransformPoint(blazorVector);
        
        // Convert back to Point3D with preserved name
        return new Point3D(transformedVector.X, transformedVector.Y, transformedVector.Z, point.Name);
    }
    
    // Helper method to transform a list of points
    private List<Point3D> TransformPoints(List<Point3D> points)
    {
        return points.Select(TransformPoint).ToList();
    }
    

    
    // Call this when any transformation parameter changes
    public void SetTransform(double x, double y, double z, double rx = 0, double ry = 0, double rz = 0, 
                            double scaleX = 1, double scaleY = 1, double scaleZ = 1,
                            double px = 0, double py = 0, double pz = 0)
    {
        X = x; Y = y; Z = z;
        Rx = rx; Ry = ry; Rz = rz;
        ScaleX = scaleX; ScaleY = scaleY; ScaleZ = scaleZ;
        Px = px; Py = py; Pz = pz;
        UpdateTransform();
    }

    // === TRANSFORMED GEOMETRY ACCESS METHODS ===
    
    // Get transformed vertices
    public List<Point3D> GetVertices()
    {
        return TransformPoints(GetLocalVertices());
    }

    // Get transformed face centers
    public List<Point3D> GetFaceCenters()
    {
        return TransformPoints(GetLocalFaceCenters());
    }

    // Get transformed edge centers
    public List<Point3D> GetEdgeCenters()
    {
        return TransformPoints(GetLocalEdgeCenters());
    }

    // Get transformed faces with rotated normals
    public List<Face3D> GetFaces()
    {
        // Get local faces
        var localFaces = GetLocalFaces();
        
        // Transform face vertices and normals
        var transformMatrix = Transform.ToMatrix3();
        var transformedFaces = new List<Face3D>();
        
        foreach (var face in localFaces)
        {
            // Transform vertices
            var transformedVertices = TransformPoints(face.Vertices);
            
            // Transform normal vector (rotation only, not translation)
            var transformedNormal = transformMatrix.TransformDirection(face.Normal);
            
            // Create new transformed face
            transformedFaces.Add(new Face3D(face.Name, transformedVertices, transformedNormal));
        }
        
        return transformedFaces;
    }

    // Get transformed edges
    public List<Edge3D> GetEdges()
    {
        var localEdges = GetLocalEdges();
        var transformedEdges = new List<Edge3D>();
        
        foreach (var edge in localEdges)
        {
            var transformedStart = TransformPoint(edge.Start);
            var transformedEnd = TransformPoint(edge.End);
            transformedEdges.Add(new Edge3D(edge.Name, transformedStart, transformedEnd));
        }
        
        return transformedEdges;
    }

    // === OVERRIDE FACE CENTER PROPERTIES FOR TRANSFORMATION ===
    
    public  Point3D FrontFaceCenter => TransformPoint(LocalFrontFaceCenter);
    public  Point3D RearFaceCenter => TransformPoint(LocalRearFaceCenter);
    public  Point3D LeftFaceCenter => TransformPoint(LocalLeftFaceCenter);
    public  Point3D RightFaceCenter => TransformPoint(LocalRightFaceCenter);
    public  Point3D TopFaceCenter => TransformPoint(LocalTopFaceCenter);
    public  Point3D BottomFaceCenter => TransformPoint(LocalBottomFaceCenter);

    // === OVERRIDE EDGE CENTER PROPERTIES FOR TRANSFORMATION ===
    
    public  Point3D CenterTopFrontEdge => TransformPoint(LocalCenterTopFrontEdge);
    public  Point3D CenterTopBackEdge => TransformPoint(LocalCenterTopBackEdge);
    public  Point3D CenterTopLeftEdge => TransformPoint(LocalCenterTopLeftEdge);
    public  Point3D CenterTopRightEdge => TransformPoint(LocalCenterTopRightEdge);
    public  Point3D CenterBottomFrontEdge => TransformPoint(LocalCenterBottomFrontEdge);
    public  Point3D CenterBottomBackEdge => TransformPoint(LocalCenterBottomBackEdge);
    public  Point3D CenterBottomLeftEdge => TransformPoint(LocalCenterBottomLeftEdge);
    public  Point3D CenterBottomRightEdge => TransformPoint(LocalCenterBottomRightEdge);
    public  Point3D CenterFrontLeftEdge => TransformPoint(LocalCenterFrontLeftEdge);
    public  Point3D CenterFrontRightEdge => TransformPoint(LocalCenterFrontRightEdge);
    public  Point3D CenterBackLeftEdge => TransformPoint(LocalCenterBackLeftEdge);
    public  Point3D CenterBackRightEdge => TransformPoint(LocalCenterBackRightEdge);
    public  Point3D CenterTopEdge => TransformPoint(LocalCenterTopEdge);
    public  Point3D CenterBottomEdge => TransformPoint(LocalCenterBottomEdge);
    public  Point3D CenterFrontEdge => TransformPoint(LocalCenterFrontEdge);
    public  Point3D CenterBackEdge => TransformPoint(LocalCenterBackEdge);
}