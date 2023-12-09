using UnityEngine;

namespace InnerDriveStudios.Util
{
    /**
	 * Simple extension class to add some useful methods to the Transform class such as snapping and returning all children as an array.
	 *
	 * @author J.C. Wichman - InnerDriveStudios.com
	 */
    public static class TransformExtensions
    {
        /**
         * Snaps the x,y,z position of the current Transform to a grid of the given size.
         */
        public static void SnapToGrid(this Transform transform, float pGridSize)
        {
            // Calculate the new position by rounding the transform's position to the nearest multiple of the grid size
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.Round(newPosition.x / pGridSize) * pGridSize;
            newPosition.y = Mathf.Round(newPosition.y / pGridSize) * pGridSize;
            newPosition.z = Mathf.Round(newPosition.z / pGridSize) * pGridSize;

            // Update the transform's position to the snapped position
            transform.position = newPosition;
        }

        /**
         * @return a new array filled with all the Transform children of the current Transform.
         */
        public static Transform[] GetChildren(this Transform transform)
		{
            int childCount = transform.childCount;
            Transform[] children = new Transform[childCount];
            for (int i = 0; i < childCount; i++) children[i] = transform.GetChild(i);
            return children;
		}
    }
}
