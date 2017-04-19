using System;
using UnityEngine;

namespace ShadowRenderPipeline
{
    [Serializable]
    public class AntiAliasingSettings : ScriptableObject, IInitializable
    {
        [SerializeField]
        bool m_Enabled;

        [SerializeField]
        Fxaa.Preset m_Preset;

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

        public void Init()
        {
            m_Preset = Fxaa.Preset.defaultPreset;
        }
    }
}
