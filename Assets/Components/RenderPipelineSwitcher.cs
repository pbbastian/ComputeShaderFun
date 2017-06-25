using System;
using System.Collections.Generic;
using ShadowRenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;

namespace Components
{
    public class RenderPipelineSwitcher : MonoBehaviour
    {
        [SerializeField]
        public List<RenderPipelineSwitchInfo> pipelines = new List<RenderPipelineSwitchInfo>();

        void Update()
        {
            foreach (var pipelineInfo in pipelines)
            {
                if (pipelineInfo.asset != null && Input.GetKey(pipelineInfo.keyCode))
                {
                    GraphicsSettings.renderPipelineAsset = pipelineInfo.asset;
                }
            }

            if (Input.GetKey(KeyCode.P))
                ScreenCapture.CaptureScreenshot(GraphicsSettings.renderPipelineAsset.name + ".png");
        }
    }

    [Serializable]
    public class RenderPipelineSwitchInfo
    {
        public KeyCode keyCode;
        public ShadowRenderPipelineAsset asset;
    }
}
