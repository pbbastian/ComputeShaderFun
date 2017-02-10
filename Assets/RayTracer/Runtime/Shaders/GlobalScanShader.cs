using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
    public struct GlobalScanData
    {
        public int itemCount;
        public ComputeBuffer buffer;
        public ComputeBuffer groupResultsBuffer;
        public ComputeBuffer dummyBuffer;
    }

    public class GlobalScanShader
    {
        private ScanShader m_ScanShader;
        private GroupAddShader m_GroupAddShader;

        public GlobalScanShader(WarpSize warpSize)
        {
            m_ScanShader = new ScanShader(warpSize);
            m_GroupAddShader = new GroupAddShader(warpSize);
        }

        public int GetGroupCount(int itemCount)
        {
            return m_ScanShader.GetGroupCount(itemCount);
        }

        public void Dispatch(GlobalScanData data)
        {
            var groupCount = m_ScanShader.GetGroupCount(data.itemCount);

            m_ScanShader.Dispatch(new ScanData
            {
                itemCount = data.itemCount,
                buffer = data.buffer,
                groupResultsBuffer = data.groupResultsBuffer
            });

            m_ScanShader.Dispatch(new ScanData
            {
                itemCount = groupCount,
                buffer = data.groupResultsBuffer,
                groupResultsBuffer = data.dummyBuffer
            });

            m_GroupAddShader.Dispatch(new GroupAddData
            {
                itemCount = data.itemCount,
                perThreadBuffer = data.buffer,
                perGroupBuffer = data.groupResultsBuffer
            });
        }
    }
}
