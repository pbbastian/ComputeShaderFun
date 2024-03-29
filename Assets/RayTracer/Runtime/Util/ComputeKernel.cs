﻿using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.Util
{
    public class ComputeKernel
    {
        Int3 m_ThreadGroupSize;
        string m_KernelName;

        public ComputeKernel(string fileName, string kernelName)
        {
            m_KernelName = kernelName;
            shader = Resources.Load<ComputeShader>(fileName);
            index = shader.FindKernel(kernelName);

            uint x, y, z;
            shader.GetKernelThreadGroupSizes(index, out x, out y, out z);
            m_ThreadGroupSize.x = (int) x;
            m_ThreadGroupSize.y = (int) y;
            m_ThreadGroupSize.z = (int) z;
        }

        public ComputeKernel(string kernelName) : this(string.Format("Shaders/{0}", kernelName), kernelName)
        {
        }

        public int index { get; private set; }

        public ComputeShader shader { get; private set; }

        public Int3 threadGroupSize
        {
            get { return m_ThreadGroupSize; }
        }

        public void Dispatch(int threadGroupsX, int threadGroupsY, int threadGroupsZ)
        {
            shader.Dispatch(index, threadGroupsX, threadGroupsY, threadGroupsZ);
        }

        public void Dispatch(ComputeBuffer argsBuffer, uint argsOffset = 0)
        {
            shader.DispatchIndirect(index, argsBuffer, argsOffset);
        }

        public bool isValid
        {
            get { return shader.HasKernel(m_KernelName); }
        }


        #region String setters

        public void SetValue(string name, bool val)
        {
            shader.SetBool(name, val);
        }

        public void SetValue(string name, float val)
        {
            shader.SetFloat(name, val);
        }

        public void SetValue(string name, params float[] values)
        {
            shader.SetFloats(name, values);
        }

        public void SetValue(string name, int val)
        {
            shader.SetInt(name, val);
        }

        public void SetValue(string name, params int[] values)
        {
            shader.SetInts(name, values);
        }

        public void SetValue(string name, Vector2 value)
        {
            shader.SetVector(name, value);
        }

        public void SetValue(string name, Vector3 value)
        {
            shader.SetVector(name, value);
        }

        public void SetValue(string name, Vector4 value)
        {
            shader.SetVector(name, value);
        }

        public void SetValue(string name, Matrix4x4 value)
        {
            shader.SetMatrix(name, value);
        }

        public void SetBuffer(string name, ComputeBuffer buffer)
        {
            shader.SetBuffer(index, name, buffer);
        }

        public void SetTexture(string name, Texture texture)
        {
            shader.SetTexture(index, name, texture);
        }

        public void SetTexture(string name, string globalTextureName)
        {
            shader.SetTextureFromGlobal(index, name, globalTextureName);
        }

        #endregion


        #region ID setters

        public void SetValue(int nameId, bool val)
        {
            shader.SetBool(nameId, val);
        }

        public void SetValue(int nameId, float val)
        {
            shader.SetFloat(nameId, val);
        }

        public void SetValue(int nameId, params float[] values)
        {
            shader.SetFloats(nameId, values);
        }

        public void SetValue(int nameId, int val)
        {
            shader.SetInt(nameId, val);
        }

        public void SetValue(int nameId, params int[] values)
        {
            shader.SetInts(nameId, values);
        }

        public void SetValue(int nameId, Vector2 value)
        {
            shader.SetVector(nameId, value);
        }

        public void SetValue(int nameId, Vector3 value)
        {
            shader.SetVector(nameId, value);
        }

        public void SetValue(int nameId, Vector4 value)
        {
            shader.SetVector(nameId, value);
        }

        public void SetValue(int nameId, Matrix4x4 value)
        {
            shader.SetMatrix(nameId, value);
        }

        public void SetBuffer(int nameId, ComputeBuffer buffer)
        {
            shader.SetBuffer(index, nameId, buffer);
        }

        public void SetTexture(int nameId, Texture texture)
        {
            shader.SetTexture(index, nameId, texture);
        }

        public void SetTexture(int nameId, int globalTextureNameId)
        {
            shader.SetTextureFromGlobal(index, nameId, globalTextureNameId);
        }

        #endregion


        #region ShaderParamDescriptor setters

        public void SetValue(ShaderParamDescriptor<bool> descriptor, bool val)
        {
            shader.SetBool(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<int> descriptor, int val)
        {
            shader.SetInt(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<float> descriptor, float val)
        {
            shader.SetFloat(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<Vector2> descriptor, Vector2 val)
        {
            shader.SetVector(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<Vector3> descriptor, Vector3 val)
        {
            shader.SetVector(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<Vector4> descriptor, Vector4 val)
        {
            shader.SetVector(descriptor.nameId, val);
        }

        public void SetValue(ShaderParamDescriptor<Matrix4x4> descriptor, Matrix4x4 value)
        {
            shader.SetMatrix(descriptor.nameId, value);
        }

        public void SetBuffer<T>(ShaderParamDescriptor<StructuredBuffer<T>> descriptor, StructuredBuffer<T> buffer)
            where T : struct
        {
            shader.SetBuffer(index, descriptor.nameId, buffer);
        }

        public void SetTexture(ShaderParamDescriptor<Texture> descriptor, Texture texture)
        {
            shader.SetTexture(index, descriptor.nameId, texture);
        }

        public void SetTexture(ShaderParamDescriptor<Texture> descriptor, string globalTextureName)
        {
            shader.SetTextureFromGlobal(index, descriptor.name, globalTextureName);
        }

        public void SetTexture(ShaderParamDescriptor<Texture> descriptor, int globalTextureNameId)
        {
            shader.SetTextureFromGlobal(index, descriptor.nameId, globalTextureNameId);
        }

        #endregion

    }
}
