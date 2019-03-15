using System.Collections.Generic;
using OpenTK;

namespace OpenTkViewer.Models.ModelPrimitives
{
    public class Vertex3D
    {
        public readonly int Id;
        public readonly Vector3d Position;
        public readonly List<Triangle3D> ConnectedTriangles;

        public Vertex3D(int id, Vector3d position)
        {
            Id = id;
            Position = position;
            ConnectedTriangles = new List<Triangle3D>();
        }

        public void ConnectTriangle(Triangle3D triangle)
        {
            ConnectedTriangles.Add(triangle);
        }

        public void DisconnectTriangle(Triangle3D triangle)
        {
            ConnectedTriangles.Remove(triangle);
        }
    }
}