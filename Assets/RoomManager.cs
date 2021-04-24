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

    private class RoomNode
    {
        private readonly GameObject roomPrefab;
        private readonly List<RoomNode> leftRooms = new List<RoomNode>();
        private readonly List<RoomNode> rightRooms = new List<RoomNode>();

        public RoomNode(GameObject roomPrefab)
        {
            this.roomPrefab = roomPrefab;
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
        Debug.Log("for side" + side + "make max" + maxIncomingDoors + "," + minOutgoingDoors + "," + maxOutgoingDoors);
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
        return new RoomNode(chosenRoomPrefab);
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
            Debug.Log("made nodesToMake=" + nodesToMake);
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
        var startNode = new RoomNode(roomPrefabs[0]);
        List<RoomNode> leftmostRooms = new List<RoomNode>();
        List<RoomNode> rightmostRooms = new List<RoomNode>();
        leftmostRooms.Add(startNode);
        rightmostRooms.Add(startNode);
        int shouldStillGenerate = 1;
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

            stepsLeft--;
        }
        Debug.Log("Made it!");
        
        EnterRoom(roomPrefabs[0], RoomSide.Right, 0);
    }

    private void EnterRoom(GameObject roomPrefab, RoomSide side, int doorNum)
    {
        var roomTransform = roomPrefab.transform;
        var roomObject = Instantiate(roomPrefab, roomTransform.position, roomTransform.rotation);
        var room = roomObject.GetComponent<Room>();
        var door = room.GetDoor(side, doorNum);
        player.transform.position = room.transform.position + door.transform.position;
    }
}
