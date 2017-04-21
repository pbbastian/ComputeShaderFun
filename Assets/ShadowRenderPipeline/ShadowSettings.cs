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

        [SerializeField]
        int m_ShadowmapResolution;

        [SerializeField]
        ShadowingMethod m_Method;

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

        public int shadowmapResolution
        {
            get { return m_ShadowmapResolution; }
            set { m_ShadowmapResolution = value; }
        }

        public ShadowingMethod method
        {
            get { return m_Method; }
            set { m_Method = value; }
        }

        public void Init()
        {

        }
    }

    public enum ShadowingMethod
    {
        RayTracing,
        ShadowMapping,
        Hybrid
    }

    public enum ShadowmapVariant
    {
        Paraboloid,
        Pinhole
    }
}
