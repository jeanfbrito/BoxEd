﻿using System;

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
		public Transforms EnabledTransforms { get; set; }

		public EntityAttribute(string name, EntityCategory category = EntityCategory.Misc, Transforms transforms = Transforms.None)
		{
			Name = name;
			Category = category;
			EnabledTransforms = transforms;
		}
	}

	[Flags]
	public enum Transforms
	{
		None = 0,
		Width = 1 << 0,
		Height = 1 << 2,
		Depth = 1 << 3,
		RotationX = 1 << 4,
		RotationY = 1 << 5,
		RotationZ = 1 << 6,
		Scale = Width | Height | Depth,
		Rotation = RotationX | RotationY | RotationZ,
		All = Scale | Rotation
	}
}