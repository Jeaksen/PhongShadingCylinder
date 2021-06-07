using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    class CameraTransformer
    {
        public static Vector4 Transform(Vector4 input, Vector3 cameraPosition, Vector3 cameraRotation)
        {
            return Vector4.Transform(input, TransformMatrix(cameraPosition, cameraRotation));
        }

        public static Matrix4x4 TransformMatrix(Vector3 cameraPosition, Vector3 cameraRotation)
        {
            var matrix = Matrix4x4.Identity;
            matrix = matrix * Translator.TranslationMatrix(-cameraPosition);
            return matrix * Rotator.RotationMatrix(cameraRotation);
        }
    }
}
