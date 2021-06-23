using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PhongShadingCylinder
{
    class FillingAlgorithm
    {
        private static NodeSortXIncreasing xMinSort = new NodeSortXIncreasing();

        public List<Vertex> CalculateInteriorVertices(Triangle triangle, int width, int height)
        {
            var vertices = new List<Vertex>();
            // very sofisticated clipping
            if (triangle.Vertices.Any(v => v.ProjectedPosition.X < -2 * width
                                        || v.ProjectedPosition.X > 3 * width
                                        || v.ProjectedPosition.Y < -2 * height
                                        || v.ProjectedPosition.Y > 3 * height))
                return vertices;

            var nodes = new List<EdgeTableNode>();
            var edgeTables = CreateEdgeTables(triangle);

            // While moving there may be a situation in which all edges are horizontal
            if (edgeTables.Values.Sum(l => l.Count) < 2)
                return vertices;

            int yMax = (int)triangle.Vertices.Select(v => v.ProjectedPosition).Max(p => p.Y);
            int y = edgeTables.Min(p => p.Key);
            Vertex leftInterpolatedBound, rightInterpolatedBound;

            while (y <= yMax)
            {
                if (edgeTables.ContainsKey(y))
                {
                    nodes.AddRange(edgeTables[y]);
                    nodes.Sort(xMinSort);
                }
                if (nodes.Count > 1)
                {
                    if (y >= 0)
                    {
                        leftInterpolatedBound = InterpolateVertex(nodes[0].Lower, nodes[0].Higher, nodes[0].StepLength * nodes[0].StepsMade);
                        rightInterpolatedBound = InterpolateVertex(nodes[1].Lower, nodes[1].Higher, nodes[1].StepLength * nodes[1].StepsMade);

                        float length = rightInterpolatedBound.ProjectedPosition.X - leftInterpolatedBound.ProjectedPosition.X;
                        length = length < 1 ? 1 : length;
                        // Interpolate between left and right
                        for (int i = 0; i < length; i++)
                        {
                            float coefficient = i / length;
                            var vertex = InterpolateVertex(leftInterpolatedBound, rightInterpolatedBound, coefficient);
                            vertex.ProjectedPosition.X = leftInterpolatedBound.ProjectedPosition.X + i;
                            vertex.ProjectedPosition.Y = y;
                            vertices.Add(vertex);
                        }
                    }

                    nodes[0].OffsetByDx();
                    nodes[1].OffsetByDx();
                    nodes[0].StepsMade++;
                    nodes[1].StepsMade++;
                }

                y++;
                nodes.RemoveAll(n => (int)n.yMax <= y);
            }

            return vertices;
        }

        private Vertex InterpolateVertex(Vertex start, Vertex end, float lineCoefficient)
        {
            var projectedPosition = (end.ProjectedPosition - start.ProjectedPosition) * lineCoefficient + start.ProjectedPosition;

            float u = GetInterpolationCoefficient(start, end, projectedPosition, lineCoefficient);
            var position = (end.Position - start.Position) * u + start.Position;
            var normal = Vector3.Normalize((end.Normal - start.Normal) * u + start.Normal);
            return new Vertex(position, normal, projectedPosition);
        }

        private float GetInterpolationCoefficient(Vertex start, Vertex end, Vector3 projectedPosition, float lineCoefficient)
        {
            if (MathF.Abs(start.ProjectedPosition.Z - end.ProjectedPosition.Z) < MathF.Pow(10, -5))
                return lineCoefficient;

            var denominator = 1 / end.ProjectedPosition.Z - 1 / start.ProjectedPosition.Z;
            var numerator = 1 / projectedPosition.Z - 1 / start.ProjectedPosition.Z;
            return numerator / denominator;
        }

        private Dictionary<int, List<EdgeTableNode>> CreateEdgeTables(Triangle triangle)
        {
            var edgeTables = new Dictionary<int, List<EdgeTableNode>>();
            int size = triangle.Vertices.Length;
            for (int i = 0; i < size; i++)
            {
                int endIndex = (i + 1) % size;
                // horizontal edges don't influence the filling, including them introduces more complex scenarios
                if ((int)triangle.Vertices[i].ProjectedPosition.Y != (int)triangle.Vertices[endIndex].ProjectedPosition.Y)
                {
                    var node = EdgeTableNode.Create(triangle.Vertices[i], triangle.Vertices[endIndex]);
                    int key = (int)node.yMin;
                    if (edgeTables.ContainsKey(key))
                        edgeTables[key].Add(node);
                    else
                        edgeTables[key] = new List<EdgeTableNode>() { node };
                }

            }
            foreach (var entry in edgeTables)
                entry.Value.Sort(xMinSort);

            return edgeTables;
        }
    }

    class NodeSortXIncreasing : IComparer<EdgeTableNode>
    {
        public int Compare(EdgeTableNode x, EdgeTableNode y)
        {
            var result = x.xMin.CompareTo(y.xMin);
            if (result == 0)
                return x.dx.CompareTo(y.dx);
            return result;
        }
    }

    class EdgeTableNode
    {
        public float yMax { get; private set; }

        public float yMin { get; private set; }

        public float xMin { get; private set; }

        public float dx { get; private set; }

        public Vertex Lower { get; private set; }

        public Vertex Higher { get; private set; }

        public int StepsMade { get; set; }

        public float StepLength { get; private set; }

        public static EdgeTableNode Create(Vertex start, Vertex end)
        {
            (var lower, var higher) = OrderVerticesProjectedYIncreasing(start, end);
            float dy = higher.ProjectedPosition.Y - lower.ProjectedPosition.Y;
            float dx = higher.ProjectedPosition.X - lower.ProjectedPosition.X;
            return new EdgeTableNode
            {
                yMax = higher.ProjectedPosition.Y,
                yMin = lower.ProjectedPosition.Y,
                xMin = lower.ProjectedPosition.X,
                Lower = lower,
                Higher = higher,
                dx = dx / dy,
                StepLength = 1 / dy
            };
        }

        public void OffsetByDx()
        {
            xMin += dx;
        }

        private static (Vertex start, Vertex end) OrderVerticesProjectedYIncreasing(Vertex start, Vertex end) => start.ProjectedPosition.Y > end.ProjectedPosition.Y ? (end, start) : (start, end);

    }
}
