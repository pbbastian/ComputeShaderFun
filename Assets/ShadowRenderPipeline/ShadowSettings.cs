using System;
using UnityEngine;

namespace Assets.ShadowRenderPipeline
{
    [Serializable]
    public class ShadowSettings
    {
        [SerializeField]
        bool m_ShadowsEnabled = true;

        [SerializeField]
        ShadowmapVariant m_ShadowmapVariant;

        [SerializeField]
        int m_ShadowmapResolution = 256;

        [SerializeField]
        ShadowingMethod m_Method;

        [SerializeField]
        float m_Bias;

        [SerializeField]
        bool m_PixelCulling;

        [SerializeField]
        bool m_SegmentCulling;

        public bool enabled
        {
            get { return m_ShadowsEnabled; }
            set { m_ShadowsEnabled = value; }
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

        public float bias
        {
            get { return m_Bias; }
            set { m_Bias = value; }
        }

        public bool pixelCulling
        {
            get { return m_PixelCulling; }
            set { m_PixelCulling = value; }
        }

        public bool segmentCulling
        {
            get { return m_SegmentCulling; }
            set { m_SegmentCulling = value; }
        }
    }

    public enum ShadowingMethod
    {
        RayTracing,
        ShadowMapping
    }

    public enum ShadowmapVariant
    {
        Paraboloid,
        Pinhole
    }
}
