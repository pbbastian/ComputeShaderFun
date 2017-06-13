using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.BenchPipeline.Runtime
{
    [ExecuteInEditMode]
    public class BenchmarkPipelineAsset : RenderPipelineAsset
    {
        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new BenchmarkPipeline(this);
        }
    }
}
