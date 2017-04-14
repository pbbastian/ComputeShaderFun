using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.Util
{
    public static class CommandBufferExtensions
    {
        public static void DispatchCompute(this CommandBuffer cb, ComputeKernel kernel, int threadGroupsX, int threadGroupsY, int threadGroupsZ)
        {
            cb.DispatchCompute(kernel.shader, kernel.index, threadGroupsX, threadGroupsY, threadGroupsZ);
        }

        public static void DispatchCompute(this CommandBuffer cb, ComputeKernel kernel, ComputeBuffer argsBuffer, uint argsOffset = 0)
        {
            cb.DispatchCompute(kernel.shader, kernel.index, argsBuffer, argsOffset);
        }

        #region CommandBuffer setters

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, int val)
        {
            cb.SetComputeIntParam(kernel.shader, name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, params int[] values)
        {
            cb.SetComputeIntParams(kernel.shader, name, values);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, float val)
        {
            cb.SetComputeFloatParam(kernel.shader, name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, params float[] values)
        {
            cb.SetComputeFloatParams(kernel.shader, name, values);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, Vector2 val)
        {
            cb.SetComputeVectorParam(kernel.shader, name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, Vector3 val)
        {
            cb.SetComputeVectorParam(kernel.shader, name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, Vector4 val)
        {
            cb.SetComputeVectorParam(kernel.shader, name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, string name, Matrix4x4 val)
        {
            cb.SetComputeFloatParams(kernel.shader, name, val.m00, val.m10, val.m20, val.m30,
                                     val.m01, val.m11, val.m21, val.m31,
                                     val.m02, val.m12, val.m22, val.m32,
                                     val.m03, val.m13, val.m23, val.m33);
        }

        public static void SetBuffer(this CommandBuffer cb, ComputeKernel kernel, string name, ComputeBuffer buffer)
        {
            cb.SetComputeBufferParam(kernel.shader, kernel.index, name, buffer);
        }

        public static void SetTexture(this CommandBuffer cb, ComputeKernel kernel, string name, RenderTargetIdentifier rt)
        {
            cb.SetComputeTextureParam(kernel.shader, kernel.index, name, rt);
        }

        #endregion

        #region CommandBuffer ShaderParamDescriptor setters

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<int> descriptor, int val)
        {
            cb.SetComputeIntParam(kernel.shader, descriptor.name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<float> descriptor, float val)
        {
            cb.SetComputeFloatParam(kernel.shader, descriptor.name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<Vector2> descriptor, Vector2 val)
        {
            cb.SetComputeVectorParam(kernel.shader, descriptor.name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<Vector3> descriptor, Vector3 val)
        {
            cb.SetComputeVectorParam(kernel.shader, descriptor.name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<Vector4> descriptor, Vector4 val)
        {
            cb.SetComputeVectorParam(kernel.shader, descriptor.name, val);
        }

        public static void SetValue(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<Matrix4x4> descriptor, Matrix4x4 val)
        {
            cb.SetComputeMatrix4x4Param(kernel.shader, descriptor.name, val);
        }

        public static void SetComputeMatrix4x4Param(this CommandBuffer cb, ComputeShader shader, string name, Matrix4x4 val)
        {
            cb.SetComputeFloatParams(shader, name, val.m00, val.m10, val.m20, val.m30,
                                     val.m01, val.m11, val.m21, val.m31,
                                     val.m02, val.m12, val.m22, val.m32,
                                     val.m03, val.m13, val.m23, val.m33);
        }

        public static void SetBuffer<T>(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<StructuredBuffer<T>> descriptor, StructuredBuffer<T> buffer)
            where T : struct
        {
            cb.SetComputeBufferParam(kernel.shader, kernel.index, descriptor.name, buffer);
        }

        public static void SetTexture(this CommandBuffer cb, ComputeKernel kernel, ShaderParamDescriptor<Texture> descriptor, RenderTargetIdentifier rt)
        {
            cb.SetComputeTextureParam(kernel.shader, kernel.index, descriptor.name, rt);
        }

        #endregion
    }
}
