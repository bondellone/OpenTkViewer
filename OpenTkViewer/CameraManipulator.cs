using System;
using System.Windows.Forms;
using OpenTK;

namespace OpenTkViewer
{
    public class CameraManipulator
    {
        public readonly Camera Camera;

        private Vector2? mouseDownPosition;
        private double rotationSphereRadius;

        public CameraManipulator(Camera camera)
        {
            Camera = camera;
        }

        public void ChangeSize(int width, int height)
        {
            rotationSphereRadius = Math.Min(width * .45, height * .45);
            Camera.CalculateProjectionMatrix(width, height);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Middle)
                return;

            mouseDownPosition = new Vector2(e.X, e.Y);
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (mouseDownPosition == null)
                return;

            var mousePosition = new Vector2(e.X, e.Y);
            var activeRotationQuaternion =
                GetRotationForMove(rotationSphereRadius, mouseDownPosition.Value, mousePosition);

            if (activeRotationQuaternion != Quaterniond.Identity)
            {
                mouseDownPosition = mousePosition;
                Camera.RotationMatrix = Camera.RotationMatrix * ViewerMath.CreateRotation(activeRotationQuaternion);
                Camera.OnTransformChanged();
            }
        }

        private Quaterniond GetRotationForMove(double radius, Vector2 startPosition, Vector2 endPosition)
        {
            var activeRotationQuaternion = Quaterniond.Identity;

            //Map the point to the sphere
            var moveSpherePosition = MapMoveToSphere(radius, startPosition, endPosition);

            //Return the quaternion equivalent to the rotation
            //Compute the vector perpendicular to the begin and end vectors
            var rotationStart3D = Vector3d.UnitZ;
            var perp = Vector3d.Cross(rotationStart3D, moveSpherePosition);

            //Compute the length of the perpendicular vector
            double epsilon = 1.0e-5;
            if (perp.Length > epsilon)
            {
                //if its non-zero
                //We're ok, so return the perpendicular vector as the transform after all
                activeRotationQuaternion.X = perp.X;
                activeRotationQuaternion.Y = perp.Y;
                activeRotationQuaternion.Z = perp.Z;
                //In the quaternion values, w is cosine (theta / 2), where theta is the rotation angle
                activeRotationQuaternion.W = -Vector3d.Dot(rotationStart3D, moveSpherePosition);
            }

            return activeRotationQuaternion;
        }

        private Vector3d MapMoveToSphere(double radius, Vector2 startPosition, Vector2 endPosition)
        {
            var deltaFromStartPixels = endPosition - startPosition;
            var deltaOnSurface = new Vector2d(
                deltaFromStartPixels.X / radius,
                deltaFromStartPixels.Y / radius);

            var lengthOnSurfaceRadi = deltaOnSurface.Length;

            // get this rotation on the surface of the sphere about y
            var positionAboutY = Vector3d.Transform(Vector3d.UnitZ, Matrix4d.CreateRotationY(lengthOnSurfaceRadi));

            // get the angle that this distance travels around the sphere
            var angleToTravel = Math.Atan2(-deltaOnSurface.Y, deltaOnSurface.X);

            // now rotate that position about z in the direction of the screen vector
            return Vector3d.Transform(positionAboutY, Matrix4d.CreateRotationZ(angleToTravel));
        }

        public void MouseUp(MouseEventArgs e)
        {
            mouseDownPosition = null;
        }

        public void MouseWheel(MouseEventArgs e)
        {
            //var ray = wheelDelta < 0
            //    ? world.GetRayForLocalBounds(mousePosition)
            //    : world.GetRayForLocalBounds(world.ScreenCenter);

            //var hitPosition = hitTest.Invoke(ray);
            //if (hitPosition == null)
            //{
            //    var x = world.RotationCenterMatrix.M41;
            //    var y = world.RotationCenterMatrix.M42;
            //    var z = world.RotationCenterMatrix.M43;
            //    hitPosition = -new Vector3(x, y, z);
            //}

            //var toCenterDirection = hitPosition.Value - ray.origin;
            //var stepToCenter = toCenterDirection.Length * 0.1;
            //if (stepToCenter < 0.01)
            //    return;

            //toCenterDirection.Normalize();
            //var direction = Vector3.Transform(ray.directionNormal, world.RotationMatrix);
            //if (e.Delta < 0)
            //    direction *= -1;
            //camera.Translate(direction * stepToCenter);

            var direction = Vector3d.UnitZ;
            if (e.Delta < 0)
                direction *= -1;

            Camera.Translate(direction * 5);
        }
    }
}
