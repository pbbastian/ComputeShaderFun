using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    [Serializable]
    public class RayTracingProfileAsset : ScriptableObject
    {
        [SerializeField]
        RayTracingProfile m_Profile = new RayTracingProfile();

        public RayTracingProfile profile
        {
            get { return m_Profile; }
        }
    }
}
