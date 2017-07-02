using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShadowRenderPipeline
{
    public static class ShadowsCompacted
    {
        public static readonly string Path = "ShadowsPrefixSum";
        public static readonly string Path4 = "ShadowsCompacted";

        public static class Kernels
        {
            public static readonly string Step1 = "Step1_Wave64";
            public static readonly string Step2 = "Step2_Wave64";
            public static readonly string Step3 = "Step3_Wave64";
            public static readonly string Step4 = "Step4_Wave64";
        }

        public static readonly string ZBufferParams = "_ZBufferParams";
        public static readonly string Light = "_Light";
        public static readonly string Projection = "_Projection";
        public static readonly string InverseView = "_InverseView";
        public static readonly string WorldToLight = "_WorldToLight";
        public static readonly string TargetTexSize = "_TargetTex_Size";
        public static readonly string NodeBuffer = "_Nodes";
        public static readonly string TriangleBuffer = "_Triangles";
        public static readonly string VertexBuffer = "_Vertices";
        public static readonly string NormalTexture = "_NormalTexture";
        public static readonly string DepthTexture = "_DepthTexture";
        public static readonly string TargetTexture = "_TargetTex";
        public static readonly string Buffer = "_Buffer";
        public static readonly string GroupResultsBuffer = "_GroupResultsBuffer";
        public static readonly string IdBuffer = "_IdBuffer";
        public static readonly string IndirectBuffer = "_IndirectBuffer";
        public static readonly string ShadowmapTexture = "_ShadowmapTexture";
    }
}
