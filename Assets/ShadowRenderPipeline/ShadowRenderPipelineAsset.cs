using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ShadowRenderPipeline
{
    [ExecuteInEditMode]
    public class ShadowRenderPipelineAsset : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("RenderPipeline/Create ShadowRenderPipeline")]
        static void CreateBasicRenderPipeline()
        {
            var instance = CreateInstance<ShadowRenderPipelineAsset>();
            UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/ShadowRenderPipeline/ShadowRenderPipeline.asset");
        }
#endif

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new ShadowRenderPipeline();
        }
    }
}
