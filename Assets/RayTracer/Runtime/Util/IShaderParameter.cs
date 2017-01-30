namespace RayTracer.Runtime.Util
{
    public interface IShaderParameter<T>
    {
        T value { get; set; }
    }
}
