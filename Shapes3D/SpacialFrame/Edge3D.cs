using BlazorThreeJS.Maths;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
// Represents a named edge between two 3D points
public class Edge3D
{
    public string Name { get; set; }
    public Point3D Start { get; set; }
    public Point3D End { get; set; }
    public Point3D Midpoint { get; set; }

    public Edge3D(string name, Point3D start, Point3D end)
    {
        Name = name;
        Start = start;
        End = end;
        Midpoint = new Point3D(
            (start.X + end.X) / 2,
            (start.Y + end.Y) / 2,
            (start.Z + end.Z) / 2,
            "Midpoint"
        );
    }



    public double Length => Math.Sqrt(
        Math.Pow(End.X - Start.X, 2) +
        Math.Pow(End.Y - Start.Y, 2) +
        Math.Pow(End.Z - Start.Z, 2)
    );



    // Returns the axis and angle needed to rotate local Y to align with this edge
    public (Vector3 axis, double angle) GetAxisAngle()
    {
        var up = new Vector3(0, 1, 0);
        var dir = new Vector3(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z).Normalize();
        var axis = up.Cross(dir);
        var axisLength = axis.Length();
        double angle = 0;
        if (axisLength > 1e-6)
        {
            axis = axis.Normalize();
            angle = Math.Acos(Math.Max(-1.0, Math.Min(1.0, up.Dot(dir))));
        }
        else
        {
            angle = up.Dot(dir) > 0 ? 0 : Math.PI;
            axis = new Vector3(1, 0, 0); // Arbitrary axis
        }
        return (axis, angle);
    }

    // Returns Euler angles (approximate for axis-aligned edges)
    public Euler EulerRotation
    {
        get
        {
            var (axis, angle) = GetAxisAngle();
            double ex = 0, ey = 0, ez = 0;
            
            // Map the rotation to the dominant axis
            if (Math.Abs(axis.X) > 0.9) ex = angle;
            else if (Math.Abs(axis.Y) > 0.9) ey = angle;
            else if (Math.Abs(axis.Z) > 0.9) ez = angle;
            else
            {
                // For non-axis-aligned rotations, distribute across axes
                // This is the simplest approach that often works
                ex = axis.X * angle;
                ey = axis.Y * angle;
                ez = axis.Z * angle;
            }
            
            return new Euler((float)ex, (float)ey, (float)ez, "XYZ");
        }
    }
}

