using BoxEd;
using UnityEngine;
using System.Collections;

public class EntityHelper : Entity
{
	public const float Transparency = 0.6f;

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

	public IEnumerator Fade(bool fadeOut)
	{
		if(fadeOut)
		{
			while(renderer.material.color.a > 0)
			{
				var colour = renderer.material.color;
				colour.a -= Time.deltaTime / 50;
				renderer.material.color = colour;
				yield return null;
			}
		}
		else
		{
			while(renderer.material.color.a < Transparency)
			{
				var colour = renderer.material.color;
				colour.a += Time.deltaTime / 50;
				renderer.material.color = colour;
				yield return null;
			}
		}
	}
}
