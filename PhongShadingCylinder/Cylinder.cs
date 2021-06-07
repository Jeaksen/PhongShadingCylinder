using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhongShadingCylinder
{
    class Cylinder
    {
        public float Height { get; set; }
        public float Radius { get; set; }
        public Vector3 Position { get; set; }
        public int DivisionPointsCount { get; set; }
        public float DiffuseReflectivity { get; set; }
        public float SpecularReflectivity { get; set; }
        public float SpecularReflectionExponent { get; set; }

    }
}
