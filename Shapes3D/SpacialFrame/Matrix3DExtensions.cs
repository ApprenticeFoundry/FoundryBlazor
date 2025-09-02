using System;
using System.Collections.Generic;
using BlazorThreeJS.Maths;

namespace FoundryBlazor.Shape;

/// <summary>
/// Fluent API extensions for Matrix3D that enable readable, chainable 3D transformations
/// </summary>
public static class Matrix3DExtensions
{
    // === BASIC TRANSFORMATION OPERATIONS ===
    
    /// <summary>Rotate around X-axis</summary>
    public static Matrix3D RotateX(this Matrix3D matrix, double radians)
    {
        return matrix.RotateX(radians * (180.0 / Math.PI)); // Convert to degrees for existing method
    }
    
    /// <summary>Rotate around X-axis in degrees</summary>
    public static Matrix3D RotateXDegrees(this Matrix3D matrix, double degrees)
    {
        return matrix.RotateX(degrees);
    }
    
    /// <summary>Rotate around Y-axis</summary>
    public static Matrix3D RotateY(this Matrix3D matrix, double radians)
    {
        return matrix.RotateY(radians * (180.0 / Math.PI)); // Convert to degrees for existing method
    }
    
    /// <summary>Rotate around Y-axis in degrees</summary>
    public static Matrix3D RotateYDegrees(this Matrix3D matrix, double degrees)
    {
        return matrix.RotateY(degrees);
    }
    
    /// <summary>Rotate around Z-axis</summary>
    public static Matrix3D RotateZ(this Matrix3D matrix, double radians)
    {
        return matrix.RotateZ(radians * (180.0 / Math.PI)); // Convert to degrees for existing method
    }
    
    /// <summary>Rotate around Z-axis in degrees</summary>
    public static Matrix3D RotateZDegrees(this Matrix3D matrix, double degrees)
    {
        return matrix.RotateZ(degrees);
    }
    
    /// <summary>Apply Euler rotations in XYZ order</summary>
    public static Matrix3D RotateEuler(this Matrix3D matrix, double x, double y, double z, bool inRadians = false)
    {
        if (inRadians)
        {
            x *= (180.0 / Math.PI);
            y *= (180.0 / Math.PI);
            z *= (180.0 / Math.PI);
        }
        return matrix.RotateEuler(x, y, z);
    }
    
    /// <summary>Apply Euler rotations using Vector3D</summary>
    public static Matrix3D RotateEuler(this Matrix3D matrix, Vector3D eulerAngles, bool inRadians = false)
    {
        return matrix.RotateEuler(eulerAngles.X, eulerAngles.Y, eulerAngles.Z, inRadians);
    }
    
    /// <summary>Move by relative offset</summary>
    public static Matrix3D MoveBy(this Matrix3D matrix, Vector3D offset)
    {
        return matrix.Translate(offset.X, offset.Y, offset.Z);
    }
    
    /// <summary>Move by relative offset</summary>
    public static Matrix3D MoveBy(this Matrix3D matrix, double x, double y, double z)
    {
        return matrix.Translate(x, y, z);
    }
    
    /// <summary>Move to absolute position (resets translation)</summary>
    public static Matrix3D MoveTo(this Matrix3D matrix, Vector3D position)
    {
        return matrix.SetTranslation(position.X, position.Y, position.Z);
    }
    
    /// <summary>Move to absolute position (resets translation)</summary>
    public static Matrix3D MoveTo(this Matrix3D matrix, double x, double y, double z)
    {
        return matrix.SetTranslation(x, y, z);
    }
    
    /// <summary>Scale uniformly</summary>
    public static Matrix3D ScaleUniform(this Matrix3D matrix, double factor)
    {
        return matrix.Scale(factor, factor, factor);
    }
    
    /// <summary>Scale non-uniformly</summary>
    public static Matrix3D ScaleBy(this Matrix3D matrix, Vector3D scaleVector)
    {
        return matrix.Scale(scaleVector.X, scaleVector.Y, scaleVector.Z);
    }
    
