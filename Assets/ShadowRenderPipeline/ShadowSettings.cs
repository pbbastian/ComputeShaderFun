using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.ShadowRenderPipeline
{
    public class ShadowSettings : ScriptableObject, IInitializable
    {
        [SerializeField]
        bool m_Enabled;

        [SerializeField]
        ShadowmapVariant m_ShadowmapVariant;

        public bool enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public ShadowmapVariant shadowmapVariant
        {
            get { return m_ShadowmapVariant; }
            set { m_ShadowmapVariant = value; }
        }

        public void Init()
        {

        }
    }

    public enum ShadowmapVariant
    {
        Paraboloid,
        Pinhole
    }
}
