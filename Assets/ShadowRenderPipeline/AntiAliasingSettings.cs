using System;
using UnityEngine;

namespace ShadowRenderPipeline
{
    [Serializable]
    public class AntiAliasingSettings
    {
        [SerializeField]
        bool m_AntiAliasingEnabled;

        [SerializeField]
        Fxaa.Preset m_Preset = Fxaa.Preset.defaultPreset;

        public bool enabled
        {
            get { return m_AntiAliasingEnabled; }
            set { m_AntiAliasingEnabled = value; }
        }

        public Fxaa.Preset preset
        {
            get { return m_Preset; }
            set { m_Preset = value; }
        }
    }
}
