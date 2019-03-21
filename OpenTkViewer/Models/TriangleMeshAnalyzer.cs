using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTkViewer.Models.Enums;
using OpenTkViewer.Models.ModelPrimitives;
using OpenTK;

namespace OpenTkViewer.Models
{
    public class TriangleMeshAnalyzer
    {
        private const int Cos0 = 1;
        private const double FaceTolerance = 0.1;

        private readonly TriangleMesh triangleMesh;

        public readonly Dictionary<int, Vertex3D> Vertices;
        public readonly Dictionary<int, Face3D> Faces;

        public List<Triangle3D> Triangles => triangleMesh.Triangles;

        private List<Edge3D> InternalEdges { get; }

        public List<EdgesLineStrip> Edges { get; private set; }

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

            Edges = edgesLineStrips.Values.SelectMany(x => x).ToList();
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
}