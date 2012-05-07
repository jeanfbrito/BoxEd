using System;
using UnityEngine;

namespace BoxEd
{
	public delegate bool SystemInitFunc();

	public static class Editor
	{
		public static Version Version { get { return new Version(0, 1); } }

		public static void InitSystem(string systemName, SystemInitFunc initFunc)
		{
			Editor.Log("Initialising {0}...", systemName);

			if(initFunc())
				Editor.Log("Initialised {0} successfully!", systemName);
			else
				Editor.LogError("Initialisation for {0} failed!", systemName);
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