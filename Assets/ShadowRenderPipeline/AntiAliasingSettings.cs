using System;
using UnityEngine;

namespace ShadowRenderPipeline
{
    public class AntiAliasingSettings : ScriptableObject
    {
        [SerializeField]
        bool m_Enabled;

        [SerializeField]
        Fxaa.Preset m_Preset = Fxaa.Preset.defaultPreset;

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public Fxaa.Preset preset
        {
            get { return m_Preset; }
            set { m_Preset = value; }
        }
    }
}
