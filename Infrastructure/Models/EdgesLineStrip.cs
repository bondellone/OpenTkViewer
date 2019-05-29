using Infrastructure.ModelPrimitives;

namespace Infrastructure.Models
{
    public class EdgesLineStrip : LineStrip<Vertex3D, Edge3D>
    {
        public EdgesLineStrip(Edge3D edge)
        {
            Elements.AddFirst(new LineStripElement<Vertex3D, Edge3D>(edge.Start, edge));
            Elements.AddLast(new LineStripElement<Vertex3D, Edge3D>(edge.End, edge));
            Edges.Add(edge);
        }

        protected override bool InternalEquals(Vertex3D first, Vertex3D second)
        {
            return first.Id == second.Id;
        }

        protected override bool Collinear(Vertex3D first, Vertex3D second, Vertex3D third)
        {
            return ViewerMath.Collinear(first.Position, second.Position, third.Position);
        }
    }
}