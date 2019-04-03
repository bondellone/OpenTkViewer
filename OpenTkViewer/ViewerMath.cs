using System;
using OpenTK;

namespace OpenTkViewer
{
    public static class ViewerMath
    {
        public static Matrix4d CreateRotation(Quaterniond q)
        {
            q.ToAxisAngle(out var axis, out var angle);
            return CreateRotation(axis, angle);
        }

        public static Matrix4d CreateRotation(Vector3d axis, double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var t = 1.0 - cos;

            axis.Normalize();

            Matrix4d result;
            result.Row0 = new Vector4d(
                t * axis.X * axis.X + cos,
                t * axis.X * axis.Y - sin * axis.Z,
                t * axis.X * axis.Z + sin * axis.Y,
                0.0);

            result.Row1 = new Vector4d(
                t * axis.X * axis.Y + sin * axis.Z,
                t * axis.Y * axis.Y + cos,
                t * axis.Y * axis.Z - sin * axis.X,
                0.0);

            result.Row2 = new Vector4d(
                t * axis.X * axis.Z - sin * axis.Y,
                t * axis.Y * axis.Z + sin * axis.X,
                t * axis.Z * axis.Z + cos,
                0.0);
            result.Row3 = Vector4d.UnitW;
            return result;
        }

        public static Matrix4d CreateScale(double scale)
        {
            return CreateScale(scale, scale, scale);
        }

        public static Matrix4d CreateScale(double x, double y, double z)
        {
            Matrix4d result;
            result.Row0 = Vector4d.UnitX * x;
            result.Row1 = Vector4d.UnitY * y;
            result.Row2 = Vector4d.UnitZ * z;
            result.Row3 = Vector4d.UnitW;
            return result;
        }

        public static bool Collinear(Vector3d a, Vector3d b, Vector3d c, double epsilon = .000001)
        {
            // Return true if a, b, and c all lie on the same line.
            return Math.Abs(Vector3d.Cross(b - a, c - a).Length) < epsilon;
        }

        public static Vector3d GetNormal(this Vector3d temp)
        {
            temp.Normalize();
            return temp;
        }

        public static Vector3d TransformPosition(this Vector3d pos, Matrix4d mat)
        {
            return new Vector3d(
                Vector3d.Dot(pos, new Vector3d(mat.Column0.Xyz)) + mat.Row3.X,
                Vector3d.Dot(pos,new Vector3d(mat.Column1.Xyz)) + mat.Row3.Y,
                Vector3d.Dot(pos, new Vector3d(mat.Column2.Xyz)) + mat.Row3.Z);
        }
    }
}
