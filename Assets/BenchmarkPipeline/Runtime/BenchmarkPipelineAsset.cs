using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.BenchPipeline.Runtime
{
    [ExecuteInEditMode]
    public class BenchmarkPipelineAsset : RenderPipelineAsset
    {
        public ComputeShader scanShader;

        public ComputeShader groupAddShader;

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new BenchmarkPipeline(this);
        }
    }
}
