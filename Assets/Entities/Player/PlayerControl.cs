using UnityEngine;
using System.Linq;
using BoxEd;

/// <summary>
/// This class is used for all player controls.
/// </summary>
[AddComponentMenu("Player/Control")]
public class PlayerControl : MonoBehaviour
{
	/// <summary>
	/// Movement speed for the character
	/// </summary>
	public float movementSpeed;
	/// <summary>
	/// Speed cap for the character
	/// </summary>
	public float maxSpeed;
	/// <summary>
	/// Air movement speed factor.
	/// </summary>
	public float airSpeedFactor;
	/// <summary>
	/// Walljump movement speed factor
	/// </summary>
	public float wallJumpControlFactor;
	/// <summary>
	/// How powerful are exits from wallgrab
	/// </summary>
	public float wallGrabExitFactor;
	/// <summary>
	/// Base jump force
	/// </summary>
	public float jumpForce;

	//Store states internally
	private PlayerState playerState;
	private PlayerDirection playerDirection;

	//Various player components
	private PlayerAnimation playerAnimation;

	//Collision data
	private Vector3 collisionNormal;
	private Vector3 collisionPoint;

	//Walljump stuff
	private GameObject prevWallGrabObj;
	private GameObject wallGrabObj;

	Vector3 acceleration;
	float inputReplacement;
	float controlMultiplier;

	/// <summary>
	/// Set up a few base things, mostly other components.
	/// </summary>
	public void Awake()
	{
		SetPlayerState(PlayerState.InAirFromFall);
		playerDirection = PlayerDirection.Stationary;

		playerAnimation = GetComponent<PlayerAnimation>();

		//Destroy client scripts if we don't own this
		//Animation and movement are handled externally
		if(networkView && !networkView.isMine && gameObject.name != "MenuDude")
		{
			Destroy(playerAnimation);
			Destroy(this);
		}
	}

	public void Kill()
	{
		transform.position = Checkpoint.CurrentCheckpoint != null ? Checkpoint.CurrentCheckpoint.transform.position :
			LevelManager.Find<Spawnpoint>().Count() > 0 ? LevelManager.Find<Spawnpoint>().First().transform.position :
			Vector3.zero;

		rigidbody.velocity = Vector3.zero;
	}

	void Update()
	{
		GetPlayerDirection();
	}

	/// <summary>
	/// General player states are handled here. All hail switch!
	/// Runs independent of framerate, so Input usage is sketchy for oneshots.
	/// </summary>
	void FixedUpdate()
	{
		var hAxis = Input.GetAxis("Horizontal");
		if(//Application.isEditor ||
			Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
		{
			//Get the acceleration from the device and normalise it if necessary.
			acceleration = Input.acceleration;
			if(acceleration.sqrMagnitude > 1)
				acceleration.Normalize();

			//Flip the Y axis.
			inputReplacement = -acceleration.y;

			//We need a control multiplier for jumping on mobile to avoid really horribly overjumping
			//This needs to be positive
			if(inputReplacement < 0)
				controlMultiplier = -inputReplacement;
			else
				controlMultiplier = inputReplacement;

			if(hAxis != 0)
				inputReplacement = hAxis;
		}
		else
		{
			inputReplacement = hAxis;
			controlMultiplier = 1;
		}

		switch(GetPlayerState())
		{
			//If we're moving normally on the floor:
			case PlayerState.Grounded:
				{
					//If we're moving, play the running anim
					if(inputReplacement > 0.1 || inputReplacement < -0.1)
					{
						playerAnimation.Running();
						AllowPlayerMovement(movementSpeed, maxSpeed);
					}
					//Conveyor anim hack
					else if(inputReplacement < 0.1 || inputReplacement > -0.1)
					{
						rigidbody.velocity = Vector3.zero;
						playerAnimation.Idle();
					}
					else
					{
						playerAnimation.Idle();
					}


					AllowJump(JumpType.Standard);
				}
				break;

			//If we've pressed the jump key, do a sharp change in velocity and move to the air state
			case PlayerState.Jump:
				{
					Vector3 jumpVelocity = Vector3.up * jumpForce;
					rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity.y, rigidbody.velocity.z);
					SetPlayerState(PlayerState.InAir);
				}
				break;

			//If we've pressed the jump key coming from a wallgrab,
			//do as the normal jump but angle it according to the collision normal
			case PlayerState.WallJump:
				{
					Vector3 wallJumpVelocity = (Vector3.up + collisionNormal) * jumpForce;
					rigidbody.velocity = wallJumpVelocity;
					SetPlayerState(PlayerState.InAirFromWall);
				}
				break;

			//Used when exiting the wallgrab state
			case PlayerState.FailedWallJump:
				{
					Vector3 failedWallJumpVelocity = collisionNormal * wallGrabExitFactor;
					rigidbody.AddForce(failedWallJumpVelocity, ForceMode.VelocityChange);
					SetPlayerState(PlayerState.InAirFromWall);
				}
				break;

			//If we're flying normally, use an altered movement speed based on the air speed
			case PlayerState.InAir:
				{
					playerAnimation.InAir();
					AllowPlayerMovement(movementSpeed * airSpeedFactor * controlMultiplier, maxSpeed);
				}
				break;

			//Same as the normal falling state, except we let the player jump once
			case PlayerState.InAirFromFall:
				{
					playerAnimation.InAir();
					AllowPlayerMovement(movementSpeed * airSpeedFactor * controlMultiplier, maxSpeed);
					AllowJump(JumpType.Standard);
				}
				break;

			//If we've pushed off from a wall, consider the extra control factor
			case PlayerState.InAirFromWall:
				{
					playerAnimation.InAir();
					AllowPlayerMovement(movementSpeed * airSpeedFactor * wallJumpControlFactor * controlMultiplier,
						maxSpeed * wallJumpControlFactor);
				}
				break;

			//If we're attached to a wall:
			case PlayerState.WallGrab:
				{
					playerAnimation.WallGrab();
					rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
					AllowJump(JumpType.WallJump);
				}
				break;
		}
	}

