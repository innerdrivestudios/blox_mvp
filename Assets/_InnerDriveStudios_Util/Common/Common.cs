using UnityEngine;

namespace InnerDriveStudios.Util
{
	//todo move these methods into extension classes where possible
	public static class Common
	{
		///note that you can be in both play mode and one of these edit modes, since you can edit in play mode
		public enum EditMode { Normal, Prefab };

		/// <summary>
		/// Returns whether the application is in play mode
		/// </summary>
		/// <returns></returns>
		public static bool IsInPlayMode()
		{
			return Application.isPlaying;
		}

		/// <summary>
		/// Is the transform a regular transform and not a RectTransform?
		/// </summary>
		/// <param name="pTransform"></param>
		/// <returns></returns>
		public static bool IsRegularTransform(Transform pTransform)
		{
			return pTransform != null && !(pTransform is RectTransform);
		}

		public static bool IsRootTransform(Transform pTransform)
		{
			return pTransform.parent == null;
		}

		public static bool IsEmpty(Transform pTransform)
		{
			return pTransform.GetComponents<Component>().Length == 1;
		}

		public static bool HasMeshRenderer(Transform pTransform)
		{
			return pTransform.GetComponent<MeshRenderer>() != null;
		}

		/// <summary> Returns false when the gameobject or its children do not contain any meshrenderer components</summary>
		public static bool GetBounds(Transform pRoot, out Bounds bounds)
		{
			Renderer[] renderers = pRoot.GetComponentsInChildren<Renderer>();

			if (renderers.Length == 0)
			{
				bounds = new Bounds();
				return false;
			}

			bounds = renderers[0].bounds;
			for (int i = 1; i < renderers.Length; i++)
			{
				bounds.Encapsulate(renderers[i].bounds);
			}

			return true;
		}

	


	}
}
