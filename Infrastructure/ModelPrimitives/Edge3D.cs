using System;
using System.Collections.Generic;

namespace Infrastructure.ModelPrimitives
{
    public class Edge3D
    {
        public readonly Vertex3D Start;
        public readonly Vertex3D End;
        public readonly LinkedList<Triangle3D> ConnectedTriangles;

        public IEnumerable<Triangle3D> Triangles => ConnectedTriangles;

        public Edge3D(Vertex3D start, Vertex3D end)
        {
            Start = start;
            End = end;
            ConnectedTriangles = new LinkedList<Triangle3D>();
        }

        public void ConnectTriangle(Triangle3D face)
        {
            if (ConnectedTriangles.Count == 2)
                Console.WriteLine("Triangles count > 2");

            ConnectedTriangles.AddLast(face);
        }

        public void DisconnectTriangle(Triangle3D face)
        {
            ConnectedTriangles.Remove(face);
        }
    }
}