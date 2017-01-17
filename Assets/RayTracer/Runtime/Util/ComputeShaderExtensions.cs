using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public static class ComputeShaderExtensions
    {
        public static void SetMatrix(this ComputeShader shader, string name, Matrix4x4 matrix)
        {
            shader.SetFloats(name,
                matrix.m00, matrix.m10, matrix.m20, matrix.m30,
                matrix.m01, matrix.m11, matrix.m21, matrix.m31,
                matrix.m02, matrix.m12, matrix.m22, matrix.m32,
                matrix.m03, matrix.m13, matrix.m23, matrix.m33);
        }
    }
}
