#if UNITY_EDITOR
using UnityEngine;

namespace BoxEd.Gui
{
	[Menu("Developer Tools", Priority = 0)]
	public class Developer : Menu
	{
		public override void OnDraw()
		{
			if(GUILayout.Button("Save current level as webplayer demo"))
			{
				LevelManager.SaveLevel("", true);
			}
		}
	}
}
#endif