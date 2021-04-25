using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> roomPrefabs;
    public GameObject player;
    public PlayerController myPlayer;
    public GameObject currentStageText;
    private Text myCurrentStageText;

    public int roomWidth = 100;

    public int currentStage = 0;
    private List<RoomNode> roomNodes = new List<RoomNode>();
    public int roomCount = 0;

    private int CountDoorsAtSide(RoomSide side, List<RoomNode> nodes)
    {
        var doorsLeft = 0;
        foreach (RoomNode room in nodes)
        {
            doorsLeft += room.GetDoorCount(side);
        }
        return doorsLeft;
    }

    private class ShouldStillGenerate
    {
        public int numGenericRooms;
        public bool needWayDown;

        public ShouldStillGenerate(int numGenericRooms, bool needWayDown)
        {
            if (numGenericRooms < 0)
                numGenericRooms = 0;
            this.numGenericRooms = numGenericRooms;
            this.needWayDown = needWayDown;
        }

        public bool IsDone()
        {
            return numGenericRooms <= 0 && !needWayDown;
        }

        public void AddMadeNode(RoomNode newNode)
        {
            var isWayDown = newNode.roomObject.GetComponent<Room>().wayDown != null;
            if (isWayDown)
            {
                Assert.IsTrue(needWayDown);
                needWayDown = false;
            } else
            {
                numGenericRooms = Math.Max(0, numGenericRooms - 1);
            }
        }
    }

    private RoomNode GetRoomWithMaxDoors(RoomSide side, int maxIncomingDoors, ShouldStillGenerate shouldStillGenerate)
    {
        Assert.IsTrue(maxIncomingDoors >= 1);
        var minOutgoingDoors = shouldStillGenerate.IsDone() ? 0 : 1;
        // Debug.Log("for side" + side + "make max" + maxIncomingDoors + "," + minOutgoingDoors + "," + maxOutgoingDoors);
        var maxOutgoingGeneralDoors = shouldStillGenerate.numGenericRooms;
        List<GameObject> available = new List<GameObject>();
        foreach (GameObject roomPrefab in roomPrefabs)
        {
            var incomingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(side).Count;
            var outgoingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(Room.OppositeSide(side)).Count;
            if (!(1 <= incomingDoorCount && incomingDoorCount <= maxIncomingDoors &&
                  minOutgoingDoors <= outgoingDoorCount))
                continue;
            var room = roomPrefab.GetComponent<Room>();
            if (room.entryPoint != null)
                continue;
            var isWayDown = room.wayDown != null;
            if (shouldStillGenerate.needWayDown && isWayDown)
            {
                available.Add(roomPrefab);
            } else if (!isWayDown && outgoingDoorCount <= maxOutgoingGeneralDoors)
            {
                available.Add(roomPrefab);
            }
        }
        Assert.IsTrue(available.Count > 0);
        var chosenRoomPrefab = available[Random.Range(0, available.Count)];
        var madeNode = new RoomNode(this, chosenRoomPrefab);
        shouldStillGenerate.AddMadeNode(madeNode);
        return madeNode;
    }

    private List<RoomNode> GenerateNextRooms(RoomSide side, List<RoomNode> currentRooms, ShouldStillGenerate shouldStillGenerate)
    {
        var oppositeSide = Room.OppositeSide(side);
        var currRoomNum = 0;
        var doorsLeft = CountDoorsAtSide(side, currentRooms);
        var nextRooms = new List<RoomNode>();
        while (doorsLeft > 0)
        {
            // find a room with <= rooms at opposite of side.
            RoomNode nextNode = GetRoomWithMaxDoors(
                oppositeSide, doorsLeft, shouldStillGenerate);
            nextRooms.Add(nextNode);
            var nodesToMake = nextNode.GetDoorCount(oppositeSide);
            Assert.IsTrue(nodesToMake > 0);
            doorsLeft -= nodesToMake;
            Assert.IsTrue(doorsLeft >= 0);
            for (var doorNum = 0; doorNum < nodesToMake; doorNum++)
            {
                while (currentRooms[currRoomNum].GetNeighborRooms(side).Count >= currentRooms[currRoomNum].GetDoorCount(side))
                {
                    currRoomNum++;
                }

                var currentRoomDoorNum = currentRooms[currRoomNum].GetNeighborRooms(side).Count;
                currentRooms[currRoomNum].GetNeighborRooms(side).Add(nextNode);
                currentRooms[currRoomNum].GetRoomDoorNums(side).Add(doorNum);
                nextNode.GetNeighborRooms(oppositeSide).Add(currentRooms[currRoomNum]);
                nextNode.GetRoomDoorNums(oppositeSide).Add(currentRoomDoorNum);
            }
        }
        return nextRooms;
    }

    public void Start()
    {
        myPlayer = player.GetComponent<PlayerController>();
        currentStage = 1;
        myCurrentStageText = currentStageText.GetComponent<Text>();
        GenerateMap();
    }

    public void GenerateMap()
    {
        myCurrentStageText.text = "Stage " + currentStage;
        foreach (RoomNode node in roomNodes)
        {
            Destroy(node.roomObject);
        }
        roomCount = 0;
        roomNodes.Clear();
        var startNode = new RoomNode(this, roomPrefabs[0]);
        List<RoomNode> leftmostRooms = new List<RoomNode>();
        List<RoomNode> rightmostRooms = new List<RoomNode>();
        leftmostRooms.Add(startNode);
        rightmostRooms.Add(startNode);
        roomNodes.Add(startNode);
        var shouldStillGenerate = new ShouldStillGenerate(5, true);
        int stepsLeft = shouldStillGenerate.numGenericRooms + 10;
        while (CountDoorsAtSide(RoomSide.Left, leftmostRooms) + CountDoorsAtSide(RoomSide.Left, rightmostRooms) > 0)
        {
            if (stepsLeft <= 0)
                Assert.IsTrue(false);

            if (Random.Range(0, 2) == 0)
            {
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate);
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate);
            }
            else
            {
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate);
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate);
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

        var entryPos = startNode.roomObject.GetComponent<Room>().entryPoint.transform.position;
        player.transform.position = new Vector3(entryPos.x, entryPos.y, player.transform.position.z);
        myPlayer.currentRoomNode = startNode;
    }

    public void EnterNextStage()
    {
        currentStage++;
        GenerateMap();
    }

    public RoomNode GetCurrentRoom()
    {
        Debug.Log("yos");
        return myPlayer.currentRoomNode;
    }
}
