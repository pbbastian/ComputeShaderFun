using System.Runtime.Serialization;
using UnityEngine;

namespace ShadowRenderPipeline
{
    public class Vector3Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!(obj is Vector3))
                return;
            var vec = (Vector3)obj;
            info.AddValue("x", vec.x);
            info.AddValue("y", vec.y);
            info.AddValue("z", vec.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Vector3(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z"));
        }
    }
}
