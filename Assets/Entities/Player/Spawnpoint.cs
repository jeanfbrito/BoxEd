using BoxEd;
using UnityEngine;

[Entity("Spawn Point", EntityCategory.Player, Transforms.All)]
public class Spawnpoint : EntityHelper
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(0, 0, 1, Transparency);
	}
}