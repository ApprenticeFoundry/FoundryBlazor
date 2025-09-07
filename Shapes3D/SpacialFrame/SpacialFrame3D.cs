using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;
using System.Xml.Linq;


namespace FoundryBlazor.Shape;

public class SpacialFrame3D : SpacialBox3D
{
    public FoShape3D Source { get; set; } = new FoShape3D();


    public SpacialFrame3D(FoShape3D shape, string units = "m")
    : base(shape, units)
    {
        Source = shape;
    }

    // Helper method to transform a point using the current Transform matrix
    private Point3D TransformPoint(Point3D point)
    {
        // Convert Point3D directly to BlazorThreeJS.Vector3
        var vector = point.AsVector3();
        
        // Get transform and ENSURE MATRIX IS COMPUTED
        var transform = Source.Transform;

        // Transform using Transform3 (now guaranteed to have clean matrix)
        var result = transform.TransformPoint(vector);

        // Convert back to Point3D with preserved name
        return new Point3D(result.X, result.Y, result.Z, point.Name);
    }
    
    // Helper method to transform a list of points
    private List<Point3D> TransformPoints(List<Point3D> points)
    {
        return points.Select(TransformPoint).ToList();
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
        var transform = Source.Transform;
        var faces = new List<Face3D>();
        
        foreach (var face in localFaces)
        {
            // Transform vertices
            var vertices = TransformPoints(face.Vertices);

            // Transform normal vector (rotation only, not translation)
            var transformedNormal = transform.TransformDirection(face.Normal);

            // Create new transformed face
            faces.Add(new Face3D(face.Name, vertices, transformedNormal));
        }
        
        return faces;
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