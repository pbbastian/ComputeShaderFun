using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace Assets.RayTracer.Runtime.ShaderPrograms
{
    public class SpatialUpsamplingShader
    {
        public static readonly ShaderParamDescriptor<Texture> SourceTexture = new ShaderParamDescriptor<Texture>("_SourceTexture");
        public static readonly ShaderParamDescriptor<Texture> ShadowTexture = new ShaderParamDescriptor<Texture>("_ShadowTexture");
        public static readonly ShaderParamDescriptor<Texture> NormalTexture = new ShaderParamDescriptor<Texture>("_NormalTexture");
        public static readonly ShaderParamDescriptor<Texture> DepthTexture = new ShaderParamDescriptor<Texture>("_DepthTexture");
        public static readonly ShaderParamDescriptor<Texture> TargetTexture = new ShaderParamDescriptor<Texture>("_TargetTexture");
        public static readonly ShaderParamDescriptor<Vector2> Size = new ShaderParamDescriptor<Vector2>("_Size");
        public static readonly ShaderParamDescriptor<Matrix4x4> Projection = new ShaderParamDescriptor<Matrix4x4>("_Projection");
        public static readonly ShaderParamDescriptor<Matrix4x4> InverseView = new ShaderParamDescriptor<Matrix4x4>("_InverseView");
        public static readonly ShaderParamDescriptor<Vector4> ZBufferParams = new ShaderParamDescriptor<Vector4>("_ZBufferParams");
        public static readonly ShaderParamDescriptor<int> ThreadGroupCount = new ShaderParamDescriptor<int>("_ThreadGroupCount");
        public static readonly ShaderParamDescriptor<StructuredBuffer<int>> WorkCounter = new ShaderParamDescriptor<StructuredBuffer<int>>("_WorkCounter");

        public static ComputeKernel CreateKernel()
        {
            return new ComputeKernel("Shaders/SpatialUpsampling", "SpatialUpsampling");
        }
    }
}
