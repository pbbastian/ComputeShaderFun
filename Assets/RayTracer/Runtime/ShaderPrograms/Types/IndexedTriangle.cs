using System;
using System.Runtime.InteropServices;
using RayTracer.Runtime.Util;

namespace RayTracer.Runtime.ShaderPrograms.Types
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct IndexedTriangle
    {
        public uint v1;
        public uint v2;
        public uint v3;

        public uint this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return v1;
                    case 1:
                        return v2;
                    case 2:
                        return v3;
                    default:
                        throw new ArgumentException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        v1 = value;
                        break;
                    case 1:
                        v2 = value;
                        break;
                    case 2:
                        v3 = value;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public static readonly int s_Size = 3 * ShaderSizes.s_UInt;
    }
}
