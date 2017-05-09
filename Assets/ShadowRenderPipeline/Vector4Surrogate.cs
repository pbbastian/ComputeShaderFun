using System.Runtime.Serialization;
using UnityEngine;

namespace ShadowRenderPipeline
{
    public class Vector4Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (!(obj is Vector4))
                return;
            var vec = (Vector4)obj;
            info.AddValue("x", vec.x);
            info.AddValue("y", vec.y);
            info.AddValue("z", vec.z);
            info.AddValue("w", vec.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Vector4(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z"),
                info.GetSingle("w"));
        }
    }
}
