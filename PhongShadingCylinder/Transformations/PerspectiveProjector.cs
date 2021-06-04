using System.Numerics;

namespace Rasterization.Transformations
{
    class PerspectiveProjector
    {
        public static Vector3 Project(Vector4 input, float width, float height)
        {
            float d = height / width;
            var matrix = new Matrix4x4(
                            d, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 0, -1,
                            0, 0, 1, 0);

            var projected = Vector4.Transform(input, matrix);
            return new Vector3(projected.X / projected.W,
                               projected.Y / projected.W,
                               projected.Z / projected.W);
        }
    }
}
