using UnityEngine;

namespace BoxEd.Gui
{
	[Menu("Developer Tools", Priority = 0, EditorOnly = true)]
	public class Developer : Menu
	{
		public static bool IsFakingWebplayer { get; set; }

		public override void OnDraw()
		{
			if(GUILayout.Button("Save current level as webplayer demo"))
			{
				LevelManager.SaveLevel("", true);
			}

			IsFakingWebplayer = GUILayout.Toggle(IsFakingWebplayer, Localisation.BoxEd["Fake webplayer mode"]);
		}
	}
}