#if !UNITY_EDITOR
using System.Diagnostics;
using UnityEngine;

/**
 * Override standard Unity debug class.
 */
public static class Debug 
{
	[Conditional("UNITY_EDITOR")]
	public static void Log (object pInfo, Object pContext, [System.Runtime.CompilerServices.CallerMemberName] string pMemberName = "")
	{
		UnityEngine.Debug.Log(pMemberName.Substring(pMemberName.LastIndexOf("\\")+1) + ":" + pInfo, pContext);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Log(object pInfo, [System.Runtime.CompilerServices.CallerFilePath] string pMemberName = "")
	{
		UnityEngine.Debug.Log(pMemberName.Substring(pMemberName.LastIndexOf("\\")+1) + ":" + pInfo);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Assert(bool condition, string message) {
		UnityEngine.Debug.Assert(condition, message);
	}
}
#endif