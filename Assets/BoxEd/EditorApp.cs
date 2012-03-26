using System;
using UnityEngine;

namespace BoxEd
{
	public static class Editor
	{
		public static Version Version { get { return new Version(0, 1); } }

		public static void InitSystem(string systemName, Action initFunc)
		{
			Editor.Log("Initialising {0}...", systemName);
			initFunc.Invoke();
			Editor.Log("Initialised {0} successfully!", systemName);
		}

		public static void Log(string format, params object[] args)
		{
			var message = String.Format(format, args);
			Debug.Log("[Editor] " + message);
		}

		public static void LogWarning(string format, params object[] args)
		{
			var message = String.Format(format, args);
			Debug.LogWarning("[Editor] " + message);
		}

		public static void LogError(string format, params object[] args)
		{
			var message = String.Format(format, args);
			Debug.LogError("[Editor] " + message);
		}
	}
}