using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class StageNode
{
    public enum StageType
    {
        Normal = 0,
        Treasure = 1,
        Puzzle = 2,
        Challenge = 3,
        Heal = 4
    }

    public readonly RoomManager manager;
    public readonly StageType type;
    public readonly int pathRisk;
    public readonly int pathReward;
    public readonly List<StageNode> nextStages = new List<StageNode>();
    public readonly int stageDepth;
    public readonly int horizontalNum;

    public StageNode(RoomManager manager, int stageDepth, int horizontalNum, StageType type, int pathRisk, int pathReward)
    {
        this.manager = manager;
        this.stageDepth = stageDepth;
        this.horizontalNum = horizontalNum;
        this.type = type;
        this.pathRisk = pathRisk;
        this.pathReward = pathReward;
    }

    public override string ToString()
    {
        return "StageNode(d" + stageDepth + "p" + horizontalNum + " " + type + " " + string.Join(" ", nextStages.ConvertAll<string>(a => a.ToString()).ToArray()) + ")";
    }

    public int GetNumWaysDown()
    {
        return Math.Max(nextStages.Count, 1);  // at least one, one for the boss!
    }
}