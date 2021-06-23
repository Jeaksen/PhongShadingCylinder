using System;
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
            var forward = new Vector3(
                    MathF.Cos(Rotator.AngleToRadians(-cameraRotation.X)) * MathF.Sin(Rotator.AngleToRadians(-cameraRotation.Y)),
                    MathF.Sin(Rotator.AngleToRadians(-cameraRotation.X)),                                                   
                    MathF.Cos(Rotator.AngleToRadians(-cameraRotation.X)) * MathF.Cos(Rotator.AngleToRadians(-cameraRotation.Y)));
            var up = Vector3.UnitY;
            var cZ = Vector3.Normalize(forward);
            var cX = Vector3.Normalize(Vector3.Cross(up, cZ));
            var cY = Vector3.Normalize(Vector3.Cross(cZ, cX));
            return new Matrix4x4(
                cX.X, cY.X, cZ.X, 0,
                cX.Y, cY.Y, cZ.Y, 0,
                cX.Z, cY.Z, cZ.Z, 0,
                Vector3.Dot(cX, -cameraPosition), Vector3.Dot(cY, -cameraPosition), Vector3.Dot(cZ, -cameraPosition), 1);
        }
    }
}
