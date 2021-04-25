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

    public int currentLevel = 0;
    public StageNode currentLevelStartStage;
    public StageNode currentStage;
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
        currentLevelStartStage = GenerateLevelStartStage();
        currentStage = currentLevelStartStage;

        myCurrentStageText = currentStageText.GetComponent<Text>();
        GenerateMap();
    }

    public void GenerateMap()
    {
        myCurrentStageText.text = "Stage " + (currentStage.stageDepth+1);
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
        // todo set number of needWayDown rooms.
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

    private static int RandomTowardsTarget(int old, int target, int maxDiff, int noise=1)
    {
        int center = (int) Mathf.MoveTowards(old, target, Random.Range(maxDiff - noise, maxDiff + noise));
        return Random.Range(center - noise, center + 2);
    }

    private StageNode GenerateLevelStartStage()
    {
        int totalDepth = 3;
        int targetWidth = 3;
        int maxWidthDiff = 2;
        Debug.Log("Making stages:");
        List<StageNode> prevStages = new List<StageNode>();
        StageNode start = new StageNode(this, 0, 0, StageNode.StageType.Normal, 0, 0);
        prevStages.Add(start);
        for (var depth = 1; depth < totalDepth; depth++)
        {
            Debug.Log("Depth=" + depth);
            int prevWidth = prevStages.Count;
            int newWidth = Math.Max(RandomTowardsTarget(prevWidth, targetWidth, maxWidthDiff), 2);
            Debug.Log("Will make" + newWidth + " new");
            List<StageNode> newStages = new List<StageNode>();
            int connectedPrevUntil = 0;
            for (var newStageNum = 0; newStageNum < newWidth; newStageNum++)
            {
                Debug.Log("Make new " + newStageNum);
                // if we have an equal mapping, every prev has to point to newWidth / prevWidth.
                int connectToLeft = prevWidth - connectedPrevUntil - 1;
                bool connectAgain = Random.Range(0, 1) == 0;
                connectAgain |= newStageNum == 0;  // have to connect to first.
                connectAgain |= connectToLeft == 0;  // have to connect if nothing else left
                int willConnectTo = connectToLeft / (newWidth - newStageNum);
                willConnectTo = Random.Range(willConnectTo - 1, willConnectTo + 1);  // add some noise
                willConnectTo = Math.Min(Math.Max(willConnectTo, 1), connectToLeft);
                Debug.Log("Decided to connect again=" + connectAgain + " and to news=" + willConnectTo + " from the" + connectToLeft + " that are left");
                
                // todo: add risk+reward
                var newStage = new StageNode(this, depth, newStageNum, StageNode.StageType.Normal, 0, 0);

                var startAtOldStageNum = connectedPrevUntil + (connectAgain ? 0 : 1);
                var newConnectPrevUntil = connectedPrevUntil + willConnectTo;
                Debug.Log("Thus will connect to prev nodes from " + startAtOldStageNum + " to " + (connectedPrevUntil + 1 + willConnectTo));
                for (var oldStageNum = startAtOldStageNum; oldStageNum <= newConnectPrevUntil; oldStageNum++)
                {
                    prevStages[oldStageNum].nextStages.Add(newStage);
                }
                newStages.Add(newStage);
                connectedPrevUntil = newConnectPrevUntil;
            }
            prevStages = newStages;
        }

        Debug.Log("start stage is:");
        Debug.Log(start);
        return start;
    }

    public void EnterNextStage()
    {
        if (currentStage.nextStages.Count == 0)
        {
            // final thingy, move on.
            currentLevel += 1;
            currentLevelStartStage = GenerateLevelStartStage();
            currentStage = currentLevelStartStage;
        }
        else
        {
            // TODO: handle this properly!!!
            currentStage = currentStage.nextStages[0];
        }    
        GenerateMap();
    }

    public RoomNode GetCurrentRoom()
    {
        return myPlayer.currentRoomNode;
    }
}
