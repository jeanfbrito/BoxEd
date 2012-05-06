using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BoxEd.Gui
{
	[Menu("File", Priority = 1)]
	public class FileMenu : Menu
	{
		/// <summary>
		/// This stores the list of loadable levels.
		/// The key is the level name, and the value is the filepath
		/// </summary>
		private Dictionary<string, string> _levels;

		public override void OnLoad()
		{
#if !UNITY_WEBPLAYER || UNITY_EDITOR
			Refresh();
#endif
		}

		public override void OnDraw()
		{
			// If we're in the webplayer, then we should only offer the sample level because Unity has no IO access
			// We also allow in-editor 'testing' of this feature
			if(Developer.IsFakingWebplayer || Application.isWebPlayer)
			{
				GUILayout.Label("Saving and loading is not enabled in the web demo!");
				if(GUILayout.Button("Try this sample level!"))
				{
					LevelManager.LoadLevel((Resources.Load("Level 1.box") as TextAsset).ToString(), true);
					MenuManager.CloseMenu();
				}
				return;
			}

			GUILayout.BeginHorizontal();
			{
				LevelManager.LevelName = GUILayout.TextField(LevelManager.LevelName);

				if(GUILayout.Button(Localisation.BoxEd["Save"], GUILayout.MaxWidth(100)))
				{
					LevelManager.SaveLevel(LevelManager.LevelName);
					Refresh();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(20);
			GUILayout.Label(Localisation.BoxEd["Level Manager"]);
			GUILayout.Space(10);

			foreach(var level in _levels)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label(level.Key);
					if(GUILayout.Button(Localisation.BoxEd["Load"], GUILayout.MaxWidth(100)))
					{
						LevelManager.LoadLevel(level.Key);
						MenuManager.CloseMenu();
					}

					if(GUILayout.Button(Localisation.BoxEd["Delete"], GUILayout.MaxWidth(100)))
					{
						File.Delete(level.Value);
						Refresh();
					}
				}
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Rescans the filesystem to reload the level list.
		/// </summary>
		private void Refresh()
		{
			_levels = new Dictionary<string, string>();

			foreach(var level in Directory.GetFiles(LevelManager.LevelFolder).Where(path => path.EndsWith(".box")))
			{
				Editor.Log("Found level file at {0}", level);
				_levels.Add(level.Substring(level.LastIndexOf("\\") + 1).Replace(".box", ""), level);
			}
		}
	}
}