using UnityEngine;

namespace RayTracer.Editor.Util
{
    public static class RayGUILayout
    {
        public static Rect GetRemainingRect()
        {
            return GUILayoutUtility.GetRect(1, 1, 1, 1, GUILayout.ExpandHeight(true));
        }
    }
}
