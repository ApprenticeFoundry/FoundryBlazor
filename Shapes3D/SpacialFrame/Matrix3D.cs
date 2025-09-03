using BlazorThreeJS.Maths;

namespace FoundryBlazor.Shapes3D.SpacialFrame;

/// <summary>
/// Matrix3D - now delegates to BlazorThreeJS Matrix3 for all operations
/// This maintains compatibility with existing FoundryBlazor code while using the enhanced math system
/// </summary>
public class Matrix3D
{
    private readonly Matrix3 _matrix;
    
    public Matrix3D()
    {
        _matrix = Matrix3.NewMatrix();
    }
    
    public Matrix3D(Matrix3 matrix)
    {
        _matrix = matrix;
    }
    
    // Static constants for compatibility
    public static readonly double DEG_TO_RAD = Math.PI / 180;
    private static readonly Queue<Matrix3D> cache = new();
    
    // Object pooling compatibility
    public static Matrix3D NewMatrix()
    {
        if (cache.Count == 0)
            return new Matrix3D();
        return cache.Dequeue();
    }
    
    public static Matrix3D? SmashMatrix(Matrix3D? source)
    {
        if (source == null) return null;
        source.Identity();
        cache.Enqueue(source);
        return null;
    }
    
    // Core transformation methods (delegate to Matrix3)
    public Matrix3D Identity()
    {
        _matrix.Identity();
        return this;
    }
    
    public Matrix3D Translate(double x, double y, double z)
    {
        _matrix.Translate(x, y, z);
        return this;
    }
    
    public Matrix3D Scale(double x, double y, double z)
    {
        _matrix.Scale(x, y, z);
        return this;
    }
    
    public Matrix3D RotateX(double angle)
    {
        _matrix.RotateX(angle);
        return this;
    }
    
    public Matrix3D RotateY(double angle)
    {
        _matrix.RotateY(angle);
        return this;
    }
    
    public Matrix3D RotateZ(double angle)
    {
        _matrix.RotateZ(angle);
        return this;
    }
    
    public Matrix3D RotateEuler(double x, double y, double z)
    {
        _matrix.RotateEuler(x, y, z);
        return this;
    }
    
    // Transform point with FoVector3D compatibility
    public FoVector3D TransformPoint(FoVector3D point)
    {
        var blazorVector = VectorConversions.FromFoVector3D(point.X, point.Y, point.Z);
        var transformedVector = _matrix.TransformPoint(blazorVector);
        return new FoVector3D(transformedVector.X, transformedVector.Y, transformedVector.Z);
    }
    
    // Matrix operations
    public Matrix3D Multiply(Matrix3D other)
    {
        _matrix.Multiply(other._matrix);
        return this;
    }
    
    public Matrix3D AppendTransform(double x, double y, double z,
                                          double scaleX, double scaleY, double scaleZ,
                                          double rotX, double rotY, double rotZ,
                                          double regX, double regY, double regZ)
    {
        _matrix.AppendTransform(x, y, z, scaleX, scaleY, scaleZ, rotX, rotY, rotZ, regX, regY, regZ);
        return this;
    }
    
    public Matrix3D PrependTransform(double x, double y, double z,
                                           double scaleX, double scaleY, double scaleZ,
                                           double rotX, double rotY, double rotZ,
                                           double regX, double regY, double regZ)
    {
        _matrix.PrependTransform(x, y, z, scaleX, scaleY, scaleZ, rotX, rotY, rotZ, regX, regY, regZ);
        return this;
    }
    
    public Matrix3D Append(double m11, double m12, double m13, double m14,
                               double m21, double m22, double m23, double m24,
                               double m31, double m32, double m33, double m34,
                               double m41, double m42, double m43, double m44)
    {
        _matrix.Append(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        return this;
    }
    
    public Matrix3D Prepend(double m11, double m12, double m13, double m14,
                                double m21, double m22, double m23, double m24,
                                double m31, double m32, double m33, double m34,
                                double m41, double m42, double m43, double m44)
    {
        _matrix.Prepend(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        return this;
    }
    
    public Matrix3D Set(double m11, double m12, double m13, double m14,
                            double m21, double m22, double m23, double m24,
                            double m31, double m32, double m33, double m34,
                            double m41, double m42, double m43, double m44)
    {
        _matrix.Set(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        return this;
    }
    
    public Matrix3D Zero()
    {
        _matrix.Zero();
        return this;
    }
    
    public Matrix3D AppendMatrix(Matrix3D matrix)
    {
        _matrix.AppendMatrix(matrix._matrix);
        return this;
    }
    
    public Matrix3D PrependMatrix(Matrix3D matrix)
    {
        _matrix.PrependMatrix(matrix._matrix);
        return this;
    }
    
    public Matrix3D Invert()
    {
        _matrix.Invert();
        return this;
    }
    
    public Matrix3D InvertCopy()
    {
        var copy = new Matrix3D(_matrix.Clone());
        copy.Invert();
        return copy;
    }
    
    public bool IsIdentity()
    {
        return _matrix.IsIdentity();
    }
    
    public Matrix3D Clone()
    {
        return new Matrix3D(_matrix.Clone());
    }
    
    // Extension method compatibility - these will delegate to Matrix3Extensions
    public FoVector3D GetTranslation()
    {
        var translation = _matrix.GetTranslation();
        return new FoVector3D(translation.X, translation.Y, translation.Z);
    }
    
    public FoVector3D GetScale()
    {
        var scale = _matrix.GetScale();
        return new FoVector3D(scale.X, scale.Y, scale.Z);
    }
    
    public FoVector3D GetRotation()
    {
        var rotation = _matrix.GetRotation();
        return new FoVector3D(rotation.X, rotation.Y, rotation.Z);
    }
    
    public Matrix3D SetTranslation(double x, double y, double z)
    {
        _matrix.SetTranslation(x, y, z);
        return this;
    }
    
    // Access to underlying Matrix3 for direct BlazorThreeJS integration
    public Matrix3 GetBlazorMatrix3() => _matrix;
    
    // Implicit conversion operators for seamless integration
    public static implicit operator Matrix3(Matrix3D matrix3D)
    {
        return matrix3D._matrix;
    }
    
    public static implicit operator Matrix3D(Matrix3 matrix)
    {
        return new Matrix3D(matrix);
    }
}