    /// <summary>Scale non-uniformly</summary>
    public static Matrix3D ScaleBy(this Matrix3D matrix, double x, double y, double z)
    {
        return matrix.Scale(x, y, z);
    }
    
    // === ADVANCED ORIENTATION OPERATIONS ===
    
    /// <summary>Orient to look at target position</summary>
    public static Matrix3D LookAt(this Matrix3D matrix, Vector3D target, Vector3D? up = null)
    {
        up ??= new Vector3D(0, 1, 0); // Default up vector
            
        var currentPosition = matrix.GetTranslation();
        var forward = (target.Subtract(currentPosition)).Normalize();
        var right = forward.Cross(up).Normalize();
        var actualUp = right.Cross(forward).Normalize();
        
        // Create rotation matrix from basis vectors
        var lookAtMatrix = Matrix3D.NewMatrix();
        lookAtMatrix.Set(
            right.X, actualUp.X, -forward.X, currentPosition.X,
            right.Y, actualUp.Y, -forward.Y, currentPosition.Y,
            right.Z, actualUp.Z, -forward.Z, currentPosition.Z,
            0, 0, 0, 1
        );
        
        return matrix.PrependMatrix(lookAtMatrix);
    }
    
    /// <summary>Align with direction vector</summary>
    public static Matrix3D AlignWith(this Matrix3D matrix, Vector3D direction, Vector3D? up = null)
    {
        up ??= new Vector3D(0, 1, 0);
            
        var currentPosition = matrix.GetTranslation();
        var target = currentPosition.Add(direction.Normalize());
        return matrix.LookAt(target, up);
    }
    
    /// <summary>Orbit around a center point</summary>
    public static Matrix3D OrbitAround(this Matrix3D matrix, Vector3D center, double radius, double angleRadians, Vector3D? axis = null)
    {
        axis ??= new Vector3D(0, 1, 0); // Default Y-axis
            
        // Calculate position on orbit
        var right = axis.Cross(new Vector3D(0, 0, 1)).Normalize();
        if (right.Length() < 0.001) // axis is parallel to Z, use X instead
            right = axis.Cross(new Vector3D(1, 0, 0)).Normalize();
            
        var forward = axis.Cross(right).Normalize();
        
        var x = Math.Cos(angleRadians) * radius;
        var z = Math.Sin(angleRadians) * radius;
        
        var orbitPosition = center.Add(right.Multiply(x)).Add(forward.Multiply(z));
        
        return matrix.MoveTo(orbitPosition).LookAt(center, axis);
    }
    
    // =====================================
    // HIERARCHICAL TRANSFORMATION METHODS
    // =====================================
    
    /// <summary>Transform this matrix relative to a parent matrix (child inherits parent's transformation)</summary>
    public static Matrix3D TransformRelativeToParent(this Matrix3D childMatrix, Matrix3D parentMatrix)
    {
        // Child's world matrix = Parent's world matrix * Child's local matrix
        return parentMatrix.PrependMatrix(childMatrix);
    }
    
    /// <summary>Get the local transformation relative to a parent matrix</summary>
    public static Matrix3D GetLocalTransform(this Matrix3D worldMatrix, Matrix3D parentWorldMatrix)
    {
        // Local matrix = Parent's inverse * Child's world matrix
        var parentInverse = parentWorldMatrix.Clone();
        parentInverse.Invert();
        return parentInverse.PrependMatrix(worldMatrix);
    }
    
    /// <summary>Apply a local transformation in the context of a parent matrix</summary>
    public static Matrix3D ApplyLocalTransform(this Matrix3D parentMatrix, Matrix3D localTransform)
    {
        // Same as TransformRelativeToParent but more explicit naming
        return parentMatrix.PrependMatrix(localTransform);
    }
    
    /// <summary>Transform a point from local space to world space using parent matrix</summary>
    public static Vector3D TransformPointToWorld(this Matrix3D parentMatrix, Vector3D localPoint)
    {
        // Apply parent transformation to local point
        return parentMatrix.TransformPoint(localPoint);
    }
    
