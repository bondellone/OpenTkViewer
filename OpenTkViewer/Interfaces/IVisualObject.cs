using System.Collections.Generic;
using OpenTK;

namespace OpenTkViewer.Interfaces
{
    public interface IVisualObject
    {
        IEnumerable<Vector3d> VerticesVectors { get; }

        IEnumerable<Vector3d> EdgesVectors { get; }

        IEnumerable<Vector3d> TrianglesVectors { get; }
    }
}