	bool jumpWanted;

	/// <summary>
	/// Call this to allow jumping from a given state.
	/// This is used to workaround the lack of oneshot Input events in the physics step.
	/// </summary>
	void AllowJump(JumpType jumpType)
	{
		transform.parent = null;

		if(Input.touchCount > 0 || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			jumpWanted = true;
			switch(jumpType)
			{
				case JumpType.Standard:
					{
						playerAnimation.Jump();
						SetPlayerState(PlayerState.Jump);
					}
					return;
				case JumpType.WallJump:
					{
						playerAnimation.Jump();
						SetPlayerState(PlayerState.WallJump);
					}
					return;
			}
		}

		jumpWanted = false;
	}

	/// <summary>
	/// Helper method to carry out the movement for a player.
	/// </summary>
	/// <param name="multiplier">How much to multiply the movement by.</param>
	void AllowPlayerMovement(float multiplier, float cap)
	{
		if(inputReplacement < 0)
		{
			if(rigidbody.velocity.x > -cap)
			{
				rigidbody.AddForce(Vector3.left * 5 * multiplier, ForceMode.Acceleration);
			}
		}

		else if(inputReplacement > 0)
		{
			if(rigidbody.velocity.x < cap)
			{
				rigidbody.AddForce(Vector3.right * 5 * multiplier, ForceMode.Acceleration);
			}
		}
	}

	/// <summary>
	/// This uses physics to determine which way we're going.
	/// </summary>
	/// <returns>The appropriate PlayerDirection state.</returns>
	public PlayerDirection GetPlayerDirection()
	{
		//NOTE TO SELF: MOONWALKING IS FUCKING AWESOME
		//TOUCH THIS CODE AT YOUR PERIL
		if(inputReplacement > 0)
		{
			playerDirection = PlayerDirection.Right;
		}

		else if(inputReplacement < 0)
		{
			playerDirection = PlayerDirection.Left;
		}

		return playerDirection;
	}

	#region Dangerous Physics Code

