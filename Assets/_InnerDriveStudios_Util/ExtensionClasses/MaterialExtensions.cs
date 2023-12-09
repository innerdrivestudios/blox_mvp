using UnityEngine;

namespace InnerDriveStudios.Util
{
	/**
	 * Simple extension class to set a couple of often used Material properties.
	 *
	 * @author J.C. Wichman - InnerDriveStudios.com
	 */
	public static class MaterialExtensions
	{
		public static void SetColor(this Material pMaterial, Color pColor)
		{
			pMaterial.SetVector("_Color", pColor);
		}

		public static void SetEmissiveColor(this Material pMaterial, Color pColor)
		{
			pMaterial.SetVector("_EmissionColor", pColor);
		}

	}
}
