using BoxEd;
using UnityEngine;
using System.Collections;

public class EntityHelper : Entity
{
	protected MeshRenderer _renderer;
	protected MeshFilter _filter;
	protected BoxCollider _collider;

	public override void OnSpawn()
	{
		_renderer = gameObject.AddComponent<MeshRenderer>();
		_renderer.material = References.Instance.helperMaterial;
		_filter = gameObject.AddComponent<MeshFilter>();
		_filter.mesh = References.Instance.cubeMesh;
		_collider = gameObject.AddComponent<BoxCollider>();
		_collider.isTrigger = true;
		gameObject.layer = LayerMask.NameToLayer("Unlit");
	}

	public override void OnEnableHelpers()
	{
		_renderer.enabled = true;
	}

	public override void OnDisableHelpers()
	{
		_renderer.enabled = false;
	}
}
