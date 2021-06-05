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
        /// Normalizes the vector in terms of affinite coordinate
        /// </summary>
        public static Vector3 Normalize(Vector4 projected)
        {
            return new Vector3(projected.X / projected.W,
                   projected.Y / projected.W,
                   projected.Z / projected.W);
        }
    }
}
