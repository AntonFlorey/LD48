using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomNode
{
    public readonly RoomManager manager;
    private readonly GameObject roomPrefab;
    private readonly List<RoomNode> leftNeighborRooms = new List<RoomNode>();
    private readonly List<RoomNode> rightNeighborRooms = new List<RoomNode>();
    private readonly List<int> leftNeighborDoorNums = new List<int>();
    private readonly List<int> rightNeighborDoorNums = new List<int>();

    public GameObject roomObject;
    public Room myRoom;

    public RoomNode(RoomManager manager, GameObject roomPrefab)
    {
        this.manager = manager;
        this.roomPrefab = roomPrefab;
        
        var baseTransform = manager.transform;
        var pos = baseTransform.position + new Vector3(manager.roomCount * manager.roomWidth, 0, 0);
        manager.roomCount++;
        roomObject = UnityEngine.Object.Instantiate(roomPrefab, pos, baseTransform.rotation);
        myRoom = roomObject.GetComponent<Room>();
        myRoom.roomNode = this;
    }

    public List<RoomNode> GetNeighborRooms(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftNeighborRooms;
            case RoomSide.Right:
                return rightNeighborRooms;
            default:
                Assert.IsTrue(false);
                return null;
        }
    }

    public List<int> GetRoomDoorNums(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftNeighborDoorNums;
            case RoomSide.Right:
                return rightNeighborDoorNums;
            default:
                Assert.IsTrue(false);
                return null;
        }
    }

    public int GetDoorCount(RoomSide side)
    {
        return roomPrefab.GetComponent<Room>().GetDoors(side).Count;
    }

    public void SetupInstance()
    {
        var leftDoors = roomObject.GetComponent<Room>().leftDoors;
        var rightDoors = roomObject.GetComponent<Room>().rightDoors;
        Assert.AreEqual(GetNeighborRooms(RoomSide.Left).Count, GetDoorCount(RoomSide.Left));
        Assert.AreEqual(GetNeighborRooms(RoomSide.Right).Count, GetDoorCount(RoomSide.Right));
    }
}