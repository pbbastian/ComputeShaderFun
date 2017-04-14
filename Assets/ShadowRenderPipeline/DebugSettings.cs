﻿using UnityEngine;

namespace ShadowRenderPipeline
{
    public class DebugSettings : ScriptableObject
    {
        [SerializeField]
        bool m_Enabled;

        [SerializeField]
        OutputBuffer m_OutputBuffer;

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public OutputBuffer outputBuffer
        {
            get { return m_OutputBuffer; }
            set { m_OutputBuffer = value; }
        }
    }
}
