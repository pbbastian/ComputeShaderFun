using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace ShadowRenderPipeline
{
    public class AntiAliasingSettings : ScriptableObject
    {
        [SerializeField]
        bool m_Enabled;

//        [SerializeField]
//        AntialiasingModel.FxaaPreset m_Preset = AntialiasingModel.FxaaPreset.Default;

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

//        public AntialiasingModel.FxaaPreset preset
//        {
//            get { return m_Preset; }
//            set { m_Preset = value; }
//        }
    }
}
