using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyComputeScript : MonoBehaviour
{
    public ComputeShader shader;
    public RenderTexture texture;

    private int m_Kernel;

    public void Start()
    {
        m_Kernel = shader.FindKernel(StringConstants.MyFirstCompute.csMain);
        RunShader();
    }

    public void RunShader()
    {
        // texture = new RenderTexture(512, 512, 24) {enableRandomWrite = true};
        texture.enableRandomWrite = true;
        texture.Create();

        shader.SetTexture(m_Kernel, StringConstants.MyFirstCompute.result, texture);
        shader.Dispatch(m_Kernel, 512 / 8, 512 / 8, 1);
    }
}