	/// <summary>
	/// This is where the collision start calculations are done.
	/// NB: This is a default Unity function.
	/// </summary>
	/// <param name="collision">Collision details.</param>
	void OnCollisionEnter(Collision collision)
	{
		//Get the data from the collision
		collisionNormal = collision.contacts[0].normal;
		collisionPoint = collision.contacts[0].point;
		wallGrabObj = collision.contacts[0].otherCollider.gameObject;

		if(wallGrabObj.layer == LayerMask.NameToLayer("Projectile"))
			return;

		//transform.parent = wallGrabObj.transform;

		//If we hit the surface of an unclimbable object which isn't walkable
		/*if(wallGrabObj.CompareTag("Unclimbable"))
{
	if(GetCollisionType(collisionNormal, collisionPoint) != CollisionType.Floor)
	{
		foreach(ContactPoint contact in collision.contacts)
		{
			if(GetCollisionType(contact.normal, contact.point) == CollisionType.Floor)
			{
				SetPlayerState(PlayerState.Grounded);
				return;
			}
		}
		//SetPlayerState(PlayerState.FailedWallJump);
		return;
	}
}*/

		switch(GetPlayerState())
		{
			//If we're falling/in the air
			case PlayerState.InAir:
			case PlayerState.InAirFromWall:
			case PlayerState.InAirFromFall:
				//React according to the type of collision
				switch(GetCollisionType(collisionNormal, collisionPoint))
				{
					//If we hit a floor, switch to the running state
					case CollisionType.Floor:
						SetPlayerState(PlayerState.Grounded);
						break;

					//If we hit a wall, switch to the wallgrab state
					case CollisionType.Wall:
						SetPlayerState(PlayerState.WallGrab);
						break;
				}

				break;
		}
	}

	void OnCollisionStay(Collision collision)
	{
		if(collision.contacts.Length > 0 &&
			collision.contacts[0].otherCollider.gameObject.layer == LayerMask.NameToLayer("Projectile"))
			return;

		//Allow players to quit the wallgrab state
		if(GetPlayerState() == PlayerState.WallGrab)
		{
			if(Input.GetKey("s"))
			{
				SetPlayerState(PlayerState.FailedWallJump);
			}
		}

		//FIXME: This shouldn't need to be covered for
		if(jumpWanted || collision.contacts.Length > 1)
			return;

		//Any collision with the ground takes priority over other collisions
		foreach(var contact in collision.contacts)
		{
			switch(GetCollisionType(contact.normal, contact.point))
			{
				case CollisionType.Floor:
					SetPlayerState(PlayerState.Grounded);
					break;
			}
		}
	}

	/// <summary>
	/// Another Unity physics function, allows us to track when the character leaves the grounded state (ie, to falling).
	/// </summary>
	void OnCollisionExit(Collision collision)
	{
		switch(GetPlayerState())
		{
			case PlayerState.Grounded:
				SetPlayerState(PlayerState.InAirFromFall);
				break;

			case PlayerState.WallGrab:
				SetPlayerState(PlayerState.InAir);
				break;
		}
	}

	#endregion

	/// <summary>
	/// Determines what kind of surface we just collided with.
	/// //TODO: Add support for sloped surfaces and stop the player flying off them
	/// </summary>
	/// <param name="normal">The normal of the collision.</param>
	/// <param name="point">The collision point (currently unused).</param>
	/// <returns>The appropriate CollisionType.</returns>
	CollisionType GetCollisionType(Vector3 normal, Vector3 point)
	{
		if(normal.y > 0.4)
			return CollisionType.Floor;
		else if(normal.y <= 0.4 && normal.y >= -0.4)
			return CollisionType.Wall;
		else if(normal.y < -0.4)
			return CollisionType.Ceiling;

		return CollisionType.Unknown;
	}

	/// <summary>
	/// Setter for the player state.
	/// </summary>
	/// <param name="newState">The new state to use.</param>
	public void SetPlayerState(PlayerState newState)
	{
		playerState = newState;
	}

	/// <summary>
	/// Getter for the player state.
	/// </summary>
	/// <returns>The current player state.</returns>
	public PlayerState GetPlayerState()
	{
		return playerState;
	}
}

/// <summary>
/// Our list of states.
/// </summary>
public enum PlayerState
{
	Grounded,
	Jump,
	InAir,
	WallGrab,
	WallJump,
	InAirFromWall,
	InAirFromFall,
	FailedWallJump
}

/// <summary>
/// Enum to determine player direction.
/// </summary>
public enum PlayerDirection
{
	Left,
	Right,
	Stationary
}

/// <summary>
/// Enum for different types of jumping.
/// </summary>
public enum JumpType
{
	Standard,
	WallJump,
	FallJump,
	WallExit
}

/// <summary>
/// Enum for collision types.
/// </summary>
public enum CollisionType
{
	Wall,
	Slope,
	Floor,
	Ceiling,
	Unknown
}
