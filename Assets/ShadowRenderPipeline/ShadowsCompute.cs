using UnityEngine;

namespace ShadowRenderPipeline
{
    public static class ShadowsCompute
    {
        public static readonly string Path = "Shadows";

        public static class Kernels
        {
            public static readonly string Shadows = "Shadows";
        }

        public static readonly string NormalTexture = "_NormalTexture";
        public static readonly string DepthTexture = "_DepthTexture";
        public static readonly string TargetTexture = "_TargetTex";
        public static readonly string NodeBuffer = "_Nodes";
        public static readonly string TriangleBuffer = "_Triangles";
        public static readonly string VertexBuffer = "_Vertices";
        public static readonly string Light = "_Light";
        public static readonly string Size = "_Size";
        public static readonly string Projection = "_Projection";
        public static readonly string InverseView = "_InverseView";
        public static readonly string ZBufferParams = "_ZBufferParams";
        public static readonly string ThreadGroupCount = "_ThreadGroupCount";
        public static readonly string WorkCounter = "_WorkCounter";
    }
}
