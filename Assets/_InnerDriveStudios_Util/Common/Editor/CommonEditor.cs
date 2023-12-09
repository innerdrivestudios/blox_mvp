using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace InnerDriveStudios.Util
{
	public static class CommonEditor
	{
		///note that you can be in both play mode and one of these edit modes, since you can edit in play mode
		public enum EditMode { Normal, Prefab };

		/// <summary>
		/// Returns the amount of selected items in the hierarchy, project window, etc.
		/// </summary>
		/// <returns></returns>
		public static int GetSelectedObjectsCount()
		{
			return Selection.objects.Length;
		}

		/// <summary>
		/// Are we in normal edit mode (not editing a prefab), or any sort of prefab edit mode.
		/// </summary>
		/// <returns></returns>
		public static EditMode GetEditMode()
		{
			return PrefabStageUtility.GetCurrentPrefabStage() == null ? EditMode.Normal : EditMode.Prefab;
		}

		/// <summary>
		/// Returns true is the GameObject is not null and in a scene, but not (part of) a prefab instance.
		/// Note that this also returns true for any non prefab parts in prefab edit mode.
		/// If you want to exclude prefab edit mode, include the GetEditMode into your test.
		/// </summary>
		/// <param name="pGameObject"></param>
		/// <returns></returns>
		public static bool IsNonPrefabGameObjectInstance(GameObject pGameObject)
		{
			return pGameObject != null && pGameObject.scene.IsValid() && !PrefabUtility.IsPartOfAnyPrefab(pGameObject);
		}

	}
}
