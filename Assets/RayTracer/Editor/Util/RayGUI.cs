using UnityEngine;

namespace Assets.RayTracer.Editor.Util
{
    public static class RayGUI
    {
        public static Rect GetCenteredRect(Rect position, Vector2 size)
        {
            var width = Mathf.Min(position.width, size.x);
            var height = Mathf.Min(position.height, size.y);
            return new Rect(position.center.x - width * 0.5f, position.center.y - height * 0.5f, width, height);
        }
    }
}
