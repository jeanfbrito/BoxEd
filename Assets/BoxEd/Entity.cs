using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BoxEd
{
	public class Entity : MonoBehaviour
	{
		public string UniqueName { get; set; }

		public string ClassName { get; private set; }

		private void Spawn()
		{
			EntityAttribute attr;
			if(GetType().TryGetAttribute(out attr))
				ClassName = attr.Name;
			else
				ClassName = GetType().Name;

			Properties = new Dictionary<PropertyInfo, EntityPropertyAttribute>();

			foreach(var prop in GetType().GetProperties())
			{
				EntityPropertyAttribute propAttr;
				if(prop.TryGetAttribute(out propAttr) && ValidateProperty(prop, attr.EnabledTransforms))
					Properties.Add(prop, propAttr);
			}

			OnSpawn();
			OnEnableHelpers();
		}

		private bool ValidateProperty(PropertyInfo property, Transforms enabledTransforms)
		{
			//Attempt to get the name of the property as a Transforms member
			//Return true if it's enabled for this entity
			try
			{
				var flags = (Transforms)Enum.Parse(typeof(Transforms), property.Name);
				return enabledTransforms.HasFlag(flags);
			}
			//If it's not a Transforms member, then we should accept it
			catch(ArgumentException)
			{
				return true;
			}
		}

		/// <summary>
		/// Called when the entity is spawned.
		/// </summary>
		public virtual void OnSpawn() { }

		/// <summary>
		/// Called when game mode is entered.
		/// </summary>
		public virtual void OnEnterGame() { }

		/// <summary>
		/// Called when the entity leaves test mode.
		/// </summary>
		public virtual void OnLeaveGame() { }

		/// <summary>
		/// Called when the user enables helpers in the editor.
		/// </summary>
		public virtual void OnEnableHelpers() { }

		/// <summary>
		/// Called when the user disables helpers in the editor.
		/// </summary>
		public virtual void OnDisableHelpers() { }

		public Dictionary<PropertyInfo, EntityPropertyAttribute> Properties { get; private set; }

		#region Statics

		/// <summary>
		/// Creates a new instance of an entity.
		/// </summary>
		/// <typeparam name="T">The child of <see cref="Entity"/> to spawn.</typeparam>
		/// <param name="position">The entity's initial position.</param>
		/// <param name="rotation">The entity's initial rotation.</param>
		/// <param name="scale">The entity's initial scale.</param>
		/// <param name="type">If set, overrides T. Used in serialisation for runtime resolution.</param>
		/// <returns>The created entity.</returns>
		/// <exception cref="ArgumentException">Thrown if type is specified but is not a child of Entity.</exception>
		public static T Create<T>(Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), Vector3? scale = null, Type type = null) where T : Entity
		{
			if(type != null && !typeof(Entity).IsAssignableFrom(type))
				throw new ArgumentException("The parameter passed to type does not inherit Entity.");

			var overrideType = type != null;
			var isCube = typeof(T) == typeof(Cube) || type == typeof(Cube);
			GameObject obj;

			obj = isCube ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject();

			var boxEntity = overrideType ? (Entity)obj.AddComponent(type) : obj.AddComponent<T>();

			obj.transform.position = position;
			obj.transform.rotation = Quaternion.Euler(rotation);
			obj.transform.localScale = scale ?? (isCube ? new Vector3(1, 1, 2) : new Vector3(1, 1, 1));

			obj.name = overrideType ? type.Name : typeof(T).Name;

			boxEntity.Spawn();

			if(EditorController.IsIngame)
				boxEntity.OnEnterGame();
			else
				boxEntity.OnLeaveGame();

			return boxEntity as T;
		}

		private static Dictionary<EntityCategory, Dictionary<Type, string>> _entitiesByCategory;
		public static Dictionary<EntityCategory, Dictionary<Type, string>> EntitiesByCategory
		{
			get
			{
				if(_entitiesByCategory == null)
				{
					var categories = Enum.GetNames(typeof(EntityCategory)).Select(name => (EntityCategory)Enum.Parse(typeof(EntityCategory), name)).Where(value => value != EntityCategory.None);
					var entityTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(Entity).IsAssignableFrom(type) && type.HasAttribute<EntityAttribute>(false));
					_entitiesByCategory = new Dictionary<EntityCategory, Dictionary<Type, string>>(categories.Count());

					foreach(var category in categories)
					{
						var categoryEntries = new Dictionary<Type, string>();

						foreach(var type in entityTypes.Where(type => type.GetAttribute<EntityAttribute>().Category == category))
							categoryEntries.Add(type, type.GetAttribute<EntityAttribute>().Name);

						if(categoryEntries.Count > 0)
							_entitiesByCategory.Add(category, categoryEntries);
					}

					//If no category is specified, stick it in misc
					foreach(var type in entityTypes.Where(type => type.GetAttribute<EntityAttribute>().Category == EntityCategory.None))
						_entitiesByCategory[EntityCategory.Misc].Add(type, type.GetAttribute<EntityAttribute>().Name);
				}

				return _entitiesByCategory;
			}
		}

		public static Entity SelectedEntity { get; set; }
		#endregion

		#region Default Properties
		[EntityProperty(Min = 0.5f, Max = 100)]
		public float Width
		{
			get { return transform.localScale.x; }
			set { transform.localScale = new Vector3(EditorController.SnapToGrid ? (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2 : value, transform.localScale.y, transform.localScale.z); }
		}

		[EntityProperty(Min = 0.5f, Max = 100)]
		public float Height
		{
			get { return transform.localScale.y; }
			set { transform.localScale = new Vector3(transform.localScale.x, EditorController.SnapToGrid ? (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2 : value, transform.localScale.z); }
		}

		[EntityProperty(Min = 0.5f, Max = 100)]
		public float Depth
		{
			get { return transform.localScale.z; }
			set { transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, EditorController.SnapToGrid ? (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2 : value); }
		}

		[EntityProperty(Min = 0, Max = 360)]
		public int RotationX
		{
			get
			{
				return (int)transform.rotation.eulerAngles.x;
			}
			set
			{
				var rot = transform.rotation.eulerAngles;
				transform.rotation = Quaternion.Euler(new Vector3((int)Math.Round(value / 5f) * 5, rot.y, rot.z));
			}
		}

		[EntityProperty(Min = 0, Max = 360)]
		public int RotationY
		{
			get
			{
				return (int)transform.rotation.eulerAngles.y;
			}
			set
			{
				var rot = transform.rotation.eulerAngles;
				transform.rotation = Quaternion.Euler(new Vector3(rot.x, (int)Math.Round(value / 5f) * 5, rot.z));
			}
		}

		[EntityProperty(Min = 0, Max = 360)]
		public int Rotation
		{
			get
			{
				return (int)transform.rotation.eulerAngles.z;
			}
			set
			{
				var rot = transform.rotation.eulerAngles;
				transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, (int)Math.Round(value / 5f) * 5));
			}
		}
		#endregion
	}

	public enum EntityCategory
	{
		None = 0,
		Geometry,
		Player,
		Lights,
		Triggers,
		Misc,
		Objects
	}
}