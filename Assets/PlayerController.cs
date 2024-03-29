using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public int maxjumps = 1;
    public float moveSpeed;
    public float jumpForce = 1f;
    public float torsoHeight = 1f;
    public float maxVelY = 10f;
    public GameObject attackPrefab;
    public float leftAttackOffset = 0;
    public float rightAttackOffset = 0;
	public float attackSpeed = 1;
	public float attackPenalty = 0f;
	public float knockbackMultiplier = 1f;

    private Rigidbody2D myBody;
    private SpriteRenderer myRenderer;
    private Animator myAnimator;
    private Collider2D myCollider;
    private HealthComponent myHealth;
    private Camera myCamera;
    [SerializeField] private int jumpsLeft = 1;
	public bool dead = false;
    [SerializeField] private bool airborne = false;
	[SerializeField] private bool wait = false;
	private bool crouch = false; 
    public RoomNode currentRoomNode;
	public float rcWidth;
    public float moveAnimRatio = 0.1f;
    [SerializeField] private bool attacking = false;
    public RoomSide orientation = RoomSide.Right;
	public LayerMask groundMask;


    // Start is called before the first frame update
    void Start()
    {
	    Physics2D.queriesStartInColliders = false;
        jumpsLeft = maxjumps;
		dead = false;
        myBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        myHealth = GetComponent<HealthComponent>();
        myCamera = Camera.main;
		groundMask = LayerMask.GetMask("Platform", "Ground");
    }

    // Update is called once per frame
    void Update()
    {
		HandleInput();
		// Check for ground underneath
		CheckAirborne();
        ToggleOrientation();
    }

	private void CheckAirborne()
	{
		RaycastHit2D[] rc1 = Physics2D.RaycastAll(transform.position, Vector2.down, torsoHeight, groundMask.value);
		if (rc1.Length != 0)
		{
			foreach (var sth in rc1)
			{
				if (sth.collider.CompareTag("ground"))
				{
					airborne = false;
					return;
				}
			}
			if (crouch)
			{
				airborne = true;
				// Agressive raycasts
				Collider2D left = Physics2D.Raycast(transform.position + Vector3.left * rcWidth, Vector2.down, torsoHeight, groundMask.value).collider;
				Collider2D right = Physics2D.Raycast(transform.position + Vector3.right * rcWidth, Vector2.down, torsoHeight, groundMask.value).collider;
				Collider2D center = Physics2D.Raycast(transform.position, Vector2.down, torsoHeight, groundMask.value).collider;
				Debug.DrawRay(transform.position + Vector3.left * rcWidth, torsoHeight * Vector2.down, Color.green, 1f);
				Debug.DrawRay(transform.position + Vector3.right * rcWidth, torsoHeight * Vector2.down, Color.green, 1f);
				Debug.DrawRay(transform.position, torsoHeight * Vector2.down, Color.green, 1f);

				if (left != null && left.tag == "platform") StartCoroutine(PlatformFall(left));
				if (right != null && right.tag == "platform") StartCoroutine(PlatformFall(right));
				if (center != null && center.tag == "platform") StartCoroutine(PlatformFall(center));

				return;
			}
			else
			{
				airborne = false;
			}
		}
		else
		{
			airborne = true;
		}
	}

	private void HandleInput()
	{
		if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
		{
			Debug.Log("Jump Input detected");
			Jump();
			return;
		}
		if (Input.GetButtonDown("Fire1"))
		{
			Debug.Log("Attack Input detected");
			Attack();
			return;
		}
		if (!airborne && Input.GetAxis("Vertical") < 0)
		{
			crouch = true;
		}
		else
		{
			crouch = false;
		}
	}

	private void LateUpdate()
	{
		var b = currentRoomNode.myRoom.getBounds();
		if (b.size == Vector3.zero)
			return;  // wait for loading
		var widthOffset = transform.localScale.x / 2;
		var heightOffset = transform.localScale.y / 2;
		float clampX = Mathf.Clamp(transform.position.x, b.min.x + widthOffset, b.max.x - widthOffset);
		float clampY = Mathf.Clamp(transform.position.y, b.min.y + heightOffset, b.max.y - heightOffset);
		transform.position = new Vector3(clampX, clampY, transform.position.z);

		ToggleAnimation();
	}

	private void FixedUpdate()
	{
		if (!attacking && !crouch)
		{
			if (!myHealth.knockedBack)
			{
				myBody.velocity = new Vector3(moveSpeed * Input.GetAxis("Horizontal"), Mathf.Clamp(myBody.velocity.y, -maxVelY, maxVelY));
			}
		}
		else if(attacking)
		{
			myBody.velocity = new Vector3(moveSpeed * Input.GetAxis("Horizontal") * attackPenalty, Mathf.Clamp(myBody.velocity.y, -maxVelY, maxVelY) * attackPenalty);
		}
		else
		{
			myBody.velocity = new Vector3(0f, Mathf.Clamp(myBody.velocity.y, -maxVelY, maxVelY));
		}
	}

	private void Jump()
	{
		if (!attacking)
		{
            myBody.velocity = new Vector3(myBody.velocity.x, Mathf.Clamp(jumpForce, -maxVelY, maxVelY));
            //myBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsLeft--;
        }
	}

	private void Attack()
	{
		if (attacking)
		{
			return;
		}
		ChangeAnimatorState("Player_Attack", attackSpeed);
		wait = true;
		StartCoroutine(SpawnAttack());
	}

	private IEnumerator SpawnAttack()
	{
		yield return new WaitUntil(() => wait == false);
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = myCamera.nearClipPlane;
		var worldPos = myCamera.ScreenToWorldPoint(mousePos);
		var selfPos = transform.position;
		// var attackDir = (worldPos.x < selfPos.x) ? RoomSide.Left : RoomSide.Right;
		var attackDir = orientation;
		var attackDirInt = attackDir == RoomSide.Left ? -1 : 1;
		var attack = Instantiate(attackPrefab, transform.parent);
		var attackController = attack.GetComponent<AttackController>();
		var extraOffset = attackDir == RoomSide.Left ? leftAttackOffset : rightAttackOffset;
		attack.transform.position = transform.position +
									new Vector3(attackDirInt * (extraOffset + attack.transform.localScale.x / 2), 0, 0);
		attackController.active = true;
		attackController.knockback *= knockbackMultiplier;
		Vector2 knockBackDir = attackDir == RoomSide.Left ? Vector2.left : Vector2.right;
		attackController.StartAttack(attackDir, knockBackDir);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("enemy"))
		{
			// TODO: do this too: but then also add a new trigger.
			// Physics2D.IgnoreCollision(myCollider, collision.collider);
			// take damage.
			HealthComponent otherHealthComp = collision.collider.GetComponent<HealthComponent>();
			if (!otherHealthComp.IsDead())
				myHealth.TakeDamage(otherHealthComp.touchDamage);
		}
		else if (collision.collider.CompareTag("ground"))
		{
            jumpsLeft = maxjumps;
			Debug.Log("jump reset by" + collision.collider);
		} 
		else if (collision.collider.CompareTag("pickup"))
		{
			var pickupComp = collision.collider.GetComponent<PickupableItem>();
			pickupComp.OnPickup(gameObject);
		}
		else if (collision.collider.CompareTag("platform"))
		{
			if (!crouch)
			{
				jumpsLeft = maxjumps;
				Debug.Log("jump reset by" + collision.collider);
			}
		}
	}

	private IEnumerator PlatformFall(Collider2D collider)
	{
		crouch = false;
		groundMask = LayerMask.GetMask("Ground");
		Physics2D.IgnoreCollision(collider, myCollider);
		yield return new WaitForSeconds(0.5f);
		Physics2D.IgnoreCollision(collider, myCollider, false);
		groundMask = LayerMask.GetMask("Ground", "Platform");
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		GameObject doorObj = other.gameObject;
		if (doorObj.transform.parent == null)
			return;
		GameObject roomObj = doorObj.transform.parent.gameObject;
		Room room = roomObj.GetComponent<Room>();
		if (other.CompareTag("door"))
		{
			int leftIdx = room.GetDoors(RoomSide.Left).IndexOf(doorObj);
			int rightIdx = room.GetDoors(RoomSide.Right).IndexOf(doorObj);
			if (leftIdx != -1)
			{
				LeaveRoomThroughDoor(room.roomNode, RoomSide.Left, leftIdx);
			}
			else if (rightIdx != -1)
			{
				LeaveRoomThroughDoor(room.roomNode, RoomSide.Right, rightIdx);
			}
		} else if (other.CompareTag("down"))
		{
			room.roomNode.manager.EnterNextStage();
		}
    }

    private void LeaveRoomThroughDoor(RoomNode oldRoom, RoomSide oldDoorSide, int oldDoorNum)
    {
	    if (!oldRoom.myRoom.IsCleared())
		    return;
        var newDoorSide = Room.OppositeSide(oldDoorSide);
        var newDoorNum = oldRoom.GetRoomDoorNums(oldDoorSide)[oldDoorNum];   // todo !!! set this!!
        Debug.Log("enter door from door num"+oldDoorNum+"to"+newDoorNum);
        RoomNode newRoom = oldRoom.GetNeighborRooms(oldDoorSide)[oldDoorNum];
        var newDoor = newRoom.roomObject.GetComponent<Room>().GetDoor(newDoorSide, newDoorNum);
        transform.position = newDoor.transform.position - Room.RoomSideToVec(newDoorSide);
        currentRoomNode = newRoom;
		currentRoomNode.manager.ReloadMinimap();
    }

    private void ToggleOrientation()
	{
		if (attacking)
		{
            return;
		}
		if (myRenderer.flipX && Input.GetAxis("Horizontal") > 0)
		{
			orientation = RoomSide.Right;
            myRenderer.flipX = false;
            return;
		}
        if (!myRenderer.flipX && Input.GetAxis("Horizontal") < 0)
		{
			orientation = RoomSide.Left;
            myRenderer.flipX = true;
            return;
        }

	}

    private void ToggleAnimation()
	{
		if(dead){
			return;
		}
		// Attack Animation
		if (attacking)
		{
            return;
		}

        float yVel = myBody.velocity.y;
        if (airborne || myHealth.knockedBack)
		{
            if(yVel >= 0)
			{
                // Player is rising
                ChangeAnimatorState("Player_Rise");
				return;
			}
            else
			{
                // Player is falling 
                ChangeAnimatorState("Player_Fall");
				return;
			}
            
        }

		if (crouch)
		{
			ChangeAnimatorState("Player_Crouch");
			return;
		}

		if (Input.GetAxis("Horizontal") != 0)
		{

			ChangeAnimatorState("Player_Run", moveSpeed * moveAnimRatio);

			return;
		}

        ChangeAnimatorState("Player_Idle");
    }

    private void ChangeAnimatorState(string targetState, float speed = 1f)
	{
		// Hab keine Bock meheeer
		myAnimator.speed = speed;
		myAnimator.PlayInFixedTime(targetState);
	}

	public void Die(){
		myBody.velocity = Vector3.zero;
		ChangeAnimatorState("Player_Die");
	}
}

