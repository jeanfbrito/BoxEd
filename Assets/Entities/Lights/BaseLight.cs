using BoxEd;
using UnityEngine;
using System.Collections;

public class BaseLight : EntityHelper
{
	protected Light _light;

	[EntityProperty(PropertyEditorType.Slider, Max = 8)]
	public float Intensity { get { return _light.intensity; } set { _light.intensity = value; } }

	[EntityProperty(PropertyEditorType.Slider, Max = 255)]
	public int Red { get { return Mathf.RoundToInt(light.color.r * 255); } set { var colour = _light.color; colour.r = (float)value / 255; _light.color = colour; } }

	[EntityProperty(PropertyEditorType.Slider, Max = 255)]
	public int Blue { get { return Mathf.RoundToInt(light.color.b * 255); } set { var colour = _light.color; colour.b = (float)value / 255; _light.color = colour; } }

	[EntityProperty(PropertyEditorType.Slider, Max = 255)]
	public int Green { get { return Mathf.RoundToInt(light.color.g * 255); } set { var colour = _light.color; colour.g = (float)value / 255; _light.color = colour; } }

	public override void OnSpawn()
	{
		base.OnSpawn();
		_light = gameObject.AddComponent<Light>();

		//Lights needn't take up a whole unit, but we scale the collider to make them just as easy to grab
		transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		_collider.extents = new Vector3(2, 2, 2);

		_renderer.material.color = new Color(0.8f, 0.8f, 0.5f, Transparency);

		//Don't cast light on unlit objects
		var unlitLayer = LayerMask.NameToLayer("Unlit");
		_light.cullingMask &= ~(1 << unlitLayer);
	}
}