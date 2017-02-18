using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct RadixSortData
    {
        public ComputeBuffer keyBuffer;
        public ComputeBuffer keyBackBuffer;
        public ComputeBuffer histogramBuffer;
        public ComputeBuffer histogramGroupResultsBuffer;
        public ComputeBuffer countBuffer;
        public ComputeBuffer dummyBuffer;
        public int limit;

        public RadixSortData(ComputeBuffer keyBuffer, ComputeBuffer keyBackBuffer, ComputeBuffer histogramBuffer, ComputeBuffer histogramGroupResultsBuffer, ComputeBuffer countBuffer, ComputeBuffer dummyBuffer, int limit)
        {
            this.keyBuffer = keyBuffer;
            this.keyBackBuffer = keyBackBuffer;
            this.histogramBuffer = histogramBuffer;
            this.histogramGroupResultsBuffer = histogramGroupResultsBuffer;
            this.countBuffer = countBuffer;
            this.dummyBuffer = dummyBuffer;
            this.limit = limit;
        }
    }

    public class RadixSortProgram
    {
        private RadixHistogramProgram m_HistogramProgram;
        private GlobalScanProgram[] m_GlobalScanProgram;
        private ScanProgram m_ScanProgram;
        private RadixCountProgram m_CountProgram;
        private RadixReorderProgram m_ReorderProgram;
        private ZeroProgram m_ZeroProgram;

        public RadixSortProgram(WarpSize warpSize)
        {
            m_HistogramProgram = new RadixHistogramProgram();
            m_GlobalScanProgram = new GlobalScanProgram[16];
            for (var i = 0; i < 16; i++)
                m_GlobalScanProgram[i] = new GlobalScanProgram(warpSize);
            m_ScanProgram = new ScanProgram(warpSize);
            m_CountProgram = new RadixCountProgram();
            m_ReorderProgram = new RadixReorderProgram();
            m_ZeroProgram = new ZeroProgram();
        }

        public int GetHistogramGroupCount(int itemCount)
        {
            return m_ScanProgram.GetGroupCount(itemCount);
        }

        public void Dispatch(RadixSortData data)
        {
            var keyBuffer = data.keyBuffer;
            var keyBackBuffer = data.keyBackBuffer;
            for (var i = 0; i < 1; i++)
            {
                var keyShift = i * 4;
                m_ZeroProgram.Dispatch(data.countBuffer, 16);
                m_HistogramProgram.Dispatch(new RadixHistogramData(keyBuffer, data.histogramBuffer, data.limit, keyShift));
                m_CountProgram.Dispatch(new RadixCountData(data.limit, keyShift, keyBuffer, data.countBuffer));
                m_ScanProgram.Dispatch(new ScanData(0, 16, data.countBuffer, data.dummyBuffer));
                for (var j = 0; j < 16; j++)
                {
                    m_ZeroProgram.Dispatch(data.histogramGroupResultsBuffer, GetHistogramGroupCount(data.limit));
                    m_GlobalScanProgram[i].Dispatch(new GlobalScanData(data.limit, j * data.limit, data.histogramBuffer, data.histogramGroupResultsBuffer, data.dummyBuffer));
                }
                //return;
                m_ReorderProgram.Dispatch(new RadixReorderData(keyBuffer, keyBackBuffer, data.histogramBuffer, data.countBuffer, data.limit, keyShift));
                var temp = keyBuffer;
                keyBuffer = keyBackBuffer;
                keyBackBuffer = temp;
            }
        }
    }
}
