namespace RayTracer.Runtime.Util
{
    public static class IntExtensions
    {
        public static int CeilDiv(this int x, int y)
        {
            return 1 + (x - 1) / y;
        }
    }
}
