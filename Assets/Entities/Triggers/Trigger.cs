using System;
using BoxEd;
using UnityEngine;

public class Trigger : ResizeableHelper
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		transform.localScale = new Vector3(2, 2, 1);
	}

	private void OnTriggerEnter(Collider collider)
	{
		OnTrigger(collider);
	}

	public virtual void OnTrigger(Collider collider) { }
}