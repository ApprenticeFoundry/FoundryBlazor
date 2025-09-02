using System;
using BlazorThreeJS.Maths;

namespace FoundryBlazor.Extensions;

/// <summary>
/// Essential Vector3D operations for face constraint system
/// </summary>
public static class Vector3DMathExtensions
{
    // === STANDARD DIRECTION VECTORS ===
    public static Vector3D Up => new(0, 1, 0);
    public static Vector3D Down => new(0, -1, 0);
    public static Vector3D Right => new(1, 0, 0);
    public static Vector3D Left => new(-1, 0, 0);
    public static Vector3D Forward => new(0, 0, 1);
    public static Vector3D Backward => new(0, 0, -1);
    
    // === BASIC OPERATIONS ===
    
    /// <summary>Calculate dot product of two vectors</summary>
    public static double Dot(Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    /// <summary>Calculate cross product of two vectors</summary>
    public static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
    
    /// <summary>Calculate vector magnitude/length</summary>
    public static double Length(this Vector3D vector)
    {
        return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    }
    
    /// <summary>Normalize vector to unit length</summary>
    public static Vector3D Normalize(this Vector3D vector)
    {
        var length = vector.Length();
        if (length < 1e-10) return new Vector3D(0, 0, 0);
        return new Vector3D(vector.X / length, vector.Y / length, vector.Z / length);
    }
    
    /// <summary>Multiply vector by scalar</summary>
    public static Vector3D Multiply(this Vector3D vector, double scalar)
    {
        return new Vector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }
    
    /// <summary>Add two vectors</summary>
    public static Vector3D Add(this Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    /// <summary>Subtract two vectors</summary>
    public static Vector3D Subtract(this Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    /// <summary>Distance between two points</summary>
    public static double Distance(Vector3D a, Vector3D b)
    {
        return a.Subtract(b).Length();
    }
    
    /// <summary>Linear interpolation between two vectors</summary>
    public static Vector3D Lerp(Vector3D a, Vector3D b, double t)
    {
        return a.Add(b.Subtract(a).Multiply(t));
    }
    
    /// <summary>Clamp vector components to range</summary>
    public static Vector3D Clamp(this Vector3D vector, double min, double max)
    {
        return new Vector3D(
            Math.Clamp(vector.X, min, max),
            Math.Clamp(vector.Y, min, max),
            Math.Clamp(vector.Z, min, max)
        );
    }
    
    /// <summary>Check if vectors are approximately equal</summary>
    public static bool ApproximatelyEqual(Vector3D a, Vector3D b, double tolerance = 1e-6)
    {
        return Distance(a, b) < tolerance;
    }
    
    /// <summary>Project vector onto another vector</summary>
    public static Vector3D Project(Vector3D vector, Vector3D onto)
    {
        var ontoNormalized = onto.Normalize();
        var projectionLength = Dot(vector, ontoNormalized);
        return ontoNormalized.Multiply(projectionLength);
    }
    
    /// <summary>Reflect vector across a normal</summary>
    public static Vector3D Reflect(Vector3D vector, Vector3D normal)
    {
        var normalizedNormal = normal.Normalize();
        return vector.Subtract(normalizedNormal.Multiply(2 * Dot(vector, normalizedNormal)));
    }
    
    /// <summary>Get angle between two vectors in radians</summary>
    public static double AngleBetween(Vector3D a, Vector3D b)
    {
        var dot = Dot(a.Normalize(), b.Normalize());
        return Math.Acos(Math.Clamp(dot, -1.0, 1.0));
    }
}
