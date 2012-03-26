using UnityEngine;

/// <summary>
/// This class controls player animation.
/// </summary>
[AddComponentMenu("Player/Animation")]
public class PlayerAnimation : MonoBehaviour
{
	/// <summary>
	/// The transition time for the majority of animations.
	/// </summary>
	public float generalTransitionTime;
	/// <summary>
	/// Specific transition time for jumping.
	/// </summary>
	public float jumpTransitionTime;

	private PlayerControl playerControl;

	void Awake()
	{
		SetUpAnimations();
		playerControl = GetComponent<PlayerControl>();
	}

	void SetUpAnimations()
	{
		animation["run"].wrapMode = WrapMode.Loop;
	}

	/// <summary>
	/// Called whilst running.
	/// </summary>
	public void Running()
	{
		animation.CrossFade("run", generalTransitionTime);
		SetRotation();
	}

	/// <summary>
	/// Called whilst idle.
	/// </summary>
	public void Idle()
	{
		animation.CrossFade("idle", generalTransitionTime);
	}

	/// <summary>
	/// Called on jump.
	/// </summary>
	public void Jump()
	{
		animation.CrossFade("jump", jumpTransitionTime);
	}

	/// <summary>
	/// Called whilst in the air.
	/// </summary>
	public void InAir()
	{
		animation.CrossFade("jumpFall", generalTransitionTime);
		SetRotation();
	}

	/// <summary>
	/// Called whilst attached to a wall.
	/// </summary>
	public void WallGrab()
	{
		animation.CrossFade("GrabWall", 0.1F);
	}

	//Internally determine whether to rotate based on player velocity
	void SetRotation()
	{
		switch(playerControl.GetPlayerState())
		{
			case PlayerState.InAir:
			case PlayerState.InAirFromFall:
			case PlayerState.InAirFromWall:
				if(rigidbody.velocity.x < 0)
				{
					transform.eulerAngles = new Vector3(0, -90, 0);
				}
				if(rigidbody.velocity.x > 0)
				{
					transform.eulerAngles = new Vector3(0, 90, 0);
				}
				break;

			case PlayerState.Grounded:
				if(playerControl.GetPlayerDirection() == PlayerDirection.Left)
				{
					transform.eulerAngles = new Vector3(0, -90, 0);
				}
				else if(playerControl.GetPlayerDirection() == PlayerDirection.Right)
				{
					transform.eulerAngles = new Vector3(0, 90, 0);
				}
				break;
		}

	}
}