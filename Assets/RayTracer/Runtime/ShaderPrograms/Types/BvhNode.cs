using System.Runtime.InteropServices;
using Assets.RayTracer.Runtime.Util;
using RayTracer.Runtime.Util;

namespace RayTracer.Runtime.ShaderPrograms.Types
{
    public struct BvhNode
    {
        public Aabb leftBounds;
        public Aabb rightBounds;
        public int left;
        public int right;
        public bool isLeftLeaf;
        public bool isRightLeaf;

        public static readonly int s_Size = Aabb.s_Size * 2 + ShaderSizes.s_Int * 2 + ShaderSizes.s_Bool * 2;

        //public static int[] Convert(BvhNode[] data)
        //{
        //    var convertedData = new int[data.Length];

        //}
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AlignedBvhNode
    {
        [FieldOffset(0)] public Aabb leftBounds;
        [FieldOffset(24)] public Aabb rightBounds;
        [FieldOffset(48)] public int left;
        [FieldOffset(52)] public int right;
        [FieldOffset(56)] private int m_IsLeftLeaf;
        [FieldOffset(60)] private int m_IsRightLeaf;

        public bool isLeftLeaf
        {
            get { return m_IsLeftLeaf != 0; }
            set { m_IsLeftLeaf = value ? -1 : 0; }
        }

        public bool isRightLeaf
        {
            get { return m_IsRightLeaf != 0; }
            set { m_IsRightLeaf = value ? -1 : 0; }
        }

        public AlignedAabb bounds
        {
            get { return leftBounds.Merge(rightBounds);}
        }

        public override string ToString()
        {
            return new DebugStringBuilder
            {
                {"leftBounds", leftBounds.ToString(), "({1})" },
                {"rightBounds", rightBounds.ToString(), "({1})" },
                {"left", left },
                {"right", right },
                {"isLeftLeaf", isLeftLeaf },
                {"isRightLeaf", isRightLeaf }
            }.ToString();
        }

        public static readonly int s_Size = 64;

        public static implicit operator AlignedBvhNode(BvhNode node)
        {
            return new AlignedBvhNode {leftBounds = node.leftBounds, rightBounds = node.rightBounds, m_IsLeftLeaf = node.isLeftLeaf ? -1 : 0, m_IsRightLeaf = node.isRightLeaf ? -1 : 0, left = node.left, right = node.right};
        }
    }
}
