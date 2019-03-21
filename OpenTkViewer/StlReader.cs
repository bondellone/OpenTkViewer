using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTkViewer.Models;
using OpenTkViewer.Models.VisualObjects;
using OpenTK;

namespace OpenTkViewer
{
    public class StlReader
    {
        private static NumberStyles style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
        private static CultureInfo culture = CultureInfo.InvariantCulture;

        public static VisualModel Load(string fileName)
        {
            var triangleMesh = new TriangleMesh();
            using (Stream fileStream = File.OpenRead(fileName))
            {
                ParseFileContents(fileStream, ref triangleMesh);
            }

            var triangleMeshAnalyzer = new TriangleMeshAnalyzer(triangleMesh);
            triangleMeshAnalyzer.ProcessTriangleMesh();
            return new VisualModel(
                triangleMeshAnalyzer.Vertices.Values.ToList(), 
                triangleMeshAnalyzer.Edges, 
                triangleMeshAnalyzer.Triangles);
        }

        private static void ParseFileContents(Stream stlStream, ref TriangleMesh triangleMesh)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (stlStream == null)
                return;
            
            var bytesInFile = stlStream.Length;
            if (bytesInFile <= 80)
                return;
            
            var first160Bytes = new byte[160];
            stlStream.Read(first160Bytes, 0, 160);
            byte[] byteOrderMark = { 0xEF, 0xBB, 0xBF };
            var startOfString = 0;
            if (first160Bytes[0] == byteOrderMark[0] && first160Bytes[0] == byteOrderMark[0] && first160Bytes[0] == byteOrderMark[0])
                startOfString = 3;
            
            var first160BytesOfStlFile = System.Text.Encoding.UTF8.GetString(first160Bytes, startOfString, first160Bytes.Length - startOfString);
            if (first160BytesOfStlFile.StartsWith("solid") && first160BytesOfStlFile.Contains("facet"))
            {
                stlStream.Position = 0;
                var stlReader = new StreamReader(stlStream);
                var vectorIndex = 0;
                var normal = Vector3d.Zero;
                var vector0 = Vector3d.Zero;
                var vector1 = Vector3d.Zero;
                var vector2 = Vector3d.Zero;
                var line = stlReader.ReadLine();

                while (line != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("facet normal"))
                        normal = Convert(line, "facet normal");

                    if (line.StartsWith("vertex"))
                    {
                        vectorIndex++;
                        switch (vectorIndex)
                        {
                            case 1:
                                vector0 = Convert(line, "vertex");
                                break;

                            case 2:
                                vector1 = Convert(line, "vertex");
                                break;

                            case 3:
                                vector2 = Convert(line, "vertex");
                                if (!ViewerMath.Collinear(vector0, vector1, vector2))
                                    triangleMesh.AddTriangle(vector0, vector1, vector2, normal);
                                vectorIndex = 0;
                                break;
                        }
                    }
                    line = stlReader.ReadLine();
                }
            }
            else
            {
                // load it as a binary stl
                // skip the first 80 bytes
                // read in the number of triangles
                stlStream.Position = 0;
                var br = new BinaryReader(stlStream);
                var fileContents = br.ReadBytes((int)stlStream.Length);
                var currentPosition = 80;
                var numTriangles = BitConverter.ToUInt32(fileContents, currentPosition);
                long bytesForNormals = numTriangles * 3 * 4;
                long bytesForVertices = numTriangles * 3 * 4 * 3;
                long bytesForAttributs = numTriangles * 2;
                currentPosition += 4;
                var numBytesRequiredForVertexData = currentPosition + bytesForNormals + bytesForVertices + bytesForAttributs;
                if (fileContents.Length < numBytesRequiredForVertexData || numTriangles < 4)
                {
                    stlStream.Close();
                }
                var vector = new Vector3d[4];
                for (var i = 0; i < numTriangles; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        vector[j] = new Vector3d(
                            BitConverter.ToSingle(fileContents, currentPosition + 0 * 4),
                            BitConverter.ToSingle(fileContents, currentPosition + 1 * 4),
                            BitConverter.ToSingle(fileContents, currentPosition + 2 * 4));
                        currentPosition += 3 * 4;
                    }
                    currentPosition += 2; // skip the attribute
                    
                    if (!ViewerMath.Collinear(vector[1], vector[2], vector[3]))
                        triangleMesh.AddTriangle(vector[1], vector[2], vector[3], vector[0]);
                }
            }
            
            stlStream.Close();
        }

        private static Vector3d Convert(string line, string parameterName)
        {
            Vector3d vector0;
            int currentPosition = parameterName.Length;
            string number = GetNumber(line, ref currentPosition);
            double.TryParse(number, style, culture, out vector0.X);

            number = GetNumber(line, ref currentPosition);
            double.TryParse(number, style, culture, out vector0.Y);

            number = GetNumber(line, ref currentPosition);
            double.TryParse(number, style, culture, out vector0.Z);

            return vector0;
        }

        private static string GetNumber(string line, ref int currentPosition)
        {
            while (line[currentPosition] == ' ')
            {
                currentPosition++;
            }

            int numberLength = 0;
            while (currentPosition < line.Length && line[currentPosition] != ' ')
            {
                currentPosition++;
                numberLength++;
            }

            return line.Substring(currentPosition - numberLength, numberLength);
        }
    }
}
