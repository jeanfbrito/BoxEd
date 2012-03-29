using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BoxEd
{
	public static class ActionManager
	{
		private static Stack<EditorAction> _stack;
		public static Stack<EditorAction> ActionStack
		{
			get
			{
				if(_stack == null)
					_stack = new Stack<EditorAction>();

				return _stack;
			}
		}

		public static void Undo()
		{
			if(ActionStack.Count < 1)
				return;

			var action = ActionStack.Pop();
			action.Revert();
		}
	}

	public abstract class EditorAction
	{
		public Entity Entity { get; set; }

		public virtual void Revert() { }
	}

	public class MoveAction : EditorAction
	{
		public Vector3 Movement { get; set; }

		public override void Revert()
		{
			if(Entity != null)
				Entity.transform.Translate(-Movement, Space.World);
		}
	}

	public class PropertyAction : EditorAction
	{
		public object Value { get; set; }
		public PropertyInfo Property { get; set; }

		public override void Revert()
		{
			Property.SetValue(Entity, Value, null);
		}
	}
}