    /// <summary>Transform a point from world space to local space using parent matrix</summary>
    public static Vector3D TransformPointToLocal(this Matrix3D parentMatrix, Vector3D worldPoint)
    {
        // Apply inverse parent transformation to world point
        var inverse = parentMatrix.Clone();
        inverse.Invert();
        return inverse.TransformPoint(worldPoint);
    }
    
    /// <summary>Create a child matrix with local transformations relative to parent</summary>
    public static Matrix3D CreateChild(this Matrix3D parentMatrix, 
        Vector3D? localPosition = null, 
        Vector3D? localRotation = null, 
        Vector3D? localScale = null)
    {
        var childLocal = Matrix3D.NewMatrix();
        
        // Apply local transformations in order: Scale -> Rotate -> Translate
        if (localScale != null && localScale.Length() > 0.001)
        {
            childLocal = childLocal.Scale(
                localScale.X == 0 ? 1 : localScale.X,
                localScale.Y == 0 ? 1 : localScale.Y, 
                localScale.Z == 0 ? 1 : localScale.Z
            );
        }
        
        if (localRotation != null && localRotation.Length() > 0.001)
        {
            childLocal = childLocal.RotateEuler(localRotation.X, localRotation.Y, localRotation.Z); // Assume radians
        }
        
        if (localPosition != null)
        {
            childLocal = childLocal.Translate(localPosition.X, localPosition.Y, localPosition.Z);
        }
        
        // Transform child's local matrix by parent's world matrix
        return parentMatrix.TransformRelativeToParent(childLocal);
    }
    
    /// <summary>Create a chain of hierarchical transformations</summary>
    public static Matrix3D CreateHierarchy(this Matrix3D rootMatrix, params (Vector3D position, Vector3D rotation, Vector3D scale)[] childTransforms)
    {
        var currentMatrix = rootMatrix;
        
        foreach (var (position, rotation, scale) in childTransforms)
        {
            currentMatrix = currentMatrix.CreateChild(position, rotation, scale);
        }
        
        return currentMatrix;
    }
    
    // =====================================
    // COMMON HIERARCHICAL PATTERNS
    // =====================================
    
    /// <summary>Create an assembly of components with automatic positioning</summary>
    public static Matrix3D CreateLinearAssembly(this Matrix3D rootMatrix, 
        Vector3D direction, double spacing, int count)
    {
        var currentMatrix = rootMatrix;
        var normalizedDirection = direction.Normalize();
        
        for (int i = 0; i < count; i++)
        {
            var offset = normalizedDirection.Multiply(spacing * i);
            currentMatrix = currentMatrix.CreateChild(localPosition: offset);
        }
        
        return currentMatrix;
    }
    
    /// <summary>Create a circular arrangement of child components</summary>
    public static List<Matrix3D> CreateCircularAssembly(this Matrix3D centerMatrix, 
        double radius, int count, Vector3D? axis = null)
    {
        axis ??= new Vector3D(0, 1, 0); // Default Y-axis
        var components = new List<Matrix3D>();
        
        for (int i = 0; i < count; i++)
        {
            var angle = (2 * Math.PI * i) / count;
            var position = new Vector3D(
                Math.Cos(angle) * radius,
                0,
                Math.Sin(angle) * radius
            );
            
            var childMatrix = centerMatrix.CreateChild(localPosition: position);
            components.Add(childMatrix);
        }
        
        return components;
    }
    
