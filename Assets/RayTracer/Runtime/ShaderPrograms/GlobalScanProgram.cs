using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class GlobalScanProgram
    {
        GroupAddProgram m_GroupAddProgram;
        ScanProgram m_ScanProgram;

        public GlobalScanProgram(WarpSize warpSize)
        {
            m_ScanProgram = new ScanProgram(warpSize);
            m_GroupAddProgram = new GroupAddProgram(warpSize);
        }

        public int GetGroupCount(int itemCount)
        {
            return m_ScanProgram.GetGroupCount(itemCount);
        }

        public void Dispatch(CommandBuffer cb, int limit, int offset, ComputeBuffer buffer, ComputeBuffer groupResultsBuffer, ComputeBuffer dummyBuffer)
        {
            var groupCount = m_ScanProgram.GetGroupCount(limit);

            m_ScanProgram.Dispatch(cb, offset, limit, buffer, groupResultsBuffer);
            m_ScanProgram.Dispatch(cb, 0, groupCount, groupResultsBuffer, dummyBuffer);
            m_GroupAddProgram.Dispatch(cb, buffer, groupResultsBuffer, offset, limit);
        }
    }
}
