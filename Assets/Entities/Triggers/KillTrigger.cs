using BoxEd;
using UnityEngine;

[Entity("Kill Trigger", EntityCategory.Triggers)]
public class KillTrigger : Trigger
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(1, 0, 0, 0.5f);
	}

	public override void OnTrigger(Collider collider)
	{
		PlayerControl player;
		if(collider.TryGetComponent(out player))
			player.Kill();
	}
}