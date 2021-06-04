using System.Numerics;

namespace Rasterization.Transformations
{
    class Translator
    {
        public static Vector4 Translate(Vector4 input, Vector3 translations)
        {
            var matrix = new Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        translations.X, translations.Y, translations.Z, 1);
            
            return Vector4.Transform(input, matrix);
        }
    }
}
