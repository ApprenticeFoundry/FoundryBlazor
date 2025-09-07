using BlazorThreeJS.Maths;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;
// Represents a named edge between two 3D points
public class Edge3D
{
    public string Name { get; set; }
    public List<Point3D> Points { get; set; }= new();
    public Point3D Start => Points.FirstOrDefault();
    public Point3D End => Points.LastOrDefault();
    public Point3D Midpoint => Points.Count == 2 ? 
        new Point3D(
            (Start.X + End.X) / 2,
            (Start.Y + End.Y) / 2,
            (Start.Z + End.Z) / 2,
            "Midpoint"
        ) : 
        new Point3D(0,0,0,"Midpoint");

    public Edge3D(string name, Point3D start, Point3D end)
    {
        Name = name;
        Points.Add(start);
        Points.Add(end);
        $"{Name} Edge3D created from {start.Name} to {end.Name}".WriteNote(2);
    }

    public Edge3D(string name, List<Point3D> points)
    {
        Name = name;
        Points.AddRange(points);
        $"{Name} Edge3D created with {points.Count} points".WriteNote(2);
    }

    public List<Vector3> AsPath() => Points.Select(p => p.AsVector3()).ToList();

    public double Length => Start.DistanceTo(End);
    public Vector3 Direction => (End - Start).AsVector3().Normalize();


}

