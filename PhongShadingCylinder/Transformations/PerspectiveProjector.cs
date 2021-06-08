using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    class PerspectiveProjector
    {
        /// <summary>
        /// Projects the input vector and normalizes it
        /// </summary>
        public static Vector3 Project(Vector4 input, float width, float height)
        {
            var projected = Vector4.Transform(input, ProjectionMatrix(width, height));
            return Normalize(projected);
        }

        public static Vector4 Inverse(Vector3 input, float width, float height)
        {
            var denermalized = Denormalize(input);
            var inversed = Vector4.Transform(denermalized, InverseMatrix(width, height));
            return inversed;
        }

        /// <summary>
        /// Returns the projection matrix for given screen width and height
        /// </summary>
        public static Matrix4x4 ProjectionMatrix(float width, float height)
        {
            float d = height / width;
            return new Matrix4x4(
                            d, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 0, -1,
                            0, 0, 1, 0);
        }

        /// <summary>
        /// Returns the inverse projection matrix for given screen width and height
        /// </summary>
        public static Matrix4x4 InverseMatrix(float width, float height)
        {
            float d = width / height;
            return new Matrix4x4(
                            d, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 0, 1,
                            0, 0, -1, 0);
        }

        /// <summary>
        /// Normalizes the vector in terms of affinite coordinate
        /// </summary>
        public static Vector3 Normalize(Vector4 projected)
        {
            return new Vector3(projected.X / projected.W,
                   projected.Y / projected.W,
                   projected.Z / projected.W);
        }

        /// <summary>
        /// Denormalizes the vector in terms of affinite coordinate
        /// </summary>
        public static Vector4 Denormalize(Vector3 input)
        {
            return new Vector4(input.X / input.Z,
                               input.Y / input.Z,
                               1,
                               1 / input.Z);
        }
    }
}
