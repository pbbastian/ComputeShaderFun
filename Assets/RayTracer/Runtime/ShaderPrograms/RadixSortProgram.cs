using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixSortProgram
    {
        private RadixHistogramProgram m_HistogramProgram;
        private GlobalScanProgram m_GlobalScanProgram;
        private ScanProgram m_ScanProgram;
        private RadixCountProgram m_CountProgram;
        private RadixReorderProgram m_ReorderProgram;
        private ZeroProgram m_ZeroProgram;
        private SequenceProgram m_SequenceProgram;

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

        public void Dispatch(ComputeBuffer keyBuffer, ComputeBuffer keyBackBuffer, ComputeBuffer indexBuffer, ComputeBuffer indexBackBuffer, ComputeBuffer histogramBuffer, ComputeBuffer histogramGroupResultsBuffer, ComputeBuffer countBuffer, ComputeBuffer dummyBuffer, int limit)
        {
            m_SequenceProgram.Dispatch(limit, indexBuffer);
            for (var i = 0; i < 8; i++)
            {
                var keyShift = i * 4;
                m_ZeroProgram.Dispatch(countBuffer, 16);
                m_HistogramProgram.Dispatch(new RadixHistogramData(keyBuffer, histogramBuffer, limit, keyShift));
                m_CountProgram.Dispatch(new RadixCountData(limit, keyShift, keyBuffer, countBuffer));
                m_ScanProgram.Dispatch(new ScanData(0, 16, countBuffer, dummyBuffer));
                for (var j = 0; j < 16; j++)
                {
                    m_ZeroProgram.Dispatch(histogramGroupResultsBuffer, GetHistogramGroupCount(limit));
                    m_GlobalScanProgram.Dispatch(new GlobalScanData(limit, j * limit, histogramBuffer, histogramGroupResultsBuffer, dummyBuffer));
                }
                
                m_ReorderProgram.Dispatch(keyBuffer, keyBackBuffer, indexBuffer, indexBackBuffer, histogramBuffer, countBuffer, limit, keyShift);

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
