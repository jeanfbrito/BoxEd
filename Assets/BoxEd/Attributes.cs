using System;

namespace BoxEd
{
	/// <summary>
	/// Indicates that a property is to be serialised by BoxEd.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public sealed class EntityPropertyAttribute : Attribute
	{
		public PropertyEditorType EditorType { get; set; }
		public float Min { get; set; }
		public float Max { get; set; }

		public EntityPropertyAttribute() { }

		public EntityPropertyAttribute(PropertyEditorType editorType)
		{
			EditorType = editorType;
			if(Min == 0 && Max == 0)
				Max = 100;
		}
	}

	public enum PropertyEditorType
	{
		None = 0,
		Textbox,
		Slider
	}

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class EntityAttribute : Attribute
	{
		public string Name { get; set; }
		public EntityCategory Category { get; set; }

		public EntityAttribute(string name) : this(name, EntityCategory.Misc) { }

		public EntityAttribute(string name, EntityCategory category)
		{
			Name = name;
			Category = category;
		}
	}
}