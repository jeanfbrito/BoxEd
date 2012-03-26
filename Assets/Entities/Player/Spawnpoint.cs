﻿using BoxEd;
using UnityEngine;

[Entity("Spawn Point", EntityCategory.Player)]
public class Spawnpoint : EntityHelper
{
	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(0, 0, 1, 0.5f);
	}
}