using System;
using System.Numerics;

namespace Rasterization.Transformations
{
    public class Rotator
    {
        public static Vector4 Rotate(Vector4 input, Vector3 angles)
        {
            var result = input;
            if (angles.X != 0)
                result = RotateXAxis(result, angleToRadians(angles.X));
            if (angles.Y != 0)
                result = RotateYAxis(result, angleToRadians(angles.Y));
            if (angles.Z != 0)
                result = RotateZAxis(result, angleToRadians(angles.Z));
            return result;
        }

        private static float angleToRadians(float angle)
        {
            return MathF.PI * angle / 360f;
        }

        private static Vector4 RotateXAxis(Vector4 input, float angle)
        {
            var matrix = new Matrix4x4(
                1, 0, 0, 0,
                0, MathF.Cos(angle), MathF.Sin(angle), 0,
                0, -MathF.Sin(angle), MathF.Cos(angle), 0,
                0, 0, 0, 1);

            return Vector4.Transform(input, matrix);
        }

        private static Vector4 RotateYAxis(Vector4 input, float angle)
        {
            var matrix = new Matrix4x4(
                    MathF.Cos(angle), 0, MathF.Sin(angle), 0,
                    0, 1, 0, 0,
                    -MathF.Sin(angle), 0, MathF.Cos(angle), 0,
                    0, 0, 0, 1);

            return Vector4.Transform(input, matrix);
        }

        private static Vector4 RotateZAxis(Vector4 input, float angle)
        {
            var matrix = new Matrix4x4(
                            MathF.Cos(angle), MathF.Sin(angle), 0, 0,
                            -MathF.Sin(angle), MathF.Cos(angle), 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);


            return Vector4.Transform(input, matrix);
        }
    }
}
