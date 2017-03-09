using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public abstract class ProgramBase
    {
        protected int m_KernelIndex;
        protected ComputeShader m_Shader;
        protected int m_SizeX;
        protected int m_SizeY;
        protected int m_SizeZ;

        protected ProgramBase(string kernelName)
        {
            m_Shader = Resources.Load<ComputeShader>(string.Format("Shaders/{0}", kernelName));
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
            m_SizeY = (int) y;
            m_SizeZ = (int) z;
        }
    }
}
