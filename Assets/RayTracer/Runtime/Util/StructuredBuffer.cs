using System;
using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class StructuredBuffer<T> : IDisposable
        where T : struct
    {
        private ComputeBuffer m_Buffer;

        public StructuredBuffer(int count, int stride)
        {
            m_Buffer = new ComputeBuffer(count, stride);
        }

        public StructuredBuffer(int count, int stride, ComputeBufferType type)
        {
            m_Buffer = new ComputeBuffer(count, stride, type);
            
        }

        public T[] data
        {
            get
            {
                var array = new T[m_Buffer.count];
                m_Buffer.GetData(array);
                return array;
            }
            set
            {
                m_Buffer.SetData(value);
            }
        }

        public int count
        {
            get { return m_Buffer.count; }
        }

        public ComputeBuffer computeBuffer
        {
            get { return m_Buffer; }
        }

        public void GetData(T[] data)
        {
            m_Buffer.GetData(data);
        }

        public void GetData(Array data)
        {
            m_Buffer.GetData(data);
        }

        public void Dispose()
        {
            if (m_Buffer != null) m_Buffer.Dispose();
        }
    }
}
