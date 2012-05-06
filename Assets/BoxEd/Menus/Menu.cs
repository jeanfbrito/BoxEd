using System;
using UnityEngine;

namespace BoxEd.Gui
{
	/// <summary>
	/// Stores metadata for a Menu.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class MenuAttribute : Attribute
	{
		public MenuAttribute(string title)
		{
			Title = title;
		}

		/// <summary>
		/// The name of this menu.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The priority for this menu in the menubar.
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// Controls whether the menu will be included in builds.
		/// </summary>
		public bool EditorOnly { get; set; }
	}

	/// <summary>
	/// The base class for all editor menus.
	/// </summary>
	public class Menu
	{
		/// <summary>
		/// Used by Unity to paint the menu.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Displayed in the menu bar and at the top of the menu.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Defines the position in the menu bar at which this menu appears.
		/// </summary>
		public int Priority { get; set; }

		private Vector2 _scrollPos;

		/// <summary>
		/// Draws the menu within a scrollbar wrapper and invokes the OnDraw callback.
		/// </summary>
		/// <param name="id">The menu id.</param>
		public void Draw(int id)
		{
			_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);
			OnDraw();
			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Usually called twice per frame (once for layouting, then again for painting).
		/// </summary>
		public virtual void OnDraw() { }

		/// <summary>
		/// Called when the window is loaded by the WindowManager.
		/// </summary>
		public virtual void OnLoad() { }
	}
}
