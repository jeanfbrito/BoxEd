using BoxEd;
using UnityEngine;

[Entity("Point Light", EntityCategory.Lights)]
public class PointLight : BaseLight
{
	[EntityProperty(PropertyEditorType.Slider)]
	public float Radius { get { return _light.range; } set { _light.range = value; } }
}
