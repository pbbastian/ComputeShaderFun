using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixSortProgram
    {
        RadixCountProgram m_CountProgram;
        GlobalScanProgram m_GlobalScanProgram;
        RadixHistogramProgram m_HistogramProgram;
        RadixReorderProgram m_ReorderProgram;
        ScanProgram m_ScanProgram;
        SequenceProgram m_SequenceProgram;
        ZeroProgram m_ZeroProgram;

        public RadixSortProgram(WarpSize warpSize)
        {
            m_HistogramProgram = new RadixHistogramProgram();
            m_GlobalScanProgram = new GlobalScanProgram(warpSize);
            m_ScanProgram = new ScanProgram(warpSize);
            m_CountProgram = new RadixCountProgram();
            m_ReorderProgram = new RadixReorderProgram();
            m_ZeroProgram = new ZeroProgram();
            m_SequenceProgram = new SequenceProgram();
        }

        public int GetHistogramGroupCount(int itemCount)
        {
            return m_ScanProgram.GetGroupCount(itemCount);
        }

        public void Dispatch(CommandBuffer cb, ComputeBuffer keyBuffer, ComputeBuffer keyBackBuffer, ComputeBuffer indexBuffer, ComputeBuffer indexBackBuffer, ComputeBuffer histogramBuffer, ComputeBuffer histogramGroupResultsBuffer, ComputeBuffer countBuffer, ComputeBuffer dummyBuffer, int limit)
        {
            m_SequenceProgram.Dispatch(cb, limit, indexBuffer);
            for (var i = 0; i < 8; i++)
            {
                var keyShift = i * 4;
                m_ZeroProgram.Dispatch(cb, countBuffer, 16);
                m_HistogramProgram.Dispatch(cb, keyBuffer, histogramBuffer, limit, keyShift);
                m_CountProgram.Dispatch(cb, limit, keyShift, keyBuffer, countBuffer);
                m_ScanProgram.Dispatch(cb, 0, 16, countBuffer, dummyBuffer);
                for (var j = 0; j < 16; j++)
                {
                    m_ZeroProgram.Dispatch(cb, histogramGroupResultsBuffer, GetHistogramGroupCount(limit));
                    m_GlobalScanProgram.Dispatch(cb, limit, j * limit, histogramBuffer, histogramGroupResultsBuffer, dummyBuffer);
                }

                m_ReorderProgram.Dispatch(cb, keyBuffer, keyBackBuffer, indexBuffer, indexBackBuffer, histogramBuffer, countBuffer, limit, keyShift);

                var keyTemp = keyBuffer;
                keyBuffer = keyBackBuffer;
                keyBackBuffer = keyTemp;

                var indexTemp = indexBuffer;
                indexBuffer = indexBackBuffer;
                indexBackBuffer = indexTemp;
            }
        }
    }
}
