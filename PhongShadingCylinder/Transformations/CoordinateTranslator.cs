using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    public class CoordinateTranslator
    {
        public static Vector2 Translate3dTo2d(Vector3 input, int width, int height)
        {
            return new Vector2(width  / 2 * (1 + input.X),
                               height / 2 * (1 - input.Y));
        }
    }
}
