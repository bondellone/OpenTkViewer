using System.Collections.Generic;
using OpenTK;

namespace Infrastructure.ModelPrimitives
{
    public class Triangle3D
    {
        public readonly List<Vertex3D> Vertices;
        public readonly List<Edge3D> Edges;
        public Vector3d Normal { get; set; }

        public Face3D SourceFace { get; set; }

        public Triangle3D()
        {
            Vertices = new List<Vertex3D>(3);
            Edges = new List<Edge3D>(3);
        }

        public void RecomputeNormal()
        {
            var d1 = Vertices[1].Position - Vertices[0].Position;
            var d2 = Vertices[2].Position - Vertices[1].Position;
            Normal = Vector3d.Cross(d1, d2);
            Normal.Normalize();
        }

        public void FlipDirection()
        {
            Normal = -1 * Normal;
            var v = Vertices[0];
            Vertices[0] = Vertices[1];
            Vertices[1] = v;
            var e = Edges[1];
            Edges[1] = Edges[2];
            Edges[2] = e;
        }

        public bool IsDegenerated()
        {
            return Vertices[0] == Vertices[1] || Vertices[1] == Vertices[2] || Vertices[2] == Vertices[0];
        }
    }
}