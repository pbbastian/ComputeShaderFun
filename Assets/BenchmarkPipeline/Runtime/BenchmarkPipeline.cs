using System;
using System.Runtime.CompilerServices;
using BenchmarkPipeline.Runtime;
using RayTracer.Runtime.ShaderPrograms;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Random = System.Random;

namespace Assets.BenchPipeline.Runtime
{
    public class BenchmarkPipeline : IRenderPipeline
    {
        public BenchmarkPipeline(BenchmarkPipelineAsset asset)
        {

        }

        const int k_Count = 1024*500;
        const int k_Seed = 7867594;

        int[] m_Input;

        GlobalScanProgram m_GlobalScanProgram;
        ComputeBuffer m_ScanBuffer;
        ComputeBuffer m_GroupResultsBuffer;
        ComputeBuffer m_DummyBuffer;

        void Initialize()
        {
            if (!disposed)
                return;

            Dispose();

            Debug.LogFormat("Count: {0}", k_Count);

            m_GlobalScanProgram = new GlobalScanProgram(WarpSize.Warp64);
            m_ScanBuffer = new ComputeBuffer(k_Count, sizeof(int));
            m_GroupResultsBuffer = new ComputeBuffer(m_GlobalScanProgram.GetGroupCount(k_Count), sizeof(int));
            m_DummyBuffer = new ComputeBuffer(1, 4);

            var random = new Random(k_Seed);
            m_Input = new int[k_Count];
            for (var i = 0; i < k_Count; i++)
                m_Input[i] = random.Next(0, 2 ^ 30);

            m_ScanBuffer.SetData(m_Input);

            disposed = false;
        }

        public void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            Initialize();
            foreach (var camera in cameras)
            {
                var benchmarkState = camera.gameObject.GetComponent<BenchmarkState>();
                if (camera.cameraType == CameraType.Game && benchmarkState != null && benchmarkState.benchmarkEnabled)
                {
                    m_ScanBuffer.SetData(m_Input);
                    using (var cmd = new CommandBuffer { name = "Global Scan" })
                    {
                        m_GlobalScanProgram.Dispatch(cmd, k_Count, 0, m_ScanBuffer, m_GroupResultsBuffer, m_DummyBuffer);
                        renderContext.ExecuteCommandBuffer(cmd);
                    }
                }
                renderContext.Submit();
            }
        }

        public bool disposed { get; private set; } = true;

        public void Dispose()
        {
            if (disposed)
                return;

            m_DummyBuffer.Dispose();
            m_GroupResultsBuffer.Dispose();
            m_ScanBuffer.Dispose();

            disposed = true;
        }
    }
}
