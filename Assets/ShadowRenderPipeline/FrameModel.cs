using UnityEngine;

namespace ShadowRenderPipeline
{
    public struct FrameModel
    {
        OutputBuffer m_OutputBuffer;
        bool m_ShadowsEnabled;
        bool m_AntiAliasingEnabled;
        string m_ShadowsKernelName;

        public FrameModel(ShadowRenderPipelineAsset asset, Camera camera)
        {
            if (asset.debugSettings.enabled && camera.cameraType != CameraType.SceneView)
                m_OutputBuffer = asset.debugSettings.outputBuffer;
            else
                m_OutputBuffer = OutputBuffer.Color;

            m_ShadowsEnabled =
                camera.cameraType != CameraType.SceneView &&
                asset.shadowSettings.enabled && (
                    m_OutputBuffer == OutputBuffer.Color || m_OutputBuffer == OutputBuffer.GBuffer3 || m_OutputBuffer == OutputBuffer.HybridShadows);

            m_AntiAliasingEnabled =
                asset.antiAliasingSettings.enabled && (
                    !asset.debugSettings.enabled ||
                    camera.cameraType == CameraType.SceneView);

            m_ShadowsKernelName = ShadowsCompute.Kernels.Shadows;
            if (m_OutputBuffer != OutputBuffer.HybridShadows && asset.shadowSettings.pixelCulling)
            {
                m_ShadowsKernelName = ShadowsCompute.Kernels.Shadows_PixelCulling;
                if (asset.shadowSettings.segmentCulling)
                    m_ShadowsKernelName = ShadowsCompute.Kernels.Shadows_PixelCulling_SegmentCulling;
            }
        }

        public OutputBuffer outputBuffer
        {
            get { return m_OutputBuffer; }
        }

        public bool shadowsEnabled
        {
            get { return m_ShadowsEnabled; }
        }

        public bool antiAliasingEnabled
        {
            get { return m_AntiAliasingEnabled; }
        }

        public string shadowsKernelName
        {
            get { return m_ShadowsKernelName; }
        }
    }
}
