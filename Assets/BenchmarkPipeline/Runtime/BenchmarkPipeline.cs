using BenchmarkPipeline.Runtime;
using RayTracer.Runtime.ShaderPrograms;
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
            m_GlobalScanProgram = new GlobalScanProgram(WarpSize.Warp32);

            m_ScanBuffer = new ComputeBuffer(k_Count, sizeof(int));
            m_GroupResultsBuffer = new ComputeBuffer(m_GlobalScanProgram.GetGroupCount(k_Count), sizeof(int));
            m_DummyBuffer = new ComputeBuffer(1, 4);

            var random = new Random(k_Seed);
            m_Input = new int[k_Count];
            for (var i = 0; i < k_Count; i++)
                m_Input[i] = random.Next(0, 2 ^ 30);
        }

        const int k_Count = 1024 * 500;
        const int k_Seed = 7867594;

        int[] m_Input;

        GlobalScanProgram m_GlobalScanProgram;
        ComputeBuffer m_ScanBuffer;
        ComputeBuffer m_GroupResultsBuffer;
        ComputeBuffer m_DummyBuffer;

        ComputeShader m_ScanShader;
        ComputeShader m_GroupAddShader;

        bool m_Initialized;

        void Initialize()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;
        }

        public void Render(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            //Initialize();
            foreach (var camera in cameras)
            {
                CullResults cullResults;
                CullResults.Cull(camera, renderContext, out cullResults);
                if (camera.cameraType == CameraType.Game && Application.isPlaying)
                {
                    m_ScanBuffer.SetData(m_Input);
                    using (var cmd = new CommandBuffer { name = "Global Scan" })
                    {
                        m_GlobalScanProgram.Dispatch(cmd, k_Count, 0, m_ScanBuffer, m_GroupResultsBuffer, m_DummyBuffer);
                        renderContext.ExecuteCommandBuffer(cmd);
                    }
                }
                var settings = new DrawRendererSettings(cullResults, camera, new ShaderPassName("Benchmark"));
                renderContext.DrawRenderers(ref settings);
                renderContext.Submit();
            }
        }

        public bool disposed { get; private set; }

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
