using System;
using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    public class Rotator
    {
        /// <summary>
        /// Rotates the input vector by values in angles vector
        /// </summary>
        public static Vector4 Rotate(Vector4 input, Vector3 angles)
        {
            var result = input;
            if (angles.X != 0)
                result = RotateXAxis(result, AngleToRadians(angles.X));
            if (angles.Y != 0)
                result = RotateYAxis(result, AngleToRadians(angles.Y));
            if (angles.Z != 0)
                result = RotateZAxis(result, AngleToRadians(angles.Z));
            return result;
        }

        /// <summary>
        /// Returns the rotation matrix for given angles
        /// </summary>
        public static Matrix4x4 RotationMatrix(Vector3 angles)
        {
            return RotationMatrixXAxis(angles.X) * RotationMatrixYAxis(angles.Y) * RotationMatrixZAxis(angles.Z);
        }

        /// <summary>
        /// Rotates the input vector by values angle around X axis
        /// </summary>
        public static Vector4 RotateXAxis(Vector4 input, float angle)
        {
            return Vector4.Transform(input, RotationMatrixXAxis(angle));
        }

        /// <summary>
        /// Rotates the input vector by values angle around X axis
        /// </summary>
        public static Vector4 RotateYAxis(Vector4 input, float angle)
        {
            return Vector4.Transform(input, RotationMatrixYAxis(angle));
        }

        /// <summary>
        /// Rotates the input vector by values angle around X axis
        /// </summary>
        public static Vector4 RotateZAxis(Vector4 input, float angle)
        {
            return Vector4.Transform(input, RotationMatrixZAxis(angle));
        }

        /// <summary>
        /// Returns the rotation matrix for given angle around X axis
        /// </summary>
        public static Matrix4x4 RotationMatrixXAxis(float angle)
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, MathF.Cos(angle), MathF.Sin(angle), 0,
                0, -MathF.Sin(angle), MathF.Cos(angle), 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Returns the rotation matrix for given angle around Y axis
        /// </summary>
        public static Matrix4x4 RotationMatrixYAxis(float angle)
        {
            return new Matrix4x4(
                    MathF.Cos(angle), 0, MathF.Sin(angle), 0,
                    0, 1, 0, 0,
                    -MathF.Sin(angle), 0, MathF.Cos(angle), 0,
                    0, 0, 0, 1);
        }

        /// <summary>
        /// Returns the rotation matrix for given angle around Z axis
        /// </summary>
        public static Matrix4x4 RotationMatrixZAxis(float angle)
        {
            return new Matrix4x4(
                            MathF.Cos(angle), MathF.Sin(angle), 0, 0,
                            -MathF.Sin(angle), MathF.Cos(angle), 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1);
        }

        public static float AngleToRadians(float angle)
        {
            return MathF.PI * angle / 180f;
        }
    }
}
