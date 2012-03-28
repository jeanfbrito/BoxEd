using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace BoxEd.Gui
{
	[Menu("Settings", Priority = 4)]
	public class SettingsMenu : Menu
	{
		public override void OnLoad()
		{

		}

		public override void OnDraw()
		{
			if(GUILayout.Button("Save Settings"))
			{
				
			}
		}
	}
}