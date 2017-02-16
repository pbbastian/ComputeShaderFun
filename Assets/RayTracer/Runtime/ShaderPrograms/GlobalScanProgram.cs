using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct GlobalScanData
    {
        public int limit;
        public int offset;
        public ComputeBuffer buffer;
        public ComputeBuffer groupResultsBuffer;
        public ComputeBuffer dummyBuffer;

        public GlobalScanData(int limit, int offset, ComputeBuffer buffer, ComputeBuffer groupResultsBuffer, ComputeBuffer dummyBuffer)
        {
            this.limit = limit;
            this.offset = offset;
            this.buffer = buffer;
            this.groupResultsBuffer = groupResultsBuffer;
            this.dummyBuffer = dummyBuffer;
        }
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
            var groupCount = m_ScanProgram.GetGroupCount(data.limit);

            m_ScanProgram.Dispatch(new ScanData
            {
                limit = data.limit,
                offset = data.offset,
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
                limit = data.limit,
                offset = data.offset,
                perThreadBuffer = data.buffer,
                perGroupBuffer = data.groupResultsBuffer
            });
        }
    }
}
