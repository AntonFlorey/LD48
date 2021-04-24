using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Windows.WebCam;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> roomPrefabs;
    public GameObject player;

    public int roomWidth = 100;

    private List<RoomNode> roomNodes = new List<RoomNode>();
    private int roomCount = 0;

    private class RoomNode
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

    private int CountDoorsAtSide(RoomSide side, List<RoomNode> nodes)
    {
        var doorsLeft = 0;
        foreach (RoomNode room in nodes)
        {
            doorsLeft += room.GetDoorCount(side);
        }
        return doorsLeft;
    }

    private RoomNode GetRoomWithMaxDoors(RoomSide side, int maxIncomingDoors, int minOutgoingDoors,
        int maxOutgoingDoors)
    {
        // Debug.Log("for side" + side + "make max" + maxIncomingDoors + "," + minOutgoingDoors + "," + maxOutgoingDoors);
        Assert.IsTrue(maxIncomingDoors >= 1);
        if (maxOutgoingDoors < 0)
            maxOutgoingDoors = 0;
        List<GameObject> available = new List<GameObject>();
        foreach (GameObject roomPrefab in roomPrefabs)
        {
            var incomingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(side).Count;
            var outgoingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(Room.OppositeSide(side)).Count;
            if (1 <= incomingDoorCount && incomingDoorCount <= maxIncomingDoors &&
                minOutgoingDoors <= outgoingDoorCount && outgoingDoorCount <= maxOutgoingDoors)
                available.Add(roomPrefab);
        }
        Assert.IsTrue(available.Count > 0);
        var chosenRoomPrefab = available[Random.Range(0, available.Count)];
        return new RoomNode(this, chosenRoomPrefab);
    }

    private List<RoomNode> GenerateNextRooms(RoomSide side, List<RoomNode> currentRooms, int shouldStillGenerate)
    {
        var oppositeSide = Room.OppositeSide(side);
        var currRoomNum = 0;
        var doorsLeft = CountDoorsAtSide(side, currentRooms);
        var nextRooms = new List<RoomNode>();
        while (doorsLeft > 0)
        {
            var minOutgoingDoors = shouldStillGenerate <= 0 ? 0 : 1;
            // find a room with <= rooms at opposite of side.
            RoomNode nextNode = GetRoomWithMaxDoors(
                oppositeSide, doorsLeft, minOutgoingDoors, shouldStillGenerate);
            nextRooms.Add(nextNode);
            var nodesToMake = nextNode.GetDoorCount(oppositeSide);
            Assert.IsTrue(nodesToMake > 0);
            doorsLeft -= nodesToMake;
            Assert.IsTrue(doorsLeft >= 0);
            for (var doorNum = 0; doorNum < nodesToMake; doorNum++)
            {
                while (currentRooms[currRoomNum].GetRooms(side).Count >= currentRooms[currRoomNum].GetDoorCount(side))
                {
                    currRoomNum++;
                }
                currentRooms[currRoomNum].GetRooms(side).Add(nextNode);
                nextNode.GetRooms(oppositeSide).Add(currentRooms[currRoomNum]);
            }
        }
        return nextRooms;
    }

    public void Start()
    {
        // generate map!
        var startNode = new RoomNode(this, roomPrefabs[0]);
        List<RoomNode> leftmostRooms = new List<RoomNode>();
        List<RoomNode> rightmostRooms = new List<RoomNode>();
        leftmostRooms.Add(startNode);
        rightmostRooms.Add(startNode);
        roomNodes.Add(startNode);
        int shouldStillGenerate = 5;
        int stepsLeft = shouldStillGenerate + 10;
        while (CountDoorsAtSide(RoomSide.Left, leftmostRooms) + CountDoorsAtSide(RoomSide.Left, rightmostRooms) > 0)
        {
            if (stepsLeft <= 0)
                Assert.IsTrue(false);

            if (Random.Range(0, 2) == 0)
            {
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate);
                shouldStillGenerate -= leftmostRooms.Count;
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate);
                shouldStillGenerate -= rightmostRooms.Count;
            }
            else
            {
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate);
                shouldStillGenerate -= rightmostRooms.Count;
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate);
                shouldStillGenerate -= leftmostRooms.Count;
            }
            roomNodes.AddRange(leftmostRooms);
            roomNodes.AddRange(rightmostRooms);

            stepsLeft--;
        }
        Assert.AreEqual(roomCount, roomNodes.Count);
        foreach (RoomNode node in roomNodes)
        {
            node.SetupInstance();
        }
        
        EnterRoom(startNode, RoomSide.Right, 0);
    }

    private void EnterRoom(RoomNode roomNode, RoomSide side, int doorNum)
    {
        var room = roomNode.roomObject.GetComponent<Room>();
        var door = room.GetDoor(side, doorNum);
        player.transform.position = room.transform.position + door.transform.position;
    }
}
