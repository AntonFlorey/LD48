using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public StageNode.StageType firstStageType = StageNode.StageType.Normal;
    public List<GameObject> roomPrefabs;
    public GameObject player;
    public PlayerController myPlayer;
    public GameObject currentStageText;
    private Text myCurrentStageText;
    public MiniMap minimap;
    public GameObject initialLevelRoomPrefab;

    public int roomWidth = 100;

    public int levelCount = 4;
    public int currentLevel = 0;
    public StageNode currentLevelStartStage;
    public StageNode currentStage;
    private List<RoomNode> roomNodes = new List<RoomNode>();
    public int roomCount = 0;
    public float[] stageTypeFrequencies;
    public bool waitingForDeathClick = false;

    public int levelStageDepth = 16;
    public List<int> levelTargetStageWidths;
    public List<int> levelTargetNumGenericRoomsPerStage;

    public Text deathScreen;

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
        public int numWaysDown;
        public int numSpecialRooms;
        public StageNode.StageType specialRoomType;

        public ShouldStillGenerate(int numGenericRooms, int numWaysDown, int numSpecialRooms, StageNode.StageType specialRoomType)
        {
            if (numGenericRooms < 0)
                numGenericRooms = 0;
            this.numGenericRooms = numGenericRooms;
            this.numWaysDown = numWaysDown;
            this.numSpecialRooms = numSpecialRooms;
            this.specialRoomType = specialRoomType;
        }

        public bool IsDone()
        {
            return numGenericRooms <= 0 && numWaysDown <= 0 && numSpecialRooms <= 0;
        }

        public void AddMadeNode(RoomNode newNode)
        {
            var room = newNode.roomObject.GetComponent<Room>();
            var isWayDown = room.wayDown != null;
            if (isWayDown)
            {
                Assert.IsTrue(numWaysDown > 0);
                numWaysDown--;
            } else if (room.specialRoom)
            {
                Assert.IsTrue(numSpecialRooms > 0);
                Assert.AreEqual(room.specialRoomType, specialRoomType);
                numSpecialRooms--;
            }
            else
            {
                numGenericRooms = Math.Max(0, numGenericRooms - 1);
            }
        }

        public override string ToString()
        {
            return "ShouldStillGenerate(numGenericRooms=" + numGenericRooms + ", numWaysDown=" + numWaysDown +
                   ", numSpecialRooms=" + numSpecialRooms + " of type " + specialRoomType + ")";
        }
    }

    private RoomNode GetRoomWithMaxDoors(RoomSide side, int maxIncomingDoors, ShouldStillGenerate shouldStillGenerate,
        int madeRoomRadius)
    {
        Assert.IsTrue(maxIncomingDoors >= 1);
        var minOutgoingDoors = shouldStillGenerate.IsDone() ? 0 : 1;
        // Debug.Log("for side" + side + "make max" + maxIncomingDoors + "," + minOutgoingDoors + "," + maxOutgoingDoors);
        var maxOutgoingGeneralDoors = shouldStillGenerate.numGenericRooms;
        List<GameObject> available = new List<GameObject>();
        foreach (GameObject roomPrefab in roomPrefabs)
        {
            var incomingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(side).Count;
            var room = roomPrefab.GetComponent<Room>();
            if (!(1 <= incomingDoorCount && incomingDoorCount <= maxIncomingDoors))
                continue;
            if (shouldStillGenerate.numGenericRooms == 0 && shouldStillGenerate.numWaysDown == 0 &&
                shouldStillGenerate.numSpecialRooms == 1 && room.specialRoomType == shouldStillGenerate.specialRoomType)
            {
                // special case: here, we can always finish, no matter how many outgoing doors there are.
                available.Add(roomPrefab);
                continue;
            }
            var outgoingDoorCount = roomPrefab.GetComponent<Room>().GetDoors(Room.OppositeSide(side)).Count;
            if (!(minOutgoingDoors <= outgoingDoorCount))
                continue;
            if (room.entryPoint != null)
                continue;
            var isWayDown = room.wayDown != null;
            if (shouldStillGenerate.numWaysDown > 0 && isWayDown)
            {
                available.Add(roomPrefab);
            } else if (shouldStillGenerate.numSpecialRooms > 0 &&
                       room.specialRoomType == shouldStillGenerate.specialRoomType)
            {
                available.Add(roomPrefab);
            }
            else if (!isWayDown && !room.specialRoom && outgoingDoorCount <= maxOutgoingGeneralDoors)
            {
                available.Add(roomPrefab);
            }
        }
        Assert.IsTrue(available.Count > 0, "Not all necessary rooms are available to make " + shouldStillGenerate);
        var chosenRoomPrefab = available[Random.Range(0, available.Count)];
        var madeNode = new RoomNode(this, chosenRoomPrefab, madeRoomRadius);
        shouldStillGenerate.AddMadeNode(madeNode);
        return madeNode;
    }

    private List<RoomNode> GenerateNextRooms(RoomSide side, List<RoomNode> currentRooms,
        ShouldStillGenerate shouldStillGenerate, int radius)
    {
        var oppositeSide = Room.OppositeSide(side);
        var currRoomNum = 0;
        var doorsLeft = CountDoorsAtSide(side, currentRooms);
        var nextRooms = new List<RoomNode>();
        while (doorsLeft > 0)
        {
            // find a room with <= rooms at opposite of side.
            RoomNode nextNode = GetRoomWithMaxDoors(
                oppositeSide, doorsLeft, shouldStillGenerate, radius);
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
        Assert.AreEqual(stageTypeFrequencies.Length, StageNode.NUM_STAGE_TYPES);
        waitingForDeathClick = false;
        deathScreen.enabled = false;

        currentStage = currentLevelStartStage;
        currentLevel = 0;
        myCurrentStageText = currentStageText.GetComponent<Text>();
        GenerateMap();
    }

    public void GenerateMap()
    {
        myCurrentStageText.text = "LD" + ((levelCount - currentLevel) * levelStageDepth - currentStage.stageDepth);
        foreach (RoomNode node in roomNodes)
        {
            Destroy(node.roomObject);
        }
        roomCount = 0;
        roomNodes.Clear();
        var isInitialStage = (currentLevel == 0 && currentStage.stageDepth == 0);
        var actualStartRoomPrefab = !isInitialStage ? startRoomPrefab : initialLevelRoomPrefab;
        var startNode = new RoomNode(this, actualStartRoomPrefab, 0);
        List<RoomNode> leftmostRooms = new List<RoomNode>();
        List<RoomNode> rightmostRooms = new List<RoomNode>();
        leftmostRooms.Add(startNode);
        rightmostRooms.Add(startNode);
        roomNodes.Add(startNode);
        var numGenericRooms = levelTargetNumGenericRoomsPerStage[currentLevel];
        var needSpecialRooms = currentStage.type != StageNode.StageType.Normal;
        var shouldStillGenerate = new ShouldStillGenerate(numGenericRooms, currentStage.GetNumWaysDown(), 
            needSpecialRooms ? 1 : 0, currentStage.type);
        if (isInitialStage)
            shouldStillGenerate = new ShouldStillGenerate(0, 0, 0, StageNode.StageType.Normal);
        int radius = 1;
        while (CountDoorsAtSide(RoomSide.Left, leftmostRooms) + CountDoorsAtSide(RoomSide.Left, rightmostRooms) > 0)
        {
            if (radius >= shouldStillGenerate.numGenericRooms + 10)
                Assert.IsTrue(false);

            if (Random.Range(0, 2) == 0)
            {
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate, -radius);
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate, radius);
            }
            else
            {
                rightmostRooms = GenerateNextRooms(RoomSide.Right, rightmostRooms, shouldStillGenerate, radius);
                leftmostRooms = GenerateNextRooms(RoomSide.Left, leftmostRooms, shouldStillGenerate, -radius);
            }
            roomNodes.AddRange(leftmostRooms);
            roomNodes.AddRange(rightmostRooms);

            radius++;
        }
        Assert.AreEqual(roomCount, roomNodes.Count);
        foreach (RoomNode node in roomNodes)
        {
            node.SetupInstance();
        }

        var sortedRoomNodes = roomNodes.OrderBy(node => node.horizontalPos);
        var wayDownNum = 0;
        foreach (var node in sortedRoomNodes)
        {
            if (node.roomObject.GetComponent<Room>().wayDown != null)
            {
                Debug.Log("will make room" + node.horizontalPos + "the wayDownNum=" + wayDownNum);
                node.wayDownNum = wayDownNum;
                wayDownNum++;
            }
        }

        var entryPos = startNode.roomObject.GetComponent<Room>().GetEntryPos();
        player.transform.position = new Vector3(entryPos.x, entryPos.y, player.transform.position.z);
        myPlayer.currentRoomNode = startNode;
        minimap.RerenderMap();
    }

    private static int RandomTowardsTarget(int old, int target, int maxDiff, int noise=1)
    {
        int center = (int) Mathf.MoveTowards(old, target, Random.Range(maxDiff - noise, maxDiff + noise));
        return Random.Range(center - noise, center + 2);
    }

    private StageNode GenerateLevelStartStage()
    {
        int totalDepth = levelStageDepth;
        int targetWidth = levelTargetStageWidths[currentLevel];
        int maxWidthDiff = 2;
        Debug.Log("Making stages:");
        List<StageNode> prevStages = new List<StageNode>();
        StageNode start = new StageNode(this, 0, 0, firstStageType, 0, 0);
        prevStages.Add(start);
        for (var depth = 1; depth < totalDepth; depth++)
        {
            Debug.Log("Depth=" + depth);
            int prevWidth = prevStages.Count;
            int newWidth = Math.Max(RandomTowardsTarget(prevWidth, targetWidth, maxWidthDiff), 2);
            if (currentLevel == 0 && depth == 1)
                newWidth = 1;  // initial room only has one way down.
            Debug.Log("Will make" + newWidth + " new");
            List<StageNode> newStages = new List<StageNode>();
            int connectedPrevUntil = 0;
            for (var newStageNum = 0; newStageNum < newWidth; newStageNum++)
            {
                // if we have an equal mapping, every prev has to point to newWidth / prevWidth.
                int connectToLeft = prevWidth - connectedPrevUntil - 1;
                Debug.Log("Make new " + newStageNum +", with connectToLeft=" + connectToLeft);
                bool connectAgain = Random.Range(0, 2) == 0;
                connectAgain |= newStageNum == 0;  // have to connect to first.
                connectAgain |= connectToLeft == 0;  // have to connect if nothing else left
                int willConnectTo = (int) ((connectToLeft * 1f / (newWidth - newStageNum)));
                willConnectTo = Random.Range(willConnectTo - 1, willConnectTo + 1);  // add some noise
                willConnectTo = Math.Min(Math.Max(willConnectTo, 1), connectToLeft);
                Debug.Log("Decided to connect again=" + connectAgain + " and to news=" + willConnectTo + " from the" + connectToLeft + " that are left");
                
                var connectToPrevStages = new List<StageNode>();
                var startAtOldStageNum = connectedPrevUntil + (connectAgain ? 0 : 1);
                var newConnectPrevUntil = connectedPrevUntil + willConnectTo;
                Debug.Log("Thus will connect to prev nodes from " + startAtOldStageNum + " to " + (connectedPrevUntil + 1 + willConnectTo));
                int riskSum = 0;
                int rewardSum = 0;
                for (var oldStageNum = startAtOldStageNum; oldStageNum <= newConnectPrevUntil; oldStageNum++)
                {
                    connectToPrevStages.Add(prevStages[oldStageNum]);
                    riskSum += prevStages[oldStageNum].pathRisk;
                    rewardSum += prevStages[oldStageNum].pathReward;
                }
                
                int risk = riskSum / connectToPrevStages.Count;
                int reward = rewardSum / connectToPrevStages.Count;
                var nextStageType = MakeRandomStageType(risk, reward);
                var newStage = new StageNode(this, depth, newStageNum, nextStageType,
                    risk + GetTypeRisk(nextStageType), reward + GetTypeReward(nextStageType));
                foreach (var oldStage in connectToPrevStages)
                    oldStage.nextStages.Add(newStage);
                newStages.Add(newStage);
                connectedPrevUntil = newConnectPrevUntil;
            }
            prevStages = newStages;
        }

        Debug.Log("start stage is:");
        Debug.Log(start);
        return start;
    }

    private int GetTypeRisk(StageNode.StageType nextStageType)
    {
        switch (nextStageType)
        {
            case StageNode.StageType.Challenge:
                return 2;
            case StageNode.StageType.Treasure:
                return 1;
            default:
                return 0;
        }
    }

    private int GetTypeReward(StageNode.StageType nextStageType)
    {
        switch (nextStageType)
        {
            case StageNode.StageType.Treasure:
                return 2;
            default:
                return 0;
        }
    }

    private StageNode.StageType MakeRandomStageType(int risk, int reward)
    {
        // todo: this ignores risk, reward for now.
        float valTotal = 0;
        foreach (var freq in stageTypeFrequencies)
        {
            valTotal += freq;
        }

        float random = Random.Range(0, valTotal);
        for (var num = 0; num < StageNode.NUM_STAGE_TYPES; num++)
        {
            random -= stageTypeFrequencies[num];
            if (random <= 0)
                return StageNode.GetStageTypeFromNum(num);
        }

        return StageNode.GetStageTypeFromNum(StageNode.NUM_STAGE_TYPES - 1);
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
            print("moving into next stage, going through way down num " + GetCurrentRoom().wayDownNum);
            currentStage = currentStage.nextStages[GetCurrentRoom().wayDownNum];
        }    
        GenerateMap();
    }

    public RoomNode GetCurrentRoom()
    {
        return myPlayer.currentRoomNode;
    }

    public void ReloadMinimap()
	{
        minimap.RerenderMap();
	}

    public void OnPlayerDie()
    {
        myPlayer.dead = true;
        myPlayer.Die();
        deathScreen.enabled = true;
        waitingForDeathClick = true;
    }

    public void Update()
    {
        if (!waitingForDeathClick)
            return;
        if (!Input.GetButtonDown("Fire1"))
            return;
        deathScreen.enabled = false;
        waitingForDeathClick = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
