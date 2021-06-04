using System.Numerics;

namespace PhongShadingCylinder.Transformations
{
    public class ParallelProjector
    {
        private static Matrix4x4 matrix = new Matrix4x4(
                                                1, 0, 0, 0,
                                                0, 1, 0, 0,
                                                0, 0, 0, 0,
                                                0, 0, 0, 1);

        public static Vector3 Project(Vector4 input)
        {
            var projected = Vector4.Transform(input, matrix);
            return new Vector3(projected.X, projected.Y, projected.Z);
        }

    }
}
