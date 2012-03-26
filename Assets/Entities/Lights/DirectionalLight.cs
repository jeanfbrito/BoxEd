using UnityEngine;
using BoxEd;

public class DirectionalLight : BaseLight
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_light.type = LightType.Directional;
	}
}