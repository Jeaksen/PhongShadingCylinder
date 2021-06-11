using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhongShadingCylinder
{
    class FillingAlgorithm
    {
        private static NodeSortXIncreasing xMinSort = new NodeSortXIncreasing();

        public List<Vertex> Fill(Triangle triangle, int width, int height)
        {
            var vertices = new List<Vertex>();
            if (triangle.Vertices.Any(v => v.ProjectedPosition.X < -width * 0.5f 
                                        || v.ProjectedPosition.X > 1.5f * width
                                        || v.ProjectedPosition.Y < -height * 0.5f
                                        || v.ProjectedPosition.Y > 1.5f * height))
                return vertices;
            var nodes = new List<EdgeTableNode>();
            var edgeTables = CreateEdgeTables(triangle);
            Vertex leftInterpolatedBound, rightInterpolatedBound;
            // While moving the polygon there may be a situation in which all edges are horizontal
            if (edgeTables.Count > 0)
            {
                int yMax = (int)triangle.Vertices.Select(v => v.ProjectedPosition).Max(p => p.Y);
                int xMin = (int)triangle.Vertices.Select(v => v.ProjectedPosition).Min(p => p.X);
                int y = edgeTables.Min(p => p.Key);

                while (y <= yMax)
                {
                    if (edgeTables.ContainsKey(y))
                    {
                        nodes.AddRange(edgeTables[y]);
                        nodes.Sort(xMinSort);
                    }
                    if (nodes.Count > 1)
                    {
                        leftInterpolatedBound = InterpolateVertex(nodes[0].Lower, nodes[0].Higher, nodes[0].StepLength * nodes[0].StepsMade);
                        rightInterpolatedBound = InterpolateVertex(nodes[1].Lower, nodes[1].Higher, nodes[1].StepLength * nodes[1].StepsMade);

                        var length = rightInterpolatedBound.ProjectedPosition.X - leftInterpolatedBound.ProjectedPosition.X;
                        // Interpolate between left and right
                        for (int i = 0; i < length; i++)
                        {
                            float coefficient = i / length;
                            var position = InterpolatePosition(leftInterpolatedBound, rightInterpolatedBound, coefficient);
                            var normal = Vector3.Normalize(InterpolateNormal(leftInterpolatedBound, rightInterpolatedBound, coefficient));
                            var projectedPosition = new Vector3(leftInterpolatedBound.ProjectedPosition.X + i, y, position.Z);
                            vertices.Add(new Vertex(position, normal) { ProjectedPosition = projectedPosition });
                        }

                        nodes[0].OffsetByDx();
                        nodes[1].OffsetByDx();
                        nodes[0].StepsMade++;
                        nodes[1].StepsMade++;
                    }

                    y++;
                    nodes.RemoveAll(n => (int)n.yMax <= y);
                }
            }

            return vertices;
        }

        private Vertex InterpolateVertex(Vertex start, Vertex end, float lineCoefficient)
        {
            var projectedPosition = (end.ProjectedPosition - start.ProjectedPosition) * lineCoefficient + start.ProjectedPosition;
            var position = InterpolatePosition(start, end, lineCoefficient);
            var normal = InterpolatePosition(start, end, lineCoefficient);
            var vertex = new Vertex(position, normal) { ProjectedPosition = projectedPosition };
            return vertex;
        }

        private Vector3 InterpolatePosition(Vertex start, Vertex end, float lineCoefficient)
        {
            float u = GetInterpolationCoefficient(start.ProjectedPosition, end.ProjectedPosition, lineCoefficient);

            return (end.Position - start.Position) * u + start.Position;
        }

        private Vector3 InterpolateNormal(Vertex start, Vertex end, float lineCoefficient)
        {
            float u = GetInterpolationCoefficient(start.ProjectedPosition, end.ProjectedPosition, lineCoefficient);

            return (end.Position - start.Position) * u + start.Position;
        }

        private float GetInterpolationCoefficient(Vector3 start, Vector3 end, float lineCoefficient)
        {
            float u;
            if (start.Z == end.Z)
                u = lineCoefficient;
            else
            {
                float z_t = (end.Z - start.Z) * lineCoefficient + start.Z;
                u = (1 / z_t - 1 / start.Z) / (1 / end.Z - 1 / start.Z);
            }

            return u;
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
                    if (edgeTables.ContainsKey((int)node.yMin))
                        edgeTables[(int)node.yMin].Add(node);
                    else
                        edgeTables.Add((int)node.yMin, new List<EdgeTableNode>() { node });
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
            xMin = xMin + dx;
        }

        private static (Vertex start, Vertex end) OrderVerticesProjectedYIncreasing(Vertex start, Vertex end) => start.ProjectedPosition.Y > end.ProjectedPosition.Y ? (end, start) : (start, end);

    }
}
