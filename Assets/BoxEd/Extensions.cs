using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BoxEd
{
	/// <summary>
	/// Contains extensions used during runtime reflection.
	/// </summary>
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Gets the first instance of attribute T for a given member.
		/// </summary>
		/// <typeparam name="T">The attribute to search for.</typeparam>
		/// <param name="member">The member to search in.</param>
		/// <param name="inherit">If true, inherited attributes are included in the search.</param>
		/// <returns>The first instance of attribute T, or null if none exists.</returns>
		public static T GetAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
		{
			var attrs = member.GetCustomAttributes(typeof(T), inherit) as IEnumerable<T>;
			return (attrs != null && attrs.Count() > 0) ? attrs.First() : null;
		}

		public static bool TryGetAttribute<T>(this MemberInfo member, out T attribute, bool inherit = true) where T : Attribute
		{
			attribute = member.GetAttribute<T>();
			return attribute != null;
		}

		public static bool HasAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
		{
			return member.GetAttribute<T>(inherit) == null ? false : true;
		}
	}

	public static class GameObjectExtensions
	{
		public static bool HasComponent<T>(this GameObject obj) where T : Component
		{
			return obj.GetComponent<T>() != null;
		}

		public static bool HasComponent<T>(this Component obj) where T : Component
		{
			return obj.GetComponent<T>() != null;
		}

		public static bool TryGetComponent<T>(this GameObject obj, out T component) where T : Component
		{
			component = obj.GetComponent<T>();
			return component != null;
		}

		public static bool TryGetComponent<T>(this Component obj, out T component) where T : Component
		{
			component = obj.GetComponent<T>();
			return component != null;
		}
	}
}