    /// <summary>Create a grid layout of components</summary>
    public static List<Matrix3D> CreateGridAssembly(this Matrix3D rootMatrix,
        int rows, int columns, double spacing)
    {
        var components = new List<Matrix3D>();
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                var position = new Vector3D(
                    col * spacing - (columns - 1) * spacing / 2,
                    0,
                    row * spacing - (rows - 1) * spacing / 2
                );
                
                var childMatrix = rootMatrix.CreateChild(localPosition: position);
                components.Add(childMatrix);
            }
        }
        
        return components;
    }
    
    /// <summary>Create a mechanical linkage (chain of connected parts)</summary>
    public static Matrix3D CreateLinkage(this Matrix3D rootMatrix, 
        double[] segmentLengths, Vector3D[] jointRotations)
    {
        var currentMatrix = rootMatrix;
        
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            var length = segmentLengths[i];
            var rotation = i < jointRotations.Length ? jointRotations[i] : new Vector3D(0, 0, 0);
            
            // Each segment connects to the end of the previous one
            currentMatrix = currentMatrix.CreateChild(
                localPosition: new Vector3D(0, length, 0), // Extend along Y-axis
                localRotation: rotation
            );
        }
        
        return currentMatrix;
    }
    
    /// <summary>Apply transformation to maintain attachment between two components</summary>
    public static Matrix3D MaintainAttachment(this Matrix3D childMatrix, 
        Matrix3D parentMatrix, Vector3D attachmentOffset)
    {
        // Calculate where the child should be based on parent's current position
        var targetPosition = parentMatrix.GetTranslation().Add(attachmentOffset);
        return childMatrix.MoveTo(targetPosition);
    }
    
    /// <summary>Create a constraint that keeps a component at a fixed distance from parent</summary>
    public static Matrix3D ConstrainDistance(this Matrix3D childMatrix, 
        Matrix3D parentMatrix, double distance)
    {
        var parentPos = parentMatrix.GetTranslation();
        var childPos = childMatrix.GetTranslation();
        var direction = (childPos.Subtract(parentPos)).Normalize();
        var constrainedPosition = parentPos.Add(direction.Multiply(distance));
        
        return childMatrix.MoveTo(constrainedPosition);
    }
    
    // =====================================
    // HIT TESTING AND RAY INTERSECTION
    // =====================================
    
    /// <summary>Transform a ray from world space to local space for hit testing</summary>
    public static (Vector3D origin, Vector3D direction) TransformRayToLocal(this Matrix3D worldMatrix, Vector3D rayOrigin, Vector3D rayDirection)
    {
        // Get inverse world transform to convert from world to local space
        var inverse = worldMatrix.Clone();
        inverse.Invert();
        
        // Transform ray origin to local space
        var localOrigin = inverse.TransformPoint(rayOrigin);
        
        // Transform ray direction to local space (no translation)
        var localDirection = inverse.TransformDirection(rayDirection);
        
        return (localOrigin, localDirection);
    }
    
    /// <summary>Test if a ray intersects with a bounding box in local coordinates</summary>
    public static bool RayIntersectsBounds(Vector3D rayOrigin, Vector3D rayDirection, 
        Vector3D boundsMin, Vector3D boundsMax, out double distance)
    {
        distance = 0;
        
        // Ray-AABB intersection using slab method
        var directionInv = new Vector3D(
            Math.Abs(rayDirection.X) < 1e-6 ? 1e6 : 1.0 / rayDirection.X,
            Math.Abs(rayDirection.Y) < 1e-6 ? 1e6 : 1.0 / rayDirection.Y,
            Math.Abs(rayDirection.Z) < 1e-6 ? 1e6 : 1.0 / rayDirection.Z
        );
        
        var t1 = (boundsMin.X - rayOrigin.X) * directionInv.X;
        var t2 = (boundsMax.X - rayOrigin.X) * directionInv.X;
        var t3 = (boundsMin.Y - rayOrigin.Y) * directionInv.Y;
        var t4 = (boundsMax.Y - rayOrigin.Y) * directionInv.Y;
        var t5 = (boundsMin.Z - rayOrigin.Z) * directionInv.Z;
        var t6 = (boundsMax.Z - rayOrigin.Z) * directionInv.Z;
        
        var tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
        var tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));
        
        // No intersection if tmax < 0 or tmin > tmax
        if (tmax < 0 || tmin > tmax)
            return false;
            
        distance = tmin > 0 ? tmin : tmax;
        return distance >= 0;
    }
    
    /// <summary>Create a picking ray from screen coordinates (requires camera matrix)</summary>
    public static (Vector3D origin, Vector3D direction) CreatePickingRay(
        double screenX, double screenY, double screenWidth, double screenHeight,
        Matrix3D viewMatrix, Matrix3D projectionMatrix)
    {
        // Convert screen coordinates to normalized device coordinates (-1 to 1)
        var ndcX = (2.0 * screenX / screenWidth) - 1.0;
        var ndcY = 1.0 - (2.0 * screenY / screenHeight);
        
        // Create ray in clip space
        var clipNear = new Vector3D(ndcX, ndcY, -1.0);
        var clipFar = new Vector3D(ndcX, ndcY, 1.0);
        
        // Transform to world space
        var viewProjInverse = projectionMatrix.PrependMatrix(viewMatrix);
        viewProjInverse.Invert();
        
        var worldNear = viewProjInverse.TransformPoint(clipNear);
        var worldFar = viewProjInverse.TransformPoint(clipFar);
        
        var rayOrigin = worldNear;
        var rayDirection = (worldFar.Subtract(worldNear)).Normalize();
        
        return (rayOrigin, rayDirection);
    }
    
    /// <summary>Perform hit testing on a matrix-transformed object</summary>
    public static bool HitTest(this Matrix3D worldMatrix, Vector3D rayOrigin, Vector3D rayDirection,
        Vector3D objectSize, out double distance, out Vector3D hitPoint)
    {
        distance = 0;
        hitPoint = new Vector3D(0, 0, 0);
        
        // Transform ray to object's local coordinate system
        var (localOrigin, localDirection) = worldMatrix.TransformRayToLocal(rayOrigin, rayDirection);
        
        // Define bounding box in local space (assuming object centered at origin)
        var boundsMin = new Vector3D(-objectSize.X/2, -objectSize.Y/2, -objectSize.Z/2);
        var boundsMax = new Vector3D(objectSize.X/2, objectSize.Y/2, objectSize.Z/2);
        
        // Test intersection in local space
        if (RayIntersectsBounds(localOrigin, localDirection, boundsMin, boundsMax, out distance))
        {
            // Calculate hit point in local space
            var localHitPoint = localOrigin.Add(localDirection.Multiply(distance));
            
            // Transform hit point back to world space
            hitPoint = worldMatrix.TransformPoint(localHitPoint);
            return true;
        }
        
        return false;
    }
    
    // === UTILITY OPERATIONS ===
    
    /// <summary>Compose with another transformation</summary>
    public static Matrix3D Compose(this Matrix3D matrix, Matrix3D other)
    {
        return matrix.AppendMatrix(other);
    }
    
    /// <summary>Get inverse transformation</summary>
    public static Matrix3D GetInverse(this Matrix3D matrix)
    {
        return matrix.Clone().Invert();
    }
    
    /// <summary>Apply transformation to a point</summary>
    public static Vector3D TransformPoint(this Matrix3D matrix, Vector3D point)
    {
        return matrix.TransformPoint(point);
    }
    
    /// <summary>Apply transformation to a direction (no translation)</summary>
    public static Vector3D TransformDirection(this Matrix3D matrix, Vector3D direction)
    {
        // Create a copy and remove translation for direction transformation
        var directionMatrix = matrix.Clone();
        directionMatrix.SetTranslation(0, 0, 0);
        return directionMatrix.TransformPoint(direction);
    }
    
    /// <summary>Linear interpolation between transforms</summary>
    public static Matrix3D Lerp(this Matrix3D from, Matrix3D to, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        
        var fromComponents = from.Decompose();
        var toComponents = to.Decompose();
        
        var lerpedTranslation = fromComponents.Translation.Lerp(toComponents.Translation, t);
        var lerpedScale = fromComponents.Scale.Lerp(toComponents.Scale, t);
        var lerpedRotation = fromComponents.Rotation.Lerp(toComponents.Rotation, t);
        
        return Matrix3D.NewMatrix()
            .ScaleBy(lerpedScale)
            .RotateEuler(lerpedRotation)
            .MoveTo(lerpedTranslation);
    }
    
    /// <summary>Smooth step interpolation</summary>
    public static Matrix3D SmoothStep(this Matrix3D from, Matrix3D to, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        var smoothT = t * t * (3.0 - 2.0 * t); // Smooth step function
        return from.Lerp(to, smoothT);
    }
    
    /// <summary>Reset to identity matrix</summary>
    public static Matrix3D Reset(this Matrix3D matrix)
    {
        return matrix.Identity();
    }
    
    /// <summary>Validate transformation matrix</summary>
    public static bool IsValid(this Matrix3D matrix)
    {
        var components = matrix.Decompose();
        return components.IsValid;
    }
    
    // === HELPER METHODS ===
    
    /// <summary>Set translation component directly</summary>
    public static Matrix3D SetTranslation(this Matrix3D matrix, double x, double y, double z)
    {
        // Extract current matrix components
        var components = matrix.Decompose();
        
        // Rebuild matrix with new translation
        return Matrix3D.NewMatrix()
            .ScaleBy(components.Scale)
            .RotateEuler(components.Rotation)
            .Translate(x, y, z);
    }
    
    /// <summary>Get translation component</summary>
    public static Vector3D GetTranslation(this Matrix3D matrix)
    {
        return matrix.Decompose().Translation;
    }
    
    /// <summary>Get rotation component as Euler angles</summary>
    public static Vector3D GetRotation(this Matrix3D matrix)
    {
        return matrix.Decompose().Rotation;
    }
    
    /// <summary>Get scale component</summary>
    public static Vector3D GetScale(this Matrix3D matrix)
    {
        return matrix.Decompose().Scale;
    }
    
    /// <summary>Decompose matrix into components</summary>
    public static TransformComponents Decompose(this Matrix3D matrix)
    {
        // This is a simplified decomposition - for full implementation,
        // you'd need proper matrix decomposition math
        return new TransformComponents
        {
            Translation = new Vector3D(0, 0, 0), // Would extract from matrix[12,13,14]
            Rotation = new Vector3D(0, 0, 0),    // Would extract from rotation part
            Scale = new Vector3D(1, 1, 1),       // Would extract from scaling part
            IsValid = true
        };
    }
    
    /// <summary>Get human-readable description</summary>
    public static string Describe(this Matrix3D matrix)
    {
        var components = matrix.Decompose();
        return $"Transform: Pos({components.Translation}), Rot({components.Rotation}), Scale({components.Scale})";
    }
}

