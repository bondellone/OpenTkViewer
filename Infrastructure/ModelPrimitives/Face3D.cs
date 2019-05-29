using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Infrastructure.ModelPrimitives
{
    public class Face3D
    {
        //private readonly SortedDictionary<int, Outline> outlines;
        //private bool isNewTrianglesAdded;
        //private List<Vertex3D> vertices;

        public readonly List<Triangle3D> Triangles;

        public int Id { get; set; }

        //public List<Vertex3D> Vertices
        //{
        //    get
        //    {
        //        //if (!isNewTrianglesAdded && vertices != null)
        //        //    return vertices;

        //        //CalculateOutlines();
        //        //vertices = outlines.Values.SelectMany(x => x.Elements.Select(y => y.Position)).ToList();
        //        //return vertices;
        //    }
        //}

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
            //isNewTrianglesAdded = true;
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

            //isNewTrianglesAdded = true;
        }

        public void Merge(Face3D face)
        {
            if (face == null)
                return;

            foreach (var faceTriangle in face.Triangles)
                Triangles.Add(faceTriangle);

            //isNewTrianglesAdded = true;
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

        //private IEnumerable<EdgeNormal> GetOutlinesEdges()
        //{
        //    var triangleHash = Triangles.ToDictionary(x => x);
        //    foreach (var triangle in Triangles)
        //    {
        //        foreach (var triangleEdge in triangle.Edges)
        //        {
        //            var first = triangleEdge.ConnectedTriangles.First.Value;
        //            var second = triangleEdge.ConnectedTriangles.Last.Value;

        //            var containFirst = triangleHash.ContainsKey(first);
        //            var containSecond = triangleHash.ContainsKey(second);

        //            if (containFirst && !containSecond)
        //                yield return new EdgeNormal(triangleEdge, first.Normal);

        //            if (!containFirst && containSecond)
        //                yield return new EdgeNormal(triangleEdge, second.Normal);
        //        }
        //    }
        //}

        private Vector3d? CalculateNormal()
        {
            if (Triangles.Count == 0)
                return null;

            var normalsSum = Triangles.Aggregate(Vector3d.Zero,
                (current, faceTriangle) => current + faceTriangle.Normal.GetNormal());

            var normal = normalsSum / Triangles.Count;
            var randomNormalOfFace = Triangles.First().Normal;
            randomNormalOfFace.Normalize();
            if (Math.Abs(normal.X - randomNormalOfFace.X) < 0.01 &&
                Math.Abs(normal.Y - randomNormalOfFace.Y) < 0.01 &&
                Math.Abs(normal.Z - randomNormalOfFace.Z) < 0.01)
                return normal;

            return null;
        }
    }
}