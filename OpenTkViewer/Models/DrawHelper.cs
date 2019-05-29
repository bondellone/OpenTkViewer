using System.Drawing;
using Infrastructure.Interfaces;
using OpenTK.Graphics.OpenGL;

namespace OpenTkViewer.Models
{
    public static class DrawHelper
    {
        //private class LightingData
        //{
        //    public float[] AmbientLight { get; set; } = { 0.2f, 0.2f, 0.2f, 1.0f };
        //    public float[] DiffuseLight0 { get; set; } = { 0.7f, 0.7f, 0.7f, 1.0f };
        //    public float[] SpecularLight0 { get; set; } = { 0.5f, 0.5f, 0.5f, 1.0f };
        //    public float[] LightDirection0 { get; set; } = { -1, -1, 1, 0.0f };

        //    public float[] DiffuseLight1 { get; set; } = { 0.5f, 0.5f, 0.5f, 1.0f };
        //    public float[] SpecularLight1 { get; set; } = { 0.3f, 0.3f, 0.3f, 1.0f };
        //    public float[] LightDirection1 { get; set; } = { 1, 1, 1, 0.0f };
        //}

        public static void InitializeViewport(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }

        public static void InitializeScene(Camera camera)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Azure);

            //GL.ShadeModel(ShadingModel.Smooth);
            //GL.FrontFace(FrontFaceDirection.Ccw);
            //GL.CullFace(CullFaceMode.Front);

            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);

            //var lighting = new LightingData();
            //GL.Light(LightName.Light0, LightParameter.Ambient, lighting.AmbientLight);
            //GL.Light(LightName.Light0, LightParameter.Diffuse, lighting.DiffuseLight0);
            //GL.Light(LightName.Light0, LightParameter.Specular, lighting.SpecularLight0);

            //GL.Light(LightName.Light1, LightParameter.Diffuse, lighting.DiffuseLight1);
            //GL.Light(LightName.Light1, LightParameter.Specular, lighting.SpecularLight1);

            //GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);

            //GL.Enable(EnableCap.Light0);
            //GL.Enable(EnableCap.Light1);
            //GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.Normalize);
            //GL.Enable(EnableCap.Lighting);
            //GL.Enable(EnableCap.ColorMaterial);

            //var lightDirectionVector = new Vector3d(
            //    lighting.LightDirection0[0], 
            //    lighting.LightDirection0[1], 
            //    lighting.LightDirection0[2]);
            //lightDirectionVector.Normalize();
            //lighting.LightDirection0[0] = (float)lightDirectionVector.X;
            //lighting.LightDirection0[1] = (float)lightDirectionVector.Y;
            //lighting.LightDirection0[2] = (float)lightDirectionVector.Z;
            //GL.Light(LightName.Light0, LightParameter.Position, lighting.LightDirection0);
            //GL.Light(LightName.Light1, LightParameter.Position, lighting.LightDirection1);

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
            GL.Color3(Color.LightSteelBlue);
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
