using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct GlobalScanData
    {
        public int itemCount;
        public ComputeBuffer buffer;
        public ComputeBuffer groupResultsBuffer;
        public ComputeBuffer dummyBuffer;
    }

    public class GlobalScanProgram
    {
        private GroupAddProgram m_GroupAddProgram;
        private ScanProgram m_ScanProgram;

        public GlobalScanProgram(WarpSize warpSize)
        {
            m_ScanProgram = new ScanProgram(warpSize);
            m_GroupAddProgram = new GroupAddProgram(warpSize);
        }

        public int GetGroupCount(int itemCount)
        {
            return m_ScanProgram.GetGroupCount(itemCount);
        }

        public void Dispatch(GlobalScanData data)
        {
            var groupCount = m_ScanProgram.GetGroupCount(data.itemCount);

            m_ScanProgram.Dispatch(new ScanData
            {
                itemCount = data.itemCount,
                buffer = data.buffer,
                groupResultsBuffer = data.groupResultsBuffer
            });

            m_ScanProgram.Dispatch(new ScanData
            {
                itemCount = groupCount,
                buffer = data.groupResultsBuffer,
                groupResultsBuffer = data.dummyBuffer
            });

            m_GroupAddProgram.Dispatch(new GroupAddData
            {
                itemCount = data.itemCount,
                perThreadBuffer = data.buffer,
                perGroupBuffer = data.groupResultsBuffer
            });
        }
    }
}
