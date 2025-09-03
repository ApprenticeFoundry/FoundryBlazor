using BlazorThreeJS.Maths;



namespace FoundryBlazor.Shape
{
    public class Face3D
    {
        /// <summary>
        /// Returns the transform (position, euler, length) for visualizing the face normal as a cylinder (Y axis aligned to normal, base at face center).
        /// </summary>
        public (Point3D position, Vector3 norm, Vector3 euler, double length) GetNormalCylinderTransform(double length = 0.4)
        {
            var position = Center;
            var normal = Normal.Normalize();
            var up = new Vector3(0, 1, 0);
            var axis = up.Cross(normal);
            double axisLength = axis.Length();
            double angle;
            if (axisLength > 1e-6)
            {
                axis = axis.Normalize();
                angle = Math.Acos(Math.Max(-1.0, Math.Min(1.0, up.Dot(normal))));
            }
            else
            {
                angle = up.Dot(normal) > 0 ? 0 : Math.PI;
                axis = new Vector3(1, 0, 0); // Arbitrary axis for 180°
            }

            // Convert axis-angle to quaternion
            double halfAngle = angle / 2.0;
            double sinHalf = Math.Sin(halfAngle);
            double qx = axis.X * sinHalf;
            double qy = axis.Y * sinHalf;
            double qz = axis.Z * sinHalf;
            double qw = Math.Cos(halfAngle);

            // Convert quaternion to Euler angles (XYZ order)
            double sqw = qw * qw;
            double sqx = qx * qx;
            double sqy = qy * qy;
            double sqz = qz * qz;

            // Roll (X-axis rotation)
            double t0 = 2.0 * (qw * qx + qy * qz);
            double t1 = 1.0 - 2.0 * (sqx + sqy);
            double roll = Math.Atan2(t0, t1);

            // Pitch (Y-axis rotation)
            double t2 = 2.0 * (qw * qy - qz * qx);
            t2 = t2 > 1.0 ? 1.0 : t2;
            t2 = t2 < -1.0 ? -1.0 : t2;
            double pitch = Math.Asin(t2);

            // Yaw (Z-axis rotation)
            double t3 = 2.0 * (qw * qz + qx * qy);
            double t4 = 1.0 - 2.0 * (sqy + sqz);
            double yaw = Math.Atan2(t3, t4);

            return (position, normal, new Vector3(roll, pitch, yaw), length);
        }

        /// <summary>
        /// Returns the transform (midpoint, euler, length) for visualizing the face normal as a line or arrow.
        /// The midpoint is at the center of the face, the euler aligns the local Z axis to the normal, and the length is as specified.
        /// </summary>
        public (Point3D midpoint, Vector3 norm, Vector3 euler, double length) GetNormalVisualizationTransform(double length)
        {
            // The normal starts at the face center and points in the direction of the normal vector.
            // The midpoint for the normal visualization is at Center + (Normal.Normalize() * (length / 2.0))
            var n = Normal.Normalize();
            var midpoint = new Point3D(
                Center.X + n.X * (length / 2.0),
                Center.Y + n.Y * (length / 2.0),
                Center.Z + n.Z * (length / 2.0)
            );
            var euler = GetEulerToNormal();
            return (midpoint, n, euler, length);
        }

        /// <summary>
        /// Returns the axis and angle (in radians) to rotate the local Z axis (0,0,1) to align with the face normal.
        /// </summary>
        public (Vector3 axis, double angle) GetAxisAngleToNormal()
        {
            // Use local Z axis as the reference for face orientation (CreateBox expects Z up)
            var localZ = new Vector3(0, 0, 1);
            var n = Normal.Normalize();
            var axis = localZ.Cross(n);
            double angle;
            if (axis.Length() > 1e-6)
            {
                axis = axis.Normalize();
                angle = Math.Acos(Math.Max(-1.0, Math.Min(1.0, localZ.Dot(n))));
            }
            else
            {
                // Parallel or anti-parallel
                axis = new Vector3(0, 1, 0); // arbitrary axis perpendicular to Z
                angle = localZ.Dot(n) > 0 ? 0 : Math.PI;
            }
            return (axis, angle);
        }

        /// <summary>
        /// Returns Euler angles (in radians) to align local Z to the face normal (approximate for axis-aligned faces).
        /// </summary>
        public Vector3 GetEulerToNormal()
        {
            var (axis, angle) = GetAxisAngleToNormal();
            // Convert axis-angle to quaternion
            double halfAngle = angle / 2.0;
            double sinHalf = Math.Sin(halfAngle);
            double qx = axis.X * sinHalf;
            double qy = axis.Y * sinHalf;
            double qz = axis.Z * sinHalf;
            double qw = Math.Cos(halfAngle);

            // Convert quaternion to Euler angles (XYZ order)
            // Reference: https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            double sqw = qw * qw;
            double sqx = qx * qx;
            double sqy = qy * qy;
            double sqz = qz * qz;

            // Roll (X-axis rotation)
            double t0 = 2.0 * (qw * qx + qy * qz);
            double t1 = 1.0 - 2.0 * (sqx + sqy);
            double roll = Math.Atan2(t0, t1);

            // Pitch (Y-axis rotation)
            double t2 = 2.0 * (qw * qy - qz * qx);
            t2 = t2 > 1.0 ? 1.0 : t2;
            t2 = t2 < -1.0 ? -1.0 : t2;
            double pitch = Math.Asin(t2);

            // Yaw (Z-axis rotation)
            double t3 = 2.0 * (qw * qz + qx * qy);
            double t4 = 1.0 - 2.0 * (sqy + sqz);
            double yaw = Math.Atan2(t3, t4);

            return new Vector3(roll, pitch, yaw);
        }

        /// <summary>
        /// Returns a tuple with the face's center and Euler rotation for visualization.
        /// </summary>
        public (Point3D position, Vector3 euler) GetTransformForVisualization()
        {
            return (Center, GetEulerToNormal());
        }

        // Width: distance between first and second vertex
        public double Width => Vertices.Count > 1 ?
            Math.Sqrt(Math.Pow(Vertices[0].X - Vertices[1].X, 2) +
                      Math.Pow(Vertices[0].Y - Vertices[1].Y, 2) +
                      Math.Pow(Vertices[0].Z - Vertices[1].Z, 2)) : 0.0;

        // Height: distance between second and third vertex
        public double Height => Vertices.Count > 2 ?
            Math.Sqrt(Math.Pow(Vertices[1].X - Vertices[2].X, 2) +
                      Math.Pow(Vertices[1].Y - Vertices[2].Y, 2) +
                      Math.Pow(Vertices[1].Z - Vertices[2].Z, 2)) : 0.0;
        public string Name { get; set; }
        public List<Point3D> Vertices { get; set; }
        public Point3D Center { get; set; }
        public Vector3 Normal { get; set; }

        public Face3D(string name, List<Point3D> vertices, Vector3 normal)
        {
            Name = name;
            Vertices = vertices;
            Center = new Point3D(
                vertices.Average(v => v.X),
                vertices.Average(v => v.Y),
                vertices.Average(v => v.Z)
            );
            Normal = normal;
        }
    }
}

