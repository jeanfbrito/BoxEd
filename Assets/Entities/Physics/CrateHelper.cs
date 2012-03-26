using BoxEd;
using UnityEngine;

[Entity("Crate", EntityCategory.Objects)]
public class CrateHelper : ResizeableHelper
{
	protected GameObject _crate;

	[EntityProperty(Min = 1, Max = 100)]
	public float Mass { get; set; }

	public override void OnSpawn()
	{
		base.OnSpawn();
		_renderer.material.color = new Color(1, 1, 1, 0.8f);
	}

	public override void OnEnterGame()
	{
		if(_crate)
			Destroy(_crate);

		_crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_crate.transform.localScale = new Vector3(Width, Height, 2);
		_crate.transform.position = transform.position;
		_crate.transform.rotation = transform.rotation;
		var rigidbody = _crate.AddComponent<Rigidbody>();

		if(Mass < 1)
			Mass = 1;

		rigidbody.mass = Mass;
		rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
	}

	public override void OnLeaveGame()
	{
		if(_crate)
			Destroy(_crate);
	}
}