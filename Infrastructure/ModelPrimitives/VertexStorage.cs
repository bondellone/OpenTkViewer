using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Infrastructure.ModelPrimitives
{
    public class VertexStorage
    {
        public const double Epsilon = 0.0001f;
        private readonly Dictionary<int, List<Vertex3D>> list;

        public int Count { get; private set; }

        public VertexStorage()
        {
            list = new Dictionary<int, List<Vertex3D>>();
        }

        public void Add(Vertex3D vertex)
        {
            Count++;
            var hash = VertexHash(vertex.Position);
            if (list.ContainsKey(hash))
            {
                list[hash].Add(vertex);
            }
            else
            {
                list[hash] = new List<Vertex3D> { vertex };
            }
        }

        public Vertex3D SearchPoint(Vector3d vertex)
        {
            var hash = VertexHash(vertex);
            return list.ContainsKey(hash)
                ? list[hash].FirstOrDefault(v => (vertex - v.Position).Length < Epsilon)
                : null;
        }

        private int VertexHash(Vector3d vector)
        {
            var a = (int)(vector.X * 4.0);
            var b = (int)vector.Y;
            var c = (int)vector.Z;
            return a ^ (b << 16) ^ (c << 8) ^ c ^ (b << 8);
        }
    }
}