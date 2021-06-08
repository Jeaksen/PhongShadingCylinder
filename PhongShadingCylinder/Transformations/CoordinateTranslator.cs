using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    public class CoordinateTranslator
    {
        /// <summary>
        /// Translates the input vector from 3D scene coordinates to 2D (scene ceneter relative to top-left relative)
        /// </summary>
        public static Vector3 Translate3dTo2d(Vector3 input, float width, float height)
        {
            return new Vector3(width  / 2 * (1 - input.X),
                               height / 2 * (1 + input.Y),
                               input.Z);
        }

        public static Vector3 Translate2dTo3d(Vector3 input, float width, float height)
        {
            return new Vector3(1 - 2 * input.X / width,
                               2 * input.Y / height - 1,
                               input.Z);
        }
    }
}
