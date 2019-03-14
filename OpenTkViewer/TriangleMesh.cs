using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK;

namespace OpenTkViewer
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

    public class TriangleMeshAnalyzer
    {
        private const int Cos0 = 1;
        private const double FaceTolerance = 0.1;

        private readonly TriangleMesh triangleMesh;

        public readonly Dictionary<int, Vertex3D> Vertices;
        public readonly Dictionary<int, Face3D> Faces;

        public List<Triangle3D> Triangles => triangleMesh.Triangles;

        private List<Edge3D> InternalEdges { get; }

        public IEnumerable<EdgesLineStrip> Edges { get; private set; }

        public TriangleMeshAnalyzer(TriangleMesh triangleMesh)
        {
            this.triangleMesh = triangleMesh;

            Vertices = new Dictionary<int, Vertex3D>();
            Faces = new Dictionary<int, Face3D>();
            InternalEdges = new List<Edge3D>();
        }

        public void ProcessTriangleMesh(Action<double, string> progress = null, CancellationToken? token = null)
        {
            if (triangleMesh == null)
                return;

            progress?.Invoke(0.2, null);
            foreach (var edge in triangleMesh.Edges)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                    return;

                if (edge.Triangles.Count() != 2)
                    continue;

                var leftTriangle = edge.Triangles.First();
                var rightTriangle = edge.Triangles.Last();
                var angleBetweenVectors = Vector3d.Dot(
                    leftTriangle.Normal.GetNormal(), rightTriangle.Normal.GetNormal());

                if (Math.Abs(Math.Abs(angleBetweenVectors) - Cos0) <= FaceTolerance)
                {
                    if (leftTriangle.SourceFace != null && rightTriangle.SourceFace != null)
                    {
                        if (leftTriangle.SourceFace == rightTriangle.SourceFace)
                            continue;

                        leftTriangle.SourceFace.Add(rightTriangle.SourceFace);
                        continue;
                    }

                    if (leftTriangle.SourceFace != null)
                    {
                        leftTriangle.SourceFace.Add(rightTriangle);
                        continue;
                    }

                    if (rightTriangle.SourceFace != null)
                    {
                        rightTriangle.SourceFace.Add(leftTriangle);
                        continue;
                    }

                    var face = new Face3D();
                    face.Add(leftTriangle);
                    face.Add(rightTriangle);
                }
                else
                {
                    if (leftTriangle.SourceFace == null)
                    {
                        var newFace = new Face3D();
                        newFace.Add(leftTriangle);
                    }

                    if (rightTriangle.SourceFace == null)
                    {
                        var newFace = new Face3D();
                        newFace.Add(rightTriangle);
                    }

                    InternalEdges.Add(edge);
                }
            }

            progress?.Invoke(0.6, null);
            if (token.HasValue && token.Value.IsCancellationRequested)
                return;

            CalculateFaces();
            progress?.Invoke(0.8, null);
            if (token.HasValue && token.Value.IsCancellationRequested)
                return;

            CalculateEdges();
            progress?.Invoke(0.9, null);
            if (token.HasValue && token.Value.IsCancellationRequested)
                return;

            CalculateVertices();
        }

        private void CalculateFaces()
        {
            var i = 1;
            foreach (var triangle in triangleMesh.Triangles)
            {
                if (triangle.SourceFace?.Id != 0)
                    continue;

                triangle.SourceFace.Id = i;
                Faces.Add(i, triangle.SourceFace);
                i++;
            }
        }

        private void CalculateEdges()
        {
            var edgesLineStrips = new Dictionary<long, List<EdgesLineStrip>>();

            foreach (var edge in InternalEdges)
            {
                var edgeKey = GetEdgeKey(edge);
                if (!edgesLineStrips.ContainsKey(edgeKey))
                {
                    var listStrips = new List<EdgesLineStrip> { new EdgesLineStrip(edge) };
                    edgesLineStrips.Add(edgeKey, listStrips);
                    continue;
                }

                EdgesLineStrip startsOfStripAndEdgeEqual = null;
                EdgesLineStrip stripStartAndEdgeEndEqual = null;
                EdgesLineStrip stripEndAndEdgeStartEqual = null;
                EdgesLineStrip endsStripAndEdgeEqual = null;

                var lineStrips = edgesLineStrips[edgeKey];

                foreach (var strip in lineStrips)
                {
                    if (strip.Start.Id == edge.Start.Id)
                        startsOfStripAndEdgeEqual = strip;

                    if (strip.Start.Id == edge.End.Id)
                        stripStartAndEdgeEndEqual = strip;

                    if (strip.End.Id == edge.Start.Id)
                        stripEndAndEdgeStartEqual = strip;

                    if (strip.End.Id == edge.End.Id)
                        endsStripAndEdgeEqual = strip;
                }

                if (startsOfStripAndEdgeEqual == null &&
                    stripStartAndEdgeEndEqual == null &&
                    stripEndAndEdgeStartEqual == null &&
                    endsStripAndEdgeEqual == null)
                {
                    lineStrips.Add(new EdgesLineStrip(edge));
                    continue;
                }

                if (startsOfStripAndEdgeEqual != null && (
                        startsOfStripAndEdgeEqual == stripEndAndEdgeStartEqual ||
                        startsOfStripAndEdgeEqual == endsStripAndEdgeEqual))
                {
                    startsOfStripAndEdgeEqual.CloseLineStrip(edge);
                    startsOfStripAndEdgeEqual.Type = ContourType.Closed;
                    continue;
                }

                if (stripStartAndEdgeEndEqual != null && (
                        stripStartAndEdgeEndEqual == stripEndAndEdgeStartEqual ||
                        stripStartAndEdgeEndEqual == endsStripAndEdgeEqual))
                {
                    stripStartAndEdgeEndEqual.CloseLineStrip(edge);
                    stripStartAndEdgeEndEqual.Type = ContourType.Closed;
                    continue;
                }

                if (startsOfStripAndEdgeEqual != null)
                {
                    if (stripStartAndEdgeEndEqual == null &&
                        stripEndAndEdgeStartEqual == null &&
                        endsStripAndEdgeEqual == null)
                        startsOfStripAndEdgeEqual.AppendToStart(edge.End, edge);

                    if (stripStartAndEdgeEndEqual != null)
                    {
                        startsOfStripAndEdgeEqual.AppendLineStripToStart(stripStartAndEdgeEndEqual, edge);
                        lineStrips.Remove(stripStartAndEdgeEndEqual);
                        stripStartAndEdgeEndEqual = null;
                    }

                    if (stripEndAndEdgeStartEqual != null)
                    {
                        startsOfStripAndEdgeEqual.AppendLineStripToStart(stripEndAndEdgeStartEqual, edge);
                        lineStrips.Remove(stripEndAndEdgeStartEqual);
                        stripEndAndEdgeStartEqual = null;
                    }

                    if (endsStripAndEdgeEqual != null)
                    {
                        startsOfStripAndEdgeEqual.AppendLineStripToStartReverse(endsStripAndEdgeEqual, edge);
                        lineStrips.Remove(endsStripAndEdgeEqual);
                        endsStripAndEdgeEqual = null;
                    }
                }

                if (stripStartAndEdgeEndEqual != null)
                {
                    if (stripEndAndEdgeStartEqual == null &&
                        endsStripAndEdgeEqual == null)
                        stripStartAndEdgeEndEqual.AppendToStart(edge.Start, edge);

                    if (stripEndAndEdgeStartEqual != null)
                    {
                        stripStartAndEdgeEndEqual.AppendLineStripToStartReverse(stripEndAndEdgeStartEqual, edge);
                        lineStrips.Remove(stripEndAndEdgeStartEqual);
                        stripEndAndEdgeStartEqual = null;
                    }

                    if (endsStripAndEdgeEqual != null)
                    {
                        stripStartAndEdgeEndEqual.AppendLineStripToStart(endsStripAndEdgeEqual, edge);
                        lineStrips.Remove(endsStripAndEdgeEqual);
                        endsStripAndEdgeEqual = null;
                    }
                }

                if (stripEndAndEdgeStartEqual != null)
                {

                    if (endsStripAndEdgeEqual == null)
                        stripEndAndEdgeStartEqual.AppendToEnd(edge.End, edge);

                    if (endsStripAndEdgeEqual != null)
                    {
                        stripEndAndEdgeStartEqual.AppendLineStripToEndReverse(endsStripAndEdgeEqual, edge);
                        lineStrips.Remove(endsStripAndEdgeEqual);
                        endsStripAndEdgeEqual = null;
                    }
                }

                endsStripAndEdgeEqual?.AppendToEnd(edge.Start, edge);
            }

            Edges = edgesLineStrips.Values.SelectMany(x => x);
        }

        private long GetEdgeKey(Edge3D edge)
        {
            var leftTriangleFaceHash = edge.ConnectedTriangles.First.Value.SourceFace.Id;
            var rightTriangleFaceHash = edge.ConnectedTriangles.Last.Value.SourceFace.Id;

            return leftTriangleFaceHash > rightTriangleFaceHash
                ? ((long)leftTriangleFaceHash << 32) + rightTriangleFaceHash
                : ((long)rightTriangleFaceHash << 32) + leftTriangleFaceHash;
        }

        private void CalculateVertices()
        {
            foreach (var lineStrip3D in Edges)
            {
                if (lineStrip3D.Type == ContourType.Closed)
                    continue;

                if (!Vertices.ContainsKey(lineStrip3D.Start.Id))
                    Vertices.Add(lineStrip3D.Start.Id, lineStrip3D.Start);

                if (!Vertices.ContainsKey(lineStrip3D.End.Id))
                    Vertices.Add(lineStrip3D.End.Id, lineStrip3D.End);
            }
        }
    }

    public enum ContourType
    {
        Open,
        Closed
    }

    public class LineStripElement<T, TK>
    {
        public readonly T Position;
        public readonly List<TK> Elements;

        public LineStripElement(T position, TK element)
        {
            Position = position;
            Elements = new List<TK>(2) { element };
        }
    }

    public abstract class LineStrip<T, TK> where TK : class
    {
        public readonly List<TK> Edges;

        public LinkedList<LineStripElement<T, TK>> Vertices { get; private set; }

        public ContourType Type { get; set; }

        public T Start => Vertices.First.Value.Position;

        public T End => Vertices.Last.Value.Position;

        protected LineStrip()
        {
            Type = ContourType.Open;
            Edges = new List<TK>();
            Vertices = new LinkedList<LineStripElement<T, TK>>();
        }

        public void AppendLineStripToStart(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            Vertices.First.Value.Elements.Add(edge);
            strip.Vertices.First.Value.Elements.Add(edge);
            foreach (var vertex in strip.Vertices)
                AppendToStart(vertex);
        }

        public void AppendLineStripToStartReverse(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            var current = strip.Vertices.Last;
            current.Value.Elements.Add(edge);
            Vertices.First.Value.Elements.Add(edge);
            while (current != null)
            {
                AppendToStart(current.Value);
                current = current.Previous;
            }
        }

        public void AppendToStart(T vertex, TK edge = null)
        {
            var newElement = new LineStripElement<T, TK>(vertex, edge);
            AppendToStart(newElement, edge);
        }

        private void AppendToStart(LineStripElement<T, TK> newElement, TK edge = null)
        {
            if (edge != null)
                Edges.Add(edge);

            if (InternalEquals(Start, newElement.Position))
                return;

            var firstVertex = Vertices.First;
            if (edge != null)
            {
                if (firstVertex.Value.Elements.Count == 2)
                    throw new Exception();

                firstVertex.Value.Elements.Add(edge);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (Collinear(
                newElement.Position,
                firstVertex.Value.Position,
                firstVertex.Next.Value.Position))
            {
                firstVertex.Value = newElement;
            }
            else
            {
                Vertices.AddFirst(newElement);
            }
        }

        public void AppendLineStripToEnd(LineStrip<T, TK> strip, TK edge)
        {
            AddStripEdges(strip, edge);
            Vertices.Last.Value.Elements.Add(edge);
            strip.Vertices.First.Value.Elements.Add(edge);
            foreach (var vertex in strip.Vertices)
                AppendToEnd(vertex);
        }

        public void AppendLineStripToEndReverse(LineStrip<T, TK> strip, TK edge = null)
        {
            AddStripEdges(strip, edge);
            var current = strip.Vertices.Last;
            current.Value.Elements.Add(edge);
            Vertices.Last.Value.Elements.Add(edge);
            while (current != null)
            {
                AppendToEnd(current.Value);
                current = current.Previous;
            }
        }

        public void AppendToEnd(T vertex, TK edge = null)
        {
            var newElement = new LineStripElement<T, TK>(vertex, edge);
            AppendToEnd(newElement, edge);
        }

        public void AppendToEnd(LineStripElement<T, TK> newElement, TK edge = null)
        {
            if (edge != null)
                Edges.Add(edge);

            if (InternalEquals(End, newElement.Position))
                return;

            var lastVertex = Vertices.Last;

            if (edge != null)
            {
                if (lastVertex.Value.Elements.Count == 2)
                    throw new Exception();

                lastVertex.Value.Elements.Add(edge);
            }

            // ReSharper disable once PossibleNullReferenceException
            if (Collinear(
                lastVertex.Previous.Value.Position,
                lastVertex.Value.Position,
                newElement.Position))
            {
                lastVertex.Value = newElement;
            }
            else
            {
                Vertices.AddLast(newElement);
            }
        }

        private void AddStripEdges(LineStrip<T, TK> strip, TK newEdge)
        {
            Edges.Add(newEdge);
            foreach (var edge in strip.Edges)
                Edges.Add(edge);
        }

        protected abstract bool InternalEquals(T first, T second);

        protected abstract bool Collinear(T first, T second, T third);

        public void CloseLineStrip(TK newEdge)
        {
            Vertices.First.Value.Elements.Add(newEdge);
            Vertices.Last.Value.Elements.Add(newEdge);
            Edges.Add(newEdge);
        }

        public void Reverse()
        {
            Vertices = new LinkedList<LineStripElement<T, TK>>(Vertices.Reverse());
        }
    }

    public class EdgesLineStrip : LineStrip<Vertex3D, Edge3D>
    {

        public EdgesLineStrip(Edge3D edge)
        {
            Vertices.AddFirst(new LineStripElement<Vertex3D, Edge3D>(edge.Start, edge));
            Vertices.AddLast(new LineStripElement<Vertex3D, Edge3D>(edge.End, edge));
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

    public class Face3D
    {
        //private readonly SortedDictionary<int, Outline> outlines;
        private bool isNewTrianglesAdded;
        private List<Vertex3D> vertices;

        public readonly List<Triangle3D> Triangles;

        public int Id { get; set; }

        public List<Vertex3D> Vertices
        {
            get
            {
                if (!isNewTrianglesAdded && vertices != null)
                    return vertices;

                //CalculateOutlines();
                //vertices = outlines.Values.SelectMany(x => x.Vertices.Select(y => y.Position)).ToList();
                return vertices;
            }
        }

        //public IEnumerable<Outline> Outlines
        //{
        //    get
        //    {
        //        if (isNewTrianglesAdded)
        //            CalculateOutlines();

        //        return outlines.Values;
        //    }
        //}

        public Vector3d? Normal => CalculateNormal();
        
        public Face3D()
        {
            Triangles = new List<Triangle3D>();
            //outlines = new SortedDictionary<int, Outline>();
        }

        public void Add(Triangle3D triangle)
        {
            Triangles.Add(triangle);
            triangle.SourceFace = this;
            isNewTrianglesAdded = true;
        }

        public void Add(Face3D face)
        {
            if (face == null)
                return;

            foreach (var faceTriangle in face.Triangles)
            {
                Triangles.Add(faceTriangle);
                faceTriangle.SourceFace = this;
            }

            isNewTrianglesAdded = true;
        }

        public void Merge(Face3D face)
        {
            if (face == null)
                return;

            foreach (var faceTriangle in face.Triangles)
                Triangles.Add(faceTriangle);

            isNewTrianglesAdded = true;
        }

        //public Outline GetOutline(int outlineId)
        //{
        //    if (isNewTrianglesAdded)
        //        CalculateOutlines();

        //    return outlines.ContainsKey(outlineId) ? outlines[outlineId] : null;
        //}

        //private void CalculateOutlines()
        //{
        //    outlines.Clear();
        //    var tempOutlines = new List<Outline>();
        //    TriangleMeshHelper.GetOutlines(GetOutlinesEdges(), this, ref tempOutlines);

        //    var outlineId = 1;
        //    foreach (var tempOutline in tempOutlines)
        //    {
        //        tempOutline.Id = outlineId;
        //        outlines.Add(outlineId, tempOutline);
        //        outlineId++;
        //    }

        //    isNewTrianglesAdded = false;
        //}

        private IEnumerable<EdgeNormal> GetOutlinesEdges()
        {
            var triangleHash = Triangles.ToDictionary(x => x);
            foreach (var triangle in Triangles)
            {
                foreach (var triangleEdge in triangle.Edges)
                {
                    var first = triangleEdge.ConnectedTriangles.First.Value;
                    var second = triangleEdge.ConnectedTriangles.Last.Value;

                    var containFirst = triangleHash.ContainsKey(first);
                    var containSecond = triangleHash.ContainsKey(second);

                    if (containFirst && !containSecond)
                        yield return new EdgeNormal(triangleEdge, first.Normal);

                    if (!containFirst && containSecond)
                        yield return new EdgeNormal(triangleEdge, second.Normal);
                }
            }
        }

        private Vector3d? CalculateNormal()
        {
            if (Triangles.Count == 0)
                return null;

            var normalsSum = Triangles.Aggregate(Vector3d.Zero,
                (current, faceTriangle) => current + faceTriangle.Normal.GetNormal());

            var normal = normalsSum / Triangles.Count;
            var randomNormalOfFace = Triangles.First().Normal;
            randomNormalOfFace.Normalize();
            if (Math.Abs(normal.X - randomNormalOfFace.X) < 0.1 &&
                Math.Abs(normal.Y - randomNormalOfFace.Y) < 0.1 &&
                Math.Abs(normal.Z - randomNormalOfFace.Z) < 0.1)
                return normal;

            return null;
        }
    }

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

    public class Vertex3D
    {
        public readonly int Id;
        public readonly Vector3d Position;
        public readonly LinkedList<Triangle3D> ConnectedTriangles;

        public Vertex3D(int id, Vector3d position)
        {
            Id = id;
            Position = position;
            ConnectedTriangles = new LinkedList<Triangle3D>();
        }

        public void ConnectTriangle(Triangle3D triangle)
        {
            ConnectedTriangles.AddLast(triangle);
        }

        public void DisconnectTriangle(Triangle3D triangle)
        {
            ConnectedTriangles.Remove(triangle);
        }
    }

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

    public class Triangle3D
    {
        public readonly List<Vertex3D> Vertices;
        public readonly List<Edge3D> Edges;
        public Vector3d Normal { get; set; }

        public Face3D SourceFace { get; set; }

        public Triangle3D()
        {
            Vertices = new List<Vertex3D>(3);
            Edges = new List<Edge3D>(3);
        }

        public void RecomputeNormal()
        {
            var d1 = Vertices[1].Position - Vertices[0].Position;
            var d2 = Vertices[2].Position - Vertices[1].Position;
            Normal = Vector3d.Cross(d1, d2);
            Normal.Normalize();
        }

        public void FlipDirection()
        {
            Normal = -1 * Normal;
            var v = Vertices[0];
            Vertices[0] = Vertices[1];
            Vertices[1] = v;
            var e = Edges[1];
            Edges[1] = Edges[2];
            Edges[2] = e;
        }

        public bool IsDegenerated()
        {
            return Vertices[0] == Vertices[1] || Vertices[1] == Vertices[2] || Vertices[2] == Vertices[0];
        }

        public string PositionToString()
        {
            return $"{Vertices[0].Position} {Vertices[1].Position} {Vertices[2].Position}";
        }
    }

    public class VertexStorage
    {
        public const double Epsilon = 0.0001f;
        private readonly Dictionary<int, LinkedList<Vertex3D>> list;

        public int Count { get; private set; }

        public VertexStorage()
        {
            list = new Dictionary<int, LinkedList<Vertex3D>>();
        }

        public static int VertexHash(Vector3d vector)
        {
            var a = (int)(vector.X * 4.0);
            var b = (int)vector.Y;
            var c = (int)vector.Z;
            return a ^ (b << 16) ^ (c << 8) ^ c ^ (b << 8);
        }

        public void Add(Vertex3D vertex)
        {
            Count++;
            var hash = VertexHash(vertex.Position);

            if (list.ContainsKey(hash))
            {
                list[hash].AddLast(vertex);
            }
            else
            {
                var vl = new LinkedList<Vertex3D>();
                vl.AddFirst(vertex);
                list[hash] = vl;
            }
        }

        public Vertex3D SearchPoint(Vector3d vertex)
        {
            var hash = VertexHash(vertex);
            return list.ContainsKey(hash)
                ? list[hash].FirstOrDefault(v => (vertex - v.Position).Length < Epsilon)
                : null;
        }
    }
}
