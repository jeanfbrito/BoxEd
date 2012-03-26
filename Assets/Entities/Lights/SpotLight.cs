using UnityEngine;
using BoxEd;

//[Entity("Spot Light", EntityCategory.Lights)]
public class SpotLight : PointLight
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_light.type = LightType.Spot;
	}
}