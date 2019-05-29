using System.Collections.Generic;
using Infrastructure.ModelPrimitives;
using OpenTK;

namespace StlReader
{
    public class TriangleMesh
    {
        public List<Triangle3D> Triangles { get; set; }
        public List<Edge3D> Edges { get; set; }
        public VertexStorage Vertices { get; set; }

        public TriangleMesh()
        {
            Triangles = new List<Triangle3D>();
            Edges = new List<Edge3D>();
            Vertices = new VertexStorage();
        }

        public Triangle3D AddTriangle(Vector3d p1, Vector3d p2, Vector3d p3, Vector3d normal)
        {
            var triangle = new Triangle3D { Normal = normal };
            var v1 = GetOrCreateVertex(p1);
            var v2 = GetOrCreateVertex(p2);
            var v3 = GetOrCreateVertex(p3);
            var normalTest = new Vector3d(normal.X, normal.Y, normal.Z);

            triangle.Vertices.Add(v1);
            triangle.Vertices.Add(v2);
            triangle.Vertices.Add(v3);

            triangle.Edges.Add(GetOrCreateEdgeBetween(v1, v2));
            triangle.Edges.Add(GetOrCreateEdgeBetween(v2, v3));
            triangle.Edges.Add(GetOrCreateEdgeBetween(v3, v1));

            triangle.Edges[0].ConnectTriangle(triangle);
            triangle.Edges[1].ConnectTriangle(triangle);
            triangle.Edges[2].ConnectTriangle(triangle);

            v1.ConnectTriangle(triangle);
            v2.ConnectTriangle(triangle);
            v3.ConnectTriangle(triangle);

            triangle.RecomputeNormal();

            if (Vector3d.Dot(normalTest, triangle.Normal) < 0)
                triangle.FlipDirection();

            return InternalAddTriangle(triangle);
        }

        private Vertex3D GetOrCreateVertex(Vector3d pos)
        {
            var existingVertex = Vertices.SearchPoint(pos);
            if (existingVertex != null)
                return existingVertex;

            var newVertex = new Vertex3D(Vertices.Count + 1, pos);
            Vertices.Add(newVertex);
            return newVertex;
        }

        private Edge3D GetOrCreateEdgeBetween(Vertex3D v1, Vertex3D v2)
        {
            foreach (var face in v1.ConnectedTriangles)
            {
                for (var i = 0; i < 3; i++)
                {
                    if (v1 == face.Vertices[i] && v2 == face.Vertices[(i + 1) % 3] ||
                        v2 == face.Vertices[i] && v1 == face.Vertices[(i + 1) % 3])
                        return face.Edges[i];
                }
            }

            var newEdge = new Edge3D(v1, v2);
            Edges.Add(newEdge);
            return newEdge;
        }

        private Triangle3D InternalAddTriangle(Triangle3D triangle)
        {
            if (triangle.IsDegenerated())
            {
                triangle.Edges[0].DisconnectTriangle(triangle);
                triangle.Edges[1].DisconnectTriangle(triangle);
                triangle.Edges[2].DisconnectTriangle(triangle);
                triangle.Vertices[0].DisconnectTriangle(triangle);
                triangle.Vertices[1].DisconnectTriangle(triangle);
                triangle.Vertices[2].DisconnectTriangle(triangle);
            }
            else
            {
                Triangles.Add(triangle);
            }

            return triangle;
        }
    }
}
