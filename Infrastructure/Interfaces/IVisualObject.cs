using System.Collections.Generic;
using Infrastructure.Models;
using OpenTK;

namespace Infrastructure.Interfaces
{
    public interface IVisualObject
    {
        Matrix4d Transformation { get; set; }

        IEnumerable<Vector3d> VerticesVectors { get; }

        IEnumerable<Vector3d> EdgesVectors { get; }

        IEnumerable<Vector3d> TrianglesVectors { get; }

        BoundingBox GetBoundingBox(Matrix4d? transformation = null);
    }
}
