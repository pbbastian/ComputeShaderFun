using UnityEngine;

namespace RayTracer.Runtime
{
    public class RayTracingProfileAsset : ScriptableObject
    {
        [SerializeField]
        private RayTracingProfile m_Profile = new RayTracingProfile();

        public RayTracingProfile profile
        {
            get { return m_Profile; }
        }
    }
}
