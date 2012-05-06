using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoxEd;
using BoxEd.Gui;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	#region Boring Stuff
	public void Awake()
	{
		Refresh();
	}

	public static bool IsMenuOpen { get { return _activeMenu != null; } }
	
	/// <summary>
	/// Refreshes the menu manager and reloads all menus.
	/// </summary>
	public static void Refresh()
	{
		Editor.InitSystem("Menu Manager", () =>
		{
			_menus = new List<Menu>();
			CloseMenu();

			//Get all Menus, except the base class itself
			foreach(var menuType in Assembly.GetExecutingAssembly().GetTypes().Where(type => type != typeof(Menu) && typeof(Menu).IsAssignableFrom(type)))
			{
				var menu = Activator.CreateInstance(menuType) as Menu;

				//Extract the MenuAttribute, otherwise throw an exception
				MenuAttribute attr;
				if(menuType.TryGetAttribute<MenuAttribute>(out attr))
				{
					if(attr.EditorOnly && !Application.isEditor)
						continue;

					menu.Id = _menus.Count + 1;

					menu.Title = Localisation.BoxEd[attr.Title];
					menu.Priority = attr.Priority;
					_menus.Add(menu);

					Editor.Log("Registered new Window, title is {0} and ID is {1}.", menu.Title, menu.Id);

					//Fire the load callback
					menu.OnLoad();
				}
				else
					Editor.LogError("Failed to initialise window of type {0}, verify that a MenuAttribute is assigned to your class.", menuType.Name);
			}
		});

		_menus.Sort((m1, m2) => m1.Priority.CompareTo(m2.Priority));
	}
	

	public static Menu OpenMenu<T>() where T : Menu
	{
		_activeMenu = _menus.First(menu => menu is T) ?? _activeMenu;
		return _activeMenu;
	}

	public static void CloseMenu()
	{
		_activeMenu = null;
	}

	private static List<Menu> _menus;
	private static Menu _activeMenu;
	#endregion

	private void OnGUI()
	{
		if(_menus == null)
			return;

		GUI.skin = References.Instance.guiSkin;

		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		{
			GUILayout.Space(5);

			if(GUILayout.Button(EditorController.IsIngame ? "Exit Game" : "Enter Game"))
				EditorController.IsIngame = !EditorController.IsIngame;

			if(!EditorController.IsIngame)
			{
				foreach(var menu in _menus)
				{
					if(GUILayout.Button(menu.Title))
						_activeMenu = (_activeMenu != menu) ? menu : null;
				}
			}
			else
				_activeMenu = null;
		}
		GUILayout.EndHorizontal();

		if(_activeMenu != null)
			GUI.Window(_activeMenu.Id, new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2), _activeMenu.Draw, _activeMenu.Title);
	}
}