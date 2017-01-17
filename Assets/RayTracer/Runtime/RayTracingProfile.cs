using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    [Serializable]
    public class RayTracingProfile
    {
        [SerializeField] private int m_RenderHeight = 480;

        [SerializeField] private int m_RenderWidth = 640;

        public int renderWidth
        {
            get { return m_RenderWidth; }
            set { m_RenderWidth = value; }
        }

        public int renderHeight
        {
            get { return m_RenderHeight; }
            set { m_RenderHeight = value; }
        }
    }
}
