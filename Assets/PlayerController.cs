using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public int maxjumps = 1;
    public float moveSpeed;
    public float jumpForce = 1f;
    public float torsoHeight = 1f;


    private Rigidbody2D myBody;
    private SpriteRenderer myRenderer;
    private Animator myAnimator;
    private int jumpsLeft = 1;
    public bool airborne = false;

    // Start is called before the first frame update
    void Start()
    {
        jumpsLeft = maxjumps;
        myBody = this.GetComponent<Rigidbody2D>();
        myAnimator = this.GetComponent<Animator>();
        myRenderer = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
		{
            Debug.Log("Jump Input detected");
            Jump();
		}

        // Check for ground underneath
        airborne = true;
        RaycastHit2D rc = Physics2D.Raycast(transform.position, Vector2.down, torsoHeight);
        Debug.DrawRay(transform.position, torsoHeight * Vector2.down, Color.green, 1f);
        if(rc.collider != null && rc.collider.tag == "ground")
		{
            airborne = false;
		}

        ToggleAnimation();
        ToggleOrientation();
    }

	private void FixedUpdate()
	{
        myBody.velocity = new Vector3(moveSpeed * Input.GetAxis("Horizontal"), myBody.velocity.y);
	}

	private void Jump()
	{
        myBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.collider.tag == "ground")
		{
            jumpsLeft = maxjumps;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("door"))
			return;
        GameObject doorObj = other.gameObject;
        GameObject roomObj = doorObj.transform.parent.gameObject;
        Room room = roomObj.GetComponent<Room>();
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
    }

    private void LeaveRoomThroughDoor(RoomNode oldRoom, RoomSide oldDoorSide, int oldDoorNum)
    {
        var newDoorSide = Room.OppositeSide(oldDoorSide);
        var newDoorNum = oldRoom.GetRoomDoorNums(oldDoorSide)[oldDoorNum];   // todo !!! set this!!
        Debug.Log("enter door from door num"+oldDoorNum+"to"+newDoorNum);
        RoomNode newRoom = oldRoom.GetRooms(oldDoorSide)[oldDoorNum];
        var newDoor = newRoom.roomObject.GetComponent<Room>().GetDoor(newDoorSide, newDoorNum);
        transform.position = newDoor.transform.position + Room.RoomSideToVec(newDoorSide);
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

            ChangeAnimatorState("Player_Run");
            return;
		}

        ChangeAnimatorState("Player_Idle");
	}

    private void ChangeAnimatorState(string targetState)
	{
        // Hab keine Bock meheeer
        myAnimator.PlayInFixedTime(targetState);
	}
}
