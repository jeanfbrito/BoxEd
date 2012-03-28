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
		public RestrictedDefaults Restrictions { get; set; }

		public EntityAttribute(string name, EntityCategory category = EntityCategory.Misc, RestrictedDefaults restrictions = RestrictedDefaults.None)
		{
			Name = name;
			Category = category;
			Restrictions = restrictions;
		}
	}

	[Flags]
	public enum RestrictedDefaults
	{
		None = 0,
		Width = 1,
		Height = 2,
		Depth = 4,
		Rotation = 8,
		Scale = Width | Height | Depth,
		All = Scale | Rotation
	}
}