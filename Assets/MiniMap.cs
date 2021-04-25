using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{

    public Bounds bound;
    public Sprite[] icons;
    public GameObject myLineRenderer;
    public GameObject specialLineRenderer;
    public GameObject myIcon;
    public GameObject playerIcon;
    public float spacing = 1f;
    public float thickness = 1f;
    public GameObject uiElem;

    private List<GameObject> activeLines = new List<GameObject>();
    private List<GameObject> activeIcons = new List<GameObject>();

	private void Update()
	{
        Image im = (Image)uiElem.GetComponentInChildren(typeof(Image), true);
        RawImage rim = (RawImage)uiElem.GetComponentInChildren(typeof(RawImage), true);
        im.enabled = Input.GetButton("Tab");
        rim.enabled = Input.GetButton("Tab");
    }

	private bool SameRoom(StageNode a, StageNode b)
	{
        return a.horizontalNum == b.horizontalNum && a.stageDepth == b.stageDepth;
	}

    private int RoomDepth(StageNode room, int d)
	{
        if(room.nextStages.Count == 0)
		{
            return d;
		}
        int maxd = d;
        for(int i = 0; i < room.nextStages.Count; i++)
		{
            maxd = System.Math.Max(maxd, RoomDepth(room.nextStages[i], d + 1));
		}
        return maxd;
	}

    public void CreateMap(RoomManager myManager)
	{
        ClearMap();
        StageNode startNode = myManager.currentLevelStartStage;
        int depthLayers = RoomDepth(startNode, 1);

        // create depth linspace
        List<float> linspaceD = new List<float>();
        for (int i = 0; i < (int)depthLayers; i++)
        {
            linspaceD.Add(bound.max.y + (bound.min.y - bound.max.y) * (float)i / (float)((int)depthLayers));
        }

        List<float> lastLinspace = new List<float>();
        Dictionary<int, List<int>> nodeToParents = new Dictionary<int, List<int>>();

        List<StageNode> currLayer = new List<StageNode>{ startNode };
        while(currLayer.Count != 0)
		{
            // Draw the Rooms from the previous layer ...
            List<float> currLayerDrawPosX = new List<float>();
            float offset = Mathf.Min(spacing, (float)(bound.max.x - bound.min.x) / currLayer.Count);
            float middle = bound.center.x - offset * currLayer.Count / 2f;
            for (int horizontalPos = 0; horizontalPos < currLayer.Count; horizontalPos++)
            {
                currLayerDrawPosX.Add(middle + offset * (float)horizontalPos);
            }

            Debug.Log("make layer!");
            for (int horizontalPos = 0; horizontalPos < currLayer.Count; horizontalPos++)
			{
				Debug.Log("has elemenet" + horizontalPos + ":" + currLayer[horizontalPos] + " which has " + currLayer[horizontalPos].type + StageNode.GetStageTypeNum(currLayer[horizontalPos].type));
                DrawIcon(currLayer[horizontalPos].type, new Vector2(currLayerDrawPosX[horizontalPos], linspaceD[currLayer[horizontalPos].stageDepth]));
                if (SameRoom(currLayer[horizontalPos], myManager.currentStage))
				{
                    DrawPlayer(new Vector2(currLayerDrawPosX[horizontalPos], linspaceD[currLayer[horizontalPos].stageDepth]));
				}
			}

            // Draw the lines
            foreach(StageNode node in currLayer)
			{
                if (node.stageDepth > 0)
                {
                    var parents = nodeToParents[node.horizontalNum];
                    foreach (int parent in parents)
                    {
                        Vector2 p0 = new Vector2(lastLinspace[parent], linspaceD[node.stageDepth - 1]);
                        Vector2 p1 = new Vector2(currLayerDrawPosX[node.horizontalNum], linspaceD[node.stageDepth]);
                        Color col = new Color(194, 194, 209);
                        int wayDownNum = myManager.GetCurrentRoom().wayDownNum;

                        if (myManager.currentStage.stageDepth == node.stageDepth - 1 && myManager.currentStage.horizontalNum == parent && wayDownNum >= 0 && node.horizontalNum == myManager.currentStage.nextStages[wayDownNum].horizontalNum)
						{
                            DrawLine(p0, p1, true, thickness);
						}
						else
						{
                            DrawLine(p0, p1, false, thickness);
                        }
                    }
                }
			}
            // Prepare next stage
            nodeToParents.Clear();
            List<StageNode> tmp = new List<StageNode>();
            foreach (StageNode node in currLayer)
			{
                foreach (StageNode child in node.nextStages)
                {
					if (!nodeToParents.ContainsKey(child.horizontalNum))
					{
                        nodeToParents.Add(child.horizontalNum, new List<int>());
					}
                    nodeToParents[child.horizontalNum].Add(node.horizontalNum);
                    if (tmp.Count == 0 || !SameRoom(tmp[tmp.Count - 1], child))
                    {
                        tmp.Add(child);
                    }
                }
            }
            lastLinspace = currLayerDrawPosX;
            currLayer = tmp;
        }

	}

    private void DrawIcon(StageNode.StageType type, Vector2 pos)
	{
        GameObject newIcon = Instantiate(myIcon, transform);
        activeIcons.Add(newIcon);
        newIcon.GetComponent<SpriteRenderer>().sprite = icons[StageNode.GetStageTypeNum(type)];
        newIcon.transform.localPosition = new Vector3(pos.x, pos.y, newIcon.transform.position.z);
	}

    private void DrawLine(Vector2 p0, Vector2 p1, bool special = false, float thickness = 1f)
	{
        GameObject newLine = special ? Instantiate(specialLineRenderer, transform) : Instantiate(myLineRenderer, transform);
        activeLines.Add(newLine);

        LineRenderer render = newLine.GetComponent<LineRenderer>();

        render.widthCurve = new AnimationCurve(
             new Keyframe(0, thickness)
             , new Keyframe(1, thickness));

        render.SetPositions(new Vector3[] {
              new Vector3(p0.x, p0.y, 0f)
              , new Vector3(p1.x, p1.y, 0f) });
    }

    private void DrawPlayer(Vector2 pos)
	{
        playerIcon.transform.localPosition = new Vector3(pos.x, pos.y, playerIcon.transform.position.z);
    }

    private void ClearMap()
	{
        foreach (var obj in activeLines)
		{
            Destroy(obj);
		}
        foreach (var obj in activeIcons)
        {
            Destroy(obj);
        }
        activeLines.Clear();
        activeIcons.Clear();
    }
}
