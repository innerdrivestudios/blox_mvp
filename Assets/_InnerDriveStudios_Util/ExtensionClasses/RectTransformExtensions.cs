using UnityEngine;

namespace InnerDriveStudios.Util
{
    /**
	 * Simple extension class to make it easier to handle RectTransforms.
	 *
	 * @author unknown
	 */
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetAll(this RectTransform rt, float value)
        {
            rt.offsetMin = new Vector2(value, value);
            rt.offsetMax = new Vector2(-value, -value);
        }
    }
}
