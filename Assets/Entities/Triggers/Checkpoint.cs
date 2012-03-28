using BoxEd;
using UnityEngine;

[Entity("Checkpoint", EntityCategory.Triggers)]
public class Checkpoint : Trigger
{
	public static Checkpoint CurrentCheckpoint { get; set; }

	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(0, 1, 0, Transparency);
	}

	public override void OnTrigger(Collider collider)
	{
		CurrentCheckpoint = this;
	}
}