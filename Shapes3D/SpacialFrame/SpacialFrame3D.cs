
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
        
        // Get transform and CHECK DIRTY FLAG BEFORE APPLYING
        var transform = Source.GetTransform();
        if ( transform.IsDirty )
        {
            $"🔍 TransformPoint: IsDirty = {transform.IsDirty}, Point = ({point.X:F2}, {point.Y:F2}, {point.Z:F2})".WriteError();
        } else
        {
            $"🔍 TransformPoint: IsDirty = {transform.IsDirty}, Point = ({point.X:F2}, {point.Y:F2}, {point.Z:F2})".WriteSuccess();
        }

        // Transform using Transform3
        var result = transform.TransformPoint(vector);

        // Convert back to Point3D with preserved name
        return new Point3D(result.X, result.Y, result.Z, point.Name);
    }
    
    // Helper method to transform a list of points
    private List<Point3D> TransformPoints(List<Point3D> points)
    {
        return points.Select(TransformPoint).ToList();
    }
    
    // === DEBUGGING METHOD ===
    public string DebugTransformation(Point3D localPoint)
    {
        var vector = localPoint.AsVector3();
        var transform = Source.GetTransform();
        var matrix = transform.ToMatrix3();
        var transformedVector = matrix.TransformPoint(vector);
        
        // Check if transform is dirty and log matrix values
        var matrixArray = matrix.GetMatrix();
        
        return $"🔍 DIRTY FLAG: {transform.IsDirty}\n" +
               $"Local: ({localPoint.X:F2}, {localPoint.Y:F2}, {localPoint.Z:F2}) " +
               $"-> Transformed: ({transformedVector.X:F2}, {transformedVector.Y:F2}, {transformedVector.Z:F2})\n" +
               $"Transform - Pos: ({transform.Position.X:F2}, {transform.Position.Y:F2}, {transform.Position.Z:F2}) " +
               $"Rot: ({transform.Rotation.X:F2}, {transform.Rotation.Y:F2}, {transform.Rotation.Z:F2}) " +
               $"Pivot: ({transform.Pivot.X:F2}, {transform.Pivot.Y:F2}, {transform.Pivot.Z:F2})\n" +
               $"Matrix[0-3]: [{matrixArray[0]:F2}, {matrixArray[1]:F2}, {matrixArray[2]:F2}, {matrixArray[3]:F2}]\n" +
               $"Matrix[4-7]: [{matrixArray[4]:F2}, {matrixArray[5]:F2}, {matrixArray[6]:F2}, {matrixArray[7]:F2}]\n" +
               $"Matrix[8-11]: [{matrixArray[8]:F2}, {matrixArray[9]:F2}, {matrixArray[10]:F2}, {matrixArray[11]:F2}]\n" +
               $"Matrix[12-15]: [{matrixArray[12]:F2}, {matrixArray[13]:F2}, {matrixArray[14]:F2}, {matrixArray[15]:F2}]";
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
        var transform = Source.GetTransform();
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