using System.Numerics;
using System.Windows.Media;

namespace PhongShadingCylinder
{
    class LightSource
    {
        public Color Intensity;
        public Vector3 Position;
        public Color AmbientColor = new Color() { R = 40, G = 40, B = 40, A = 255};
    }
}
