using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    class Translator
    {
        /// <summary>
        /// Translates the input vector by values in translations vector
        /// </summary>
        public static Vector4 Translate(Vector4 input, Vector3 translations)
        {
            return Vector4.Transform(input, TranslationMatrix(translations));
        }

        /// <summary>
        /// Returns the translation matrix for given translations
        /// </summary>
        public static Matrix4x4 TranslationMatrix(Vector3 translations)
        {
            return new Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        translations.X, translations.Y, translations.Z, 1);
        }

        /// <summary>
        /// Returns the inverse translation matrix for given translations
        /// </summary>
        public static Matrix4x4 InverseMatrix(Vector3 translations)
        {
            return new Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        -translations.X, -translations.Y, -translations.Z, 1);
        }
    }
}
