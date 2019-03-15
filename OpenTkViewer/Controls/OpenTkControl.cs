using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OpenTkViewer.Models;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using OpenTkViewer.Models.Enums;

namespace OpenTkViewer.Controls
{
    public class OpenTkControl : WindowsFormsHost
    {
        private readonly Camera camera;
        private readonly GLControl glControl;
        private readonly TriangleMeshAnalyzer triangleMeshAnalyzer;

        private Vector2? mouseDownPosition;
        private double rotationSphereRadius;

        public OpenTkControl()
        {
            camera = new Camera(Width, Height);
            camera.Translate(new Vector3d(0, 0, -200));

            var mode = new GraphicsMode(32, 24, 0, 8);
            glControl = new GLControl(mode)
            {
                Dock = DockStyle.Fill,
                Location = new Point(0,0),
                VSync = false
            };
            Child = glControl;

            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseWheel += GlControl_MouseWheel;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseUp += GlControl_MouseUp;
            triangleMeshAnalyzer = StlReader.Load();
            glControl.Invalidate();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            rotationSphereRadius = Math.Min(glControl.Width * .45, glControl.Height * .45);
            camera.CalculateProjectionMatrix(glControl.Width, glControl.Height);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Azure);
            
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            var projectionMatrix = camera.ProjectionMatrix;
            GL.LoadMatrix(ref projectionMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
            var modelViewMatrix = camera.ModelViewMatrix;
            GL.LoadMatrix(ref modelViewMatrix);

            DrawMeshAnalyzer();

            glControl.SwapBuffers();
        }

        private void DrawMeshAnalyzer()
        {
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1,1);
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.LightGray);
            foreach (var triangle3D in triangleMeshAnalyzer.Triangles)
            {
                GL.Normal3(triangle3D.Normal);
                foreach (var vector in triangle3D.Vertices.Select(x => x.Position))
                    GL.Vertex3(vector);
            }

            GL.End();
            GL.PolygonOffset(0, 0);
            GL.Disable(EnableCap.PolygonOffsetFill);

            GL.Color3(Color.Black);
            GL.LineWidth(3);
            
            foreach (var edgesLineStrip in triangleMeshAnalyzer.Edges)
            {
                GL.Begin(PrimitiveType.LineStrip);
                foreach (var vector in edgesLineStrip.Elements.Select(x => x.Position.Position))
                    GL.Vertex3(vector);

                if (edgesLineStrip.Type == ContourType.Closed)
                    GL.Vertex3(edgesLineStrip.Elements.First.Value.Position.Position);
                GL.End();
            }
            GL.LineWidth(1);

            GL.PointSize(5);
            GL.Begin(PrimitiveType.Points);
            foreach (var vector in triangleMeshAnalyzer.Vertices.Values.Select(x => x.Position))
                GL.Vertex3(vector);

            GL.End();
            GL.PointSize(1);

            GL.PopMatrix();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Middle)
                return;

            mouseDownPosition = new Vector2(e.X, e.Y);
            glControl.Invalidate();
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownPosition == null)
                return;

            var mousePosition = new Vector2(e.X, e.Y);
            var activeRotationQuaternion =
                GetRotationForMove(rotationSphereRadius, mouseDownPosition.Value, mousePosition);

            if (activeRotationQuaternion != Quaterniond.Identity)
            {
                mouseDownPosition = mousePosition;
                camera.RotationMatrix = camera.RotationMatrix * ViewerMath.CreateRotation(activeRotationQuaternion);
                camera.OnTransformChanged();
            }

            glControl.Invalidate();
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
            var angleToTravel = Math.Atan2(deltaOnSurface.Y, deltaOnSurface.X);

            // now rotate that position about z in the direction of the screen vector
            return Vector3d.Transform(positionAboutY, Matrix4d.CreateRotationZ(angleToTravel));
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDownPosition = null;
            glControl.Invalidate();
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
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

            camera.Translate(direction * 5);
            glControl.Invalidate();
        }
    }
}