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
    public GameObject camera;
    public GameObject slashAttack;

    private Rigidbody2D myBody;
    private SpriteRenderer myRenderer;
    private Animator myAnimator;
    private Collider2D myCollider;
    private HealthComponent myHealth;
    private Camera myCamera;
    private int jumpsLeft = 1;
    public bool airborne = false;
    public RoomNode currentRoomNode;
    public float moveAnimRatio = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
	    Physics2D.queriesStartInColliders = false;
        jumpsLeft = maxjumps;
        myBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        myHealth = GetComponent<HealthComponent>();
        myCamera = camera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
		{
            Debug.Log("Jump Input detected");
            Jump();
		}
		if (Input.GetButtonDown("Fire1"))
		{
			Debug.Log("Attack Input detected");
			Attack();
		}

        // Check for ground underneath
        RaycastHit2D rc = Physics2D.Raycast(transform.position, Vector2.down, torsoHeight);
        Debug.DrawRay(transform.position, torsoHeight * Vector2.down, Color.green, 1f);
        airborne = rc.collider == null;

        ToggleAnimation();
        ToggleOrientation();
    }

	private void FixedUpdate()
	{
        myBody.velocity = new Vector3(moveSpeed * Input.GetAxis("Horizontal"), Mathf.Clamp(myBody.velocity.y, -maxVelY, maxVelY));
	}

	private void Jump()
	{
        myBody.velocity = new Vector3(myBody.velocity.x, Mathf.Clamp(jumpForce, -maxVelY, maxVelY));
        //myBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
	}

	private void Attack()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = myCamera.nearClipPlane;
		var worldPos = myCamera.ScreenToWorldPoint(mousePos);
		var selfPos = transform.position;
		var attackDir = (worldPos.x < selfPos.x) ? RoomSide.Left : RoomSide.Right;
		var attackDirInt = attackDir == RoomSide.Left ? -1 : 1;
		Debug.Log(attackDir);
		var attack = Instantiate(slashAttack, transform.parent);
		attack.transform.position = transform.position + new Vector3(attackDirInt * transform.localScale.x / 2, 0, 0);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("enemy"))
		{
			// TODO: do this too: but then also add a new collider
			// Physics2D.IgnoreCollision(myCollider, collision.collider);
			// take damage.
			HealthComponent otherHealthComp = collision.collider.GetComponent<HealthComponent>();
			myHealth.TakeDamage(otherHealthComp.touchDamage);
		}
		else if (collision.collider.CompareTag("ground"))
		{
            jumpsLeft = maxjumps;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
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
			Debug.Log("entering new stage " + (room.roomNode.manager.currentStage+1));
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
        transform.position = newDoor.transform.position + Room.RoomSideToVec(newDoorSide);
        currentRoomNode = newRoom;
    }

    private void ToggleOrientation()
	{
		if (myRenderer.flipX && Input.GetAxis("Horizontal") > 0)
		{
            myRenderer.flipX = false;
            return;
		}
        if (!myRenderer.flipX && Input.GetAxis("Horizontal") < 0)
		{
            myRenderer.flipX = true;
            return;
        }

	}

    private void ToggleAnimation()
	{
        float yVel = myBody.velocity.y;
        if (airborne)
		{
            if(yVel > 0)
			{
                // Player is rising
                ChangeAnimatorState("Player_Rise");
            }
            else
			{
                // Player is falling 
                ChangeAnimatorState("Player_Fall"); // TODO
            }
            return;
        }

        if(Input.GetAxis("Horizontal") != 0)
		{

            ChangeAnimatorState("Player_Run", moveSpeed * moveAnimRatio);
            
            return;
		}

        ChangeAnimatorState("Player_Idle");
        myAnimator.speed = 1f;

    }

    private void ChangeAnimatorState(string targetState, float speed = 1f)
	{
        // Hab keine Bock meheeer
        myAnimator.PlayInFixedTime(targetState);
        myAnimator.speed = speed;
    }
}
