using System;

namespace FoundryBlazor.Shapes3D.SpacialFrame;

/// <summary>
/// FoVector3D - Double precision 3D vector for FoundryBlazor compatibility
/// Used for bridging between FoundryBlazor (double precision) and BlazorThreeJS (float precision)
/// </summary>
public class FoVector3D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public FoVector3D(double x = 0, double y = 0, double z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static FoVector3D operator +(FoVector3D a, FoVector3D b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static FoVector3D operator -(FoVector3D a, FoVector3D b)
        => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static FoVector3D operator *(FoVector3D v, double scalar)
        => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public override string ToString()
        => $"({X:F2}, {Y:F2}, {Z:F2})";
}
