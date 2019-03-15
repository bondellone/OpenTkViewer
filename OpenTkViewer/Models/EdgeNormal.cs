using OpenTkViewer.Models.ModelPrimitives;
using OpenTK;

namespace OpenTkViewer.Models
{
    public class EdgeNormal
    {
        public readonly Edge3D Edge;
        public readonly Vector3d Normal;

        public EdgeNormal(Edge3D edge, Vector3d normal)
        {
            Edge = edge;
            Normal = normal;
        }
    }
}