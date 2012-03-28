using UnityEngine;
using BoxEd;

[Entity("Spot Light", EntityCategory.Lights)]
public class SpotLight : PointLight
{
	[EntityProperty(Min = 0, Max = 180)]
	public int FieldOfView { get { return (int)_light.spotAngle; } set { _light.spotAngle = value; } }

	public override void OnSpawn()
	{
		base.OnSpawn();
		_light.type = LightType.Spot;

		var rot = transform.rotation.eulerAngles;
		transform.rotation = Quaternion.Euler(90, rot.y, rot.z);
	}
}