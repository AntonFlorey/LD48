using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RoomNode
{
    private readonly RoomManager manager;
    private readonly GameObject roomPrefab;
    private readonly List<RoomNode> leftRooms = new List<RoomNode>();
    private readonly List<RoomNode> rightRooms = new List<RoomNode>();

    public GameObject roomObject;

    public RoomNode(RoomManager manager, GameObject roomPrefab)
    {
        this.manager = manager;
        this.roomPrefab = roomPrefab;
        
        var baseTransform = manager.transform;
        var pos = baseTransform.position + new Vector3(manager.roomCount * manager.roomWidth, 0, 0);
        manager.roomCount++;
        roomObject = UnityEngine.Object.Instantiate(roomPrefab, pos, baseTransform.rotation);
        roomObject.GetComponent<Room>().roomNode = this;
    }

    public List<RoomNode> GetRooms(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftRooms;
            case RoomSide.Right:
                return rightRooms;
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
        Assert.AreEqual(GetRooms(RoomSide.Left).Count, GetDoorCount(RoomSide.Left));
        Assert.AreEqual(GetRooms(RoomSide.Right).Count, GetDoorCount(RoomSide.Right));
    }
}