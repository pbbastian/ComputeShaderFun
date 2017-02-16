using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct RadixSortData
    {
        public ComputeBuffer keyBuffer;
        public ComputeBuffer histogramBuffer;
        public ComputeBuffer histogramGroupResultsBuffer;
        public ComputeBuffer countBuffer;
        public ComputeBuffer dummyBuffer;
        public int itemCount;
        public int keyShift;
    }

    public class RadixSortProgram
    {
        private RadixHistogramProgram m_HistogramProgram;
        private GlobalScanProgram m_GlobalScanProgram;
        private ScanProgram m_ScanProgram;
        private RadixCountProgram m_CountProgram;

        public RadixSortProgram(WarpSize warpSize)
        {
            m_HistogramProgram = new RadixHistogramProgram();
            m_GlobalScanProgram = new GlobalScanProgram(warpSize);
            m_ScanProgram = new ScanProgram(warpSize);
            m_CountProgram = new RadixCountProgram();
        }

        public void Dispatch(RadixSortData data)
        {
            for (var i = 0; i < 8; i++)
            {
                var keyShift = i * 4;
                m_HistogramProgram.Dispatch(new RadixHistogramData(data.keyBuffer, data.histogramBuffer, data.itemCount, data.keyShift));
                m_CountProgram.Dispatch(new RadixCountData(data.itemCount, 0xF << keyShift, keyShift, data.keyBuffer, data.countBuffer));
                m_ScanProgram.Dispatch(new ScanData(0, 16, data.countBuffer, data.dummyBuffer));
                for (var j = 0; j < 16; j++)
                {
                    m_GlobalScanProgram.Dispatch(new GlobalScanData(data.itemCount, j * data.itemCount, data.histogramBuffer, data.histogramGroupResultsBuffer, data.dummyBuffer));
                }
                // TODO: Re-order
            }
        }
    }
}
