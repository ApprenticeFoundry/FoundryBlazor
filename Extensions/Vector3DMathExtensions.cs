using System;
using BlazorThreeJS.Maths;

namespace FoundryBlazor.Extensions;

/// <summary>
/// Essential Vector3D operations for face constraint system
/// </summary>
public static class Vector3DMathExtensions
{
    // === STANDARD DIRECTION VECTORS ===
    public static FoVector3D Up => new(0, 1, 0);
    public static FoVector3D Down => new(0, -1, 0);
    public static FoVector3D Right => new(1, 0, 0);
    public static FoVector3D Left => new(-1, 0, 0);
    public static FoVector3D Forward => new(0, 0, 1);
    public static FoVector3D Backward => new(0, 0, -1);
    
    // === BASIC OPERATIONS ===
    
    /// <summary>Calculate dot product of two vectors</summary>
    public static double Dot(FoVector3D a, FoVector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    /// <summary>Calculate cross product of two vectors</summary>
    public static FoVector3D Cross(FoVector3D a, FoVector3D b)
    {
        return new FoVector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
    
    /// <summary>Calculate vector magnitude/length</summary>
    public static double Length(this FoVector3D vector)
    {
        return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    }
    
    /// <summary>Normalize vector to unit length</summary>
    public static FoVector3D Normalize(this FoVector3D vector)
    {
        var length = vector.Length();
        if (length < 1e-10) return new FoVector3D(0, 0, 0);
        return new FoVector3D(vector.X / length, vector.Y / length, vector.Z / length);
    }
    
    /// <summary>Multiply vector by scalar</summary>
    public static FoVector3D Multiply(this FoVector3D vector, double scalar)
    {
        return new FoVector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }
    
    /// <summary>Add two vectors</summary>
    public static FoVector3D Add(this FoVector3D a, FoVector3D b)
    {
        return new FoVector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    /// <summary>Subtract two vectors</summary>
    public static FoVector3D Subtract(this FoVector3D a, FoVector3D b)
    {
        return new FoVector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    /// <summary>Distance between two points</summary>
    public static double Distance(FoVector3D a, FoVector3D b)
    {
        return a.Subtract(b).Length();
    }
    
    /// <summary>Linear interpolation between two vectors</summary>
    public static FoVector3D Lerp(FoVector3D a, FoVector3D b, double t)
    {
        return a.Add(b.Subtract(a).Multiply(t));
    }
    
    /// <summary>Clamp vector components to range</summary>
    public static FoVector3D Clamp(this FoVector3D vector, double min, double max)
    {
        return new FoVector3D(
            Math.Clamp(vector.X, min, max),
            Math.Clamp(vector.Y, min, max),
            Math.Clamp(vector.Z, min, max)
        );
    }
    
    /// <summary>Check if vectors are approximately equal</summary>
    public static bool ApproximatelyEqual(FoVector3D a, FoVector3D b, double tolerance = 1e-6)
    {
        return Distance(a, b) < tolerance;
    }
    
    /// <summary>Project vector onto another vector</summary>
    public static FoVector3D Project(FoVector3D vector, FoVector3D onto)
    {
        var ontoNormalized = onto.Normalize();
        var projectionLength = Dot(vector, ontoNormalized);
        return ontoNormalized.Multiply(projectionLength);
    }
    
    /// <summary>Reflect vector across a normal</summary>
    public static FoVector3D Reflect(FoVector3D vector, FoVector3D normal)
    {
        var normalizedNormal = normal.Normalize();
        return vector.Subtract(normalizedNormal.Multiply(2 * Dot(vector, normalizedNormal)));
    }
    
    /// <summary>Get angle between two vectors in radians</summary>
    public static double AngleBetween(FoVector3D a, FoVector3D b)
    {
        var dot = Dot(a.Normalize(), b.Normalize());
        return Math.Acos(Math.Clamp(dot, -1.0, 1.0));
    }
}