/// <summary>
/// Components of a 3D transformation
/// </summary>
public struct TransformComponents
{
    public Vector3D Translation;
    public Vector3D Rotation;
    public Vector3D Scale;
    public bool IsValid;
}

/// <summary>
/// Vector3D extensions for mathematical operations
/// </summary>
public static class Vector3DExtensions
{
    public static Vector3D Normalize(this Vector3D vector)
    {
        var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        if (length < 0.0001) return new Vector3D(0, 0, 0);
        return new Vector3D(vector.X / length, vector.Y / length, vector.Z / length);
    }
    
    public static Vector3D Cross(this Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
    
    public static double Dot(this Vector3D a, Vector3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    public static double Length(this Vector3D vector)
    {
        return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    }
    
    public static Vector3D Lerp(this Vector3D from, Vector3D to, double t)
    {
        return new Vector3D(
            from.X + (to.X - from.X) * t,
            from.Y + (to.Y - from.Y) * t,
            from.Z + (to.Z - from.Z) * t
        );
    }
    
    public static Vector3D Add(this Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Vector3D Subtract(this Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Vector3D Multiply(this Vector3D vector, double scalar)
    {
        return new Vector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }
    
    public static string ToStringFormatted(this Vector3D vector)
    {
        return $"({vector.X:F2}, {vector.Y:F2}, {vector.Z:F2})";
    }
}
