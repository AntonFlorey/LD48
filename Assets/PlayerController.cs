using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        transform.position += new Vector3(x, y, 0) * Time.deltaTime * moveSpeed;
    }

    private void Jump()
	{

	}
    
    private void OnTriggerEnter2D(Collider2D other)
    {
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
        Debug.Log(oldRoom + "/" + oldDoorSide + "/" + oldDoorNum);
        var newDoorSide = Room.OppositeSide(oldDoorSide);
        var newDoorNum = 0;   // todo !!! set this!!
        RoomNode newRoom = oldRoom.GetRooms(oldDoorSide)[oldDoorNum];
        var newDoor = newRoom.roomObject.GetComponent<Room>().GetDoor(newDoorSide, newDoorNum);
        transform.position = newRoom.roomObject.transform.position + newDoor.transform.position + 
                             Room.RoomSideToVec(newDoorSide);
    }
}
