using System;
using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class StructuredBuffer<T> : IDisposable
        where T : struct
    {
        public StructuredBuffer(int count, int stride)
        {
            computeBuffer = new ComputeBuffer(count, stride);
        }

        public StructuredBuffer(int count, int stride, ComputeBufferType type)
        {
            computeBuffer = new ComputeBuffer(count, stride, type);
        }

        [Obsolete]
        public StructuredBuffer(ComputeBuffer buffer)
        {
            computeBuffer = buffer;
        }

        public T[] data
        {
            get
            {
                var array = new T[computeBuffer.count];
                computeBuffer.GetData(array);
                return array;
            }
            set { computeBuffer.SetData(value); }
        }

        public int count
        {
            get { return computeBuffer.count; }
        }

        public ComputeBuffer computeBuffer { get; private set; }

        public void Dispose()
        {
            if (computeBuffer != null) computeBuffer.Dispose();
        }

        public void GetData(T[] data)
        {
            computeBuffer.GetData(data);
        }

        public void GetData(Array data)
        {
            computeBuffer.GetData(data);
        }

        public static implicit operator ComputeBuffer(StructuredBuffer<T> buffer)
        {
            return buffer.computeBuffer;
        }
    }
}
