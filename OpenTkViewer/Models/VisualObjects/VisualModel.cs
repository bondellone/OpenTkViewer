using System.Collections.Generic;
using System.Linq;
using OpenTkViewer.Interfaces;
using OpenTkViewer.Models.ModelPrimitives;
using OpenTK;

namespace OpenTkViewer.Models.VisualObjects
{
    public class VisualModel : IVisualObject
    {
        private readonly List<Vector3d> verticesVectors;
        private readonly List<Vector3d> edgesVectors;
        private readonly List<Vector3d> trianglesVectors;

        public readonly List<Vertex3D> Vertices;
        public readonly List<EdgesLineStrip> Edges;
        public readonly List<Triangle3D> Triangles;

        public IEnumerable<Vector3d> VerticesVectors => verticesVectors;
        public IEnumerable<Vector3d> EdgesVectors => edgesVectors;
        public IEnumerable<Vector3d> TrianglesVectors => trianglesVectors;

        public VisualModel(
            List<Vertex3D> vertices,
            List<EdgesLineStrip> edges,
            List<Triangle3D> triangles)
        {
            Vertices = vertices;
            verticesVectors = vertices.Select(x => x.Position).ToList();

            Edges = edges;
            edgesVectors = edges
                .SelectMany(x => x.Elements)
                .Select(x => x.Position.Position)
                .ToList();

            Triangles = triangles;
            trianglesVectors = triangles
                .SelectMany(x => x.Vertices)
                .Select(x => x.Position)
                .ToList();
        }
    }
}
