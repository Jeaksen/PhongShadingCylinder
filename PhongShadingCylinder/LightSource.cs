using System.Numerics;
using System.Windows.Media;

namespace PhongShadingCylinder
{
    public class LightSource
    {
        public Color Intensity;
        public Vector3 Position;
        public Color AmbientColor;

        public Vector3 VectorTo(Vector3 from) => Vector3.Normalize(Position - from);
    }
}
