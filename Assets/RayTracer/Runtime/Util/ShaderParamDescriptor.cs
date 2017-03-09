using System;
using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class ShaderParamDescriptor<T>
    {
        public ShaderParamDescriptor(string name)
        {
            this.name = name;
            nameId = Shader.PropertyToID(name);
            valueType = typeof(T);
        }

        public string name { get; private set; }

        public int nameId { get; private set; }

        public Type valueType { get; private set; }
    }
}
