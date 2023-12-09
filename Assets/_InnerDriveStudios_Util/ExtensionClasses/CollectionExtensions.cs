using System.Collections.Generic;
using UnityEngine;

namespace InnerDriveStudios.Util
{
	/**
	 * Simple extension class to get a random list or array element.
	 *
	 * @author J.C. Wichman - InnerDriveStudios.com
	 */
	public static class CollectionExtensions
	{
		public static T GetRandomElement<T>(this List<T> pList)
		{
			return pList[Random.Range(0, pList.Count)];
		}

		public static T GetRandomElement<T>(this T[] pArray)
		{
			return pArray[Random.Range(0, pArray.Length)];
		}
	}
}
