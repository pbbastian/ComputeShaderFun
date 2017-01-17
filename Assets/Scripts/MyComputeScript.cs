using UnityEngine;

public class MyComputeScript : MonoBehaviour
{
    private int m_Kernel;
    public ComputeShader shader;
    public RenderTexture texture;

    public void Start()
    {
        RunShader();
    }

    public void RunShader()
    {
        m_Kernel = shader.FindKernel(StringConstants.MyFirstCompute.csMain);
        // texture = new RenderTexture(512, 512, 24) {enableRandomWrite = true};
        texture.enableRandomWrite = true;

        texture.Create();

        shader.SetTexture(m_Kernel, StringConstants.MyFirstCompute.result, texture);
        shader.Dispatch(m_Kernel, 512 / 8, 512 / 8, 1);
    }
}
