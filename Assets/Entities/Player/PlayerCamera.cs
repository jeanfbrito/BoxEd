using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Camera")]
public class PlayerCamera : MonoBehaviour
{
	private Transform target;
	public float cameraHeight;
	public float cameraDistance;
	private Vector3 wantedPosition;

	public bool CanMove { get; set; }

	void Start()
	{
		if(!networkView.isMine)
			Destroy(this.gameObject);

		foreach(var ply in GameObject.FindGameObjectsWithTag("Player"))
		{
			if(ply.networkView.isMine)
			{
				Debug.Log("Got player");
				CanMove = true;
				target = ply.transform;
				transform.position = target.transform.position;
				return;
			}
		}			
	}

	void FixedUpdate()
	{
		wantedPosition = target.transform.position + new Vector3(0, cameraHeight, -cameraDistance);

		if(CanMove)
			transform.position = Vector3.Lerp(transform.position, wantedPosition, 0.1F);
	}
}