using System.Text;

namespace RayTracer.Runtime.Util
{
    public static class StringDebug
    {
        public static string FormatDebug(this object arg, string name, string argFormat = "{1}")
        {
            return string.Format("{0}=" + argFormat, name, arg);
        }

        public static string Join(params string[] strings)
        {
            return string.Join(", ", strings);
        }
    }
}
