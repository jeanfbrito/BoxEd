using BoxEd;
using UnityEngine;

[Entity("Kill Trigger", EntityCategory.Player)]
public class KillTrigger : Trigger
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(1, 0, 0, Transparency);
	}

	public override void OnTrigger(Collider collider)
	{
		PlayerControl player;
		if(collider.TryGetComponent(out player))
			player.Kill();
	}
}