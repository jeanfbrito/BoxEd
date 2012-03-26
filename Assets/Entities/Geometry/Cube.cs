using UnityEngine;
using BoxEd;
using System;

[Entity("Block", EntityCategory.Geometry)]
public class Cube : Entity
{
	[EntityProperty(Min = 0.5f, Max = 100)]
	public float Width {
		get { return transform.localScale.x; }
		set { transform.localScale = new Vector3(EditorController.SnapToGrid ? (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2 : value, transform.localScale.y, transform.localScale.z); }
	}

	[EntityProperty(Min = 0.5f, Max = 100)]
	public float Height
	{
		get { return transform.localScale.y; }
		set { transform.localScale = new Vector3(transform.localScale.x, EditorController.SnapToGrid ? (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2 : value, transform.localScale.z); }
	}
}