using System.Drawing;
using OpenTkViewer.Interfaces;
using OpenTK.Graphics.OpenGL;

namespace OpenTkViewer.Models
{
    public static class DrawHelper
    {
        public static void InitializeViewport(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }

        public static void InitializeScene(Camera camera)
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
        }

        public static void DrawOnScene(this IVisualObject visualObject)
        {
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1, 1);
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.LightGray);
            foreach (var triangleVector in visualObject.TrianglesVectors)
                GL.Vertex3(triangleVector);

            GL.End();
            GL.PolygonOffset(0, 0);
            GL.Disable(EnableCap.PolygonOffsetFill);

            GL.Color3(Color.Black);
            GL.LineWidth(3);
            GL.Begin(PrimitiveType.Lines);
            foreach (var edgeVector in visualObject.EdgesVectors)
                    GL.Vertex3(edgeVector);
            
            GL.End();
            GL.LineWidth(1);

            GL.PointSize(5);
            GL.Begin(PrimitiveType.Points);
            foreach (var vertexVector in visualObject.VerticesVectors)
                GL.Vertex3(vertexVector);

            GL.End();
            GL.PointSize(1);

            GL.PopMatrix();
        }
    }
}
