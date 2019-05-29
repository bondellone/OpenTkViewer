using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces;
using Infrastructure.ModelPrimitives;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using OpenTK;

namespace Infrastructure.VisualObjects
{
    public class VisualModel : IVisualObject
    {
        private readonly BoundingBox boundingBox; 
        private readonly List<Vector3d> verticesVectors;
        private readonly List<Vector3d> edgesVectors;
        private readonly List<Vector3d> trianglesVectors;

        public readonly List<Vertex3D> Vertices;
        public readonly List<EdgesLineStrip> Edges;
        public readonly List<Triangle3D> Triangles;

        public IEnumerable<Vector3d> VerticesVectors => verticesVectors;
        public IEnumerable<Vector3d> EdgesVectors => edgesVectors;
        public IEnumerable<Vector3d> TrianglesVectors => trianglesVectors;

        public Matrix4d Transformation { get; set; }

        public VisualModel(
            List<Vertex3D> vertices,
            List<EdgesLineStrip> edges,
            List<Triangle3D> triangles)
        {
            Vertices = vertices;
            verticesVectors = vertices.Select(x => x.Position).ToList();

            Edges = edges;
            edgesVectors = new List<Vector3d>();
            Vector3d? previousPosition = null;
            foreach (var edge in edges)
            {
                foreach (var edgeElement in edge.Elements)
                {
                    if (previousPosition != null)
                    {
                        edgesVectors.Add(previousPosition.Value);
                        edgesVectors.Add(edgeElement.Position.Position);
                    }

                    previousPosition = edgeElement.Position.Position;
                }

                if (edge.Type == ContourType.Closed)
                {
                    edgesVectors.Add(edge.End.Position);
                    edgesVectors.Add(edge.Start.Position);
                }

                previousPosition = null;
            }

            Triangles = triangles;
            trianglesVectors = triangles
                .SelectMany(x => x.Vertices)
                .Select(x => x.Position)
                .ToList();

            boundingBox = BoundingBox.CreateFromPoints(trianglesVectors);
        }

        public BoundingBox GetBoundingBox(Matrix4d? transformation = null)
        {
            var resultTransformation = transformation ?? Matrix4d.Identity;
            var transformedMin = 
                Vector3d.TransformPosition(boundingBox.Min, resultTransformation);
            var transformedMax =
                Vector3d.TransformPosition(boundingBox.Max, resultTransformation);
            return new BoundingBox(transformedMin, transformedMax);
        }
    }
}
