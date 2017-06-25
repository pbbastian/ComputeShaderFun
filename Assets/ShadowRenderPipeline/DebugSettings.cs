using System;
using UnityEngine;

namespace ShadowRenderPipeline
{
    [Serializable]
    public class DebugSettings
    {
        [SerializeField]
        bool m_DebugEnabled;

        [SerializeField]
        OutputBuffer m_OutputBuffer;

        public bool enabled
        {
            get { return m_DebugEnabled; }
            set { m_DebugEnabled = value; }
        }

        public OutputBuffer outputBuffer
        {
            get { return m_OutputBuffer; }
            set { m_OutputBuffer = value; }
        }
    }
}
