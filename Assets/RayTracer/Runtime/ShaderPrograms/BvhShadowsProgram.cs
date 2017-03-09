using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public static class BvhShadowsProgram
    {
        public static readonly ShaderParamDescriptor<Texture> MainTex = new ShaderParamDescriptor<Texture>("_MainTex");
        public static readonly ShaderParamDescriptor<Texture> CameraGBufferTexture2 = new ShaderParamDescriptor<Texture>("_CameraGBufferTexture2");
        public static readonly ShaderParamDescriptor<Texture> CameraDepthTexture = new ShaderParamDescriptor<Texture>("_CameraDepthTexture");
        public static readonly ShaderParamDescriptor<Texture> TargetTex = new ShaderParamDescriptor<Texture>("_TargetTex");
        public static readonly ShaderParamDescriptor<StructuredBuffer<AlignedBvhNode>> Nodes = new ShaderParamDescriptor<StructuredBuffer<AlignedBvhNode>>("_Nodes");
        public static readonly ShaderParamDescriptor<StructuredBuffer<IndexedTriangle>> Triangles = new ShaderParamDescriptor<StructuredBuffer<IndexedTriangle>>("_Triangles");
        public static readonly ShaderParamDescriptor<StructuredBuffer<Vector4>> Vertices = new ShaderParamDescriptor<StructuredBuffer<Vector4>>("_Vertices");
        public static readonly ShaderParamDescriptor<Matrix4x4> InverseView = new ShaderParamDescriptor<Matrix4x4>("_InverseView");
        public static readonly ShaderParamDescriptor<Matrix4x4> Projection = new ShaderParamDescriptor<Matrix4x4>("_Projection");
        public static readonly ShaderParamDescriptor<Vector3> Light = new ShaderParamDescriptor<Vector3>("_Light");
        public static readonly ShaderParamDescriptor<Vector2> Size = new ShaderParamDescriptor<Vector2>("_Size");

        public static ComputeKernel CreateKernel()
        {
            return new ComputeKernel("Shaders/BvhShadows", "BvhShadows");
        }
    }
}
