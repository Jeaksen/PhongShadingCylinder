using System;
using System.Collections.Generic;
using System.Numerics;

namespace PhongShadingCylinder
{
    public class MeshCreator
    {
        /// <summary>
        /// Creates a traingulated Mesh for a cylinder with startPoint as the center 
        /// of the bottom cap with circleDivisionPoints points on the rim of the cylinder
        /// </summary>
        /// <param name="radius">Radius of the cylinder</param> 
        /// <param name="height">Height of the cylinder</param>
        /// <param name="startPoint">Center of the bottom cap</param>
        /// <param name="circleDivisionPoints">Center of the bottom cap</param>
        /// <returns>Mesh object triangulating a cylinder</returns>
        public Mesh CreateCylinderMesh(float radius, float height, Vector3 startPoint, int circleDivisionPoints)
        {
            var bottomCap = startPoint;
            var topCap = new Vector3(startPoint.X, startPoint.Y + height, startPoint.Z);
            var bottomCapRimPoints = new List<Vertex>();
            var bottomCapSidePoints = new List<Vertex>();
            var topCapRimPoints = new List<Vertex>();
            var topCapSidePoints = new List<Vertex>();
            var trianglesSide = new List<Triangle>();
            var trianglesBottom = new List<Triangle>();
            var trianglesTop = new List<Triangle>();
            float angle = 0;
            float deltaAngle = MathF.PI * 2 / circleDivisionPoints;
            var bottomCapNormal = new Vector3(0, -1, 0);
            var topCapNormal = new Vector3(0, 1, 0);

            var bottomCapVertex = new Vertex(bottomCap, bottomCapNormal);
            var topCapVertex = new Vertex(topCap, topCapNormal);
            // start from back most point and go counter clockwise
            for (int i = 0; i < circleDivisionPoints; i++)
            {
                float x = -MathF.Sin(angle) * radius;
                float z = MathF.Cos(angle) * radius;

                Vector3 normal = new Vector3(x, 0, z);
                Vector3 normalizedNormal = Vector3.Normalize(normal);

                var bottomRimPoint = new Vector3(bottomCap.X + x, bottomCap.Y, bottomCap.Z + z);
                var topRimPoint = new Vector3(topCap.X + x, topCap.Y, topCap.Z + z);

                bottomCapSidePoints.Add(new Vertex(bottomRimPoint, normalizedNormal));
                topCapSidePoints.Add(new Vertex(topRimPoint, normalizedNormal));

                bottomCapRimPoints.Add(new Vertex(bottomRimPoint, bottomCapNormal));
                topCapRimPoints.Add(new Vertex(topRimPoint, topCapNormal));

                angle += deltaAngle;
            }


            for (int i = 0; i < circleDivisionPoints; i++)
            {
                int idxFirst = i;
                int idxSecond = (i + 1) % circleDivisionPoints;

                var triangleBottomCap = new Triangle(bottomCapRimPoints[idxFirst], bottomCapRimPoints[idxSecond], bottomCapVertex);
                var triangleTopCap = new Triangle(topCapRimPoints[idxFirst], topCapRimPoints[idxSecond], topCapVertex);

                var triangleSide2Top1Bottom = new Triangle(topCapSidePoints[idxFirst], bottomCapSidePoints[idxFirst], topCapSidePoints[idxSecond]);
                var triangleSide2Bottom1Top = new Triangle(bottomCapSidePoints[idxFirst], bottomCapSidePoints[idxSecond], topCapSidePoints[idxSecond]);

                trianglesBottom.Add(triangleBottomCap);
                trianglesTop.Add(triangleTopCap);
                trianglesSide.Add(triangleSide2Top1Bottom);
                trianglesSide.Add(triangleSide2Bottom1Top);
            }

            var mesh = new Mesh();
            mesh.Vertices.AddRange(bottomCapSidePoints);
            mesh.Vertices.AddRange(bottomCapRimPoints);
            mesh.Vertices.AddRange(topCapRimPoints);
            mesh.Vertices.AddRange(topCapSidePoints);

            mesh.Triangles.AddRange(trianglesBottom);
            mesh.Triangles.AddRange(trianglesTop);
            mesh.Triangles.AddRange(trianglesSide);

            return mesh;
        }
    }

    public class Mesh
    {
        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<Triangle> Triangles { get; set; } = new List<Triangle>();

    }
   
    public class Triangle
    {
        public Vertex[] Vertices { get; private set; } = new Vertex[3];

        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            Vertices[0] = v1;
            Vertices[1] = v2;
            Vertices[2] = v3;
        }

        public override string ToString()
        {
            return $"1: {Vertices[0]}, 2: {Vertices[1]}, 3: {Vertices[2]}";
        }
    }

    public class Vertex
    {
        public Vector3 Position;

        public Vector3 ProjectedPosition;

        public Vector3 Normal;

        public Vertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public Vertex Clone()
        {
            return new Vertex(Position, Normal);
        }

        public override string ToString()
        {
            return $"{ProjectedPosition}, {Position} , {Normal}";
        }
    }
}
