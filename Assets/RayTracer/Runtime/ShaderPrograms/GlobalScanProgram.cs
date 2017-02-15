using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct GlobalScanData
    {
        public int length;
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
            var groupCount = m_ScanProgram.GetGroupCount(data.length);

            m_ScanProgram.Dispatch(new ScanData
            {
                limit = data.length,
                offset = 0,
                buffer = data.buffer,
                groupResultsBuffer = data.groupResultsBuffer
            });

            m_ScanProgram.Dispatch(new ScanData
            {
                limit = groupCount,
                offset = 0,
                buffer = data.groupResultsBuffer,
                groupResultsBuffer = data.dummyBuffer
            });

            m_GroupAddProgram.Dispatch(new GroupAddData
            {
                limit = data.length,
                perThreadBuffer = data.buffer,
                perGroupBuffer = data.groupResultsBuffer
            });
        }
    }
}
