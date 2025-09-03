
using BlazorThreeJS.Maths;

namespace FoundryBlazor.Extensions;


public static class Vector3MathExtensions
{

    //add 2 vectors to create third
    public static Vector3 Add(this Vector3 a, Vector3 b)
    {
        return new Vector3
        {
            X = a.X + b.X,
            Y = a.Y + b.Y,
            Z = a.Z + b.Z
        };
    }

    //Lerp a point along a line specified by a vector using a scaler
    public static Vector3 Lerp(this Vector3 a, Vector3 b, double t)
    {
        return new Vector3
        {
            X = a.X + (b.X - a.X) * t,
            Y = a.Y + (b.Y - a.Y) * t,
            Z = a.Z + (b.Z - a.Z) * t
        };
    }

    //conver to euler angles
    public static Vector3 ToEuler(this Vector3 a)
    {
        var x = Math.Atan2(a.Y, a.Z);
        var y = Math.Atan2(a.Z, a.X);
        var z = Math.Atan2(a.X, a.Y);
        return new Vector3
        {
            X = x,
            Y = y,
            Z = z
        };
    }


    //subtract 2 vectors to create third
    public static Vector3 Subtract(this Vector3 a, Vector3 b)
    {
        return new Vector3
        {
            X = a.X - b.X,
            Y = a.Y - b.Y,
            Z = a.Z - b.Z
        };
    }

    //center point of 2 vectors
    public static Vector3 Center(this Vector3 a, Vector3 b)
    {
        return new Vector3
        {
            X = (a.X + b.X) / 2,
            Y = (a.Y + b.Y) / 2,
            Z = (a.Z + b.Z) / 2
        };
    }
    //multiply vector by scalar
    public static Vector3 Multiply(this Vector3 a, double scalar)
    {
        return new Vector3
        {
            X = a.X * scalar,
            Y = a.Y * scalar,
            Z = a.Z * scalar
        };
    }

    // center of 

    public static Vector3 BoundingBox(this Vector3 a, Vector3 b)
    {
        return new Vector3
        {
            X = Math.Abs(a.X - b.X),
            Y = Math.Abs(a.Y - b.Y),
            Z = Math.Abs(a.Z - b.Z)
        };
    }

    // distance between 2 vectors
    public static double Distance(this Vector3 a, Vector3 b)
    {
        return Math.Sqrt(
            Math.Pow(a.X - b.X, 2) +
            Math.Pow(a.Y - b.Y, 2) +
            Math.Pow(a.Z - b.Z, 2)
        );
    }




}

