using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    [Serializable]
    public class RayTracingProfile
    {
        [SerializeField]
        private int m_Width = 640;

        [SerializeField]
        private int m_Height = 480;

        public int width
        {
            get { return m_Width; }
        }

        public int height
        {
            get { return m_Height; }
        }
    }
}
