
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

    // dot product of 2 vectors
    public static double Dot(this Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    // cross product of 2 vectors
    public static Vector3 Cross(this Vector3 a, Vector3 b)
    {
        return new Vector3
        {
            X = a.Y * b.Z - a.Z * b.Y,
            Y = a.Z * b.X - a.X * b.Z,
            Z = a.X * b.Y - a.Y * b.X
        };
    }

    // normalize vector
    public static Vector3 Normalize(this Vector3 a)
    {
        var length = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        return new Vector3
        {
            X = a.X / length,
            Y = a.Y / length,
            Z = a.Z / length
        };
    }

    //euler angles to vector
    

    // //transform vector by matrix
    // public static Vector3 ApplyMatrix4(this Vector3 v, Matrix4x4 m)
    // {
    //     return new Vector3
    //     {
    //         X = v.X * m.Elements[0] + v.Y * m.Elements[4] + v.Z * m.Elements[8] + m.Elements[12],
    //         Y = v.X * m.Elements[1] + v.Y * m.Elements[5] + v.Z * m.Elements[9] + m.Elements[13],
    //         Z = v.X * m.Elements[2] + v.Y * m.Elements[6] + v.Z * m.Elements[10] + m.Elements[14]
    //     };
    // }


}

