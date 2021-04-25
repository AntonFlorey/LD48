using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{

    public Bounds bound;
    public Sprite[] icons;
    public GameObject myLineRenderer;
    public GameObject myIcon;
    public GameObject playerIcon;
    public float spacing = 1f;

    private List<GameObject> activeLines = new List<GameObject>();
    private List<GameObject> activeIcons = new List<GameObject>();

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
        Debug.Log("Generating the minimap...");
        StageNode startNode = myManager.currentLevelStartStage;
        int depthLayers = RoomDepth(startNode, 1);

        // create depth linspace
        List<float> linspaceD = new List<float>();
        for (int i = 0; i < (int)depthLayers; i++)
        {
            linspaceD.Add(bound.max.y + (bound.min.y - bound.max.y) * (float)i / (float)((int)depthLayers));
        }

        List<StageNode> currLayer = new List<StageNode>{ startNode };
        while(currLayer.Count != 0)
		{
            // Draw the Rooms from the previous layer ...
            // create vert linspace
            List<float> linspaceV = new List<float>();
            float offset = Mathf.Min(spacing, (float)(bound.max.x - bound.min.x) / currLayer.Count);
            float middle = bound.center.x - offset * currLayer.Count / 2f;
            for (int i = 0; i < currLayer.Count; i++)
            {
                linspaceV.Add(middle + offset * (float)i);
            }

            for (int i = 0; i < currLayer.Count; i++)
			{
                DrawIcon((int)currLayer[i].type, new Vector2(linspaceV[i], linspaceD[currLayer[i].stageDepth]));
                Debug.Log("Minimap icon added...");
                if (SameRoom(currLayer[i], myManager.currentStage))
				{
                    DrawPlayer(new Vector2(linspaceV[i], linspaceD[currLayer[i].stageDepth]));
				}
			}
            List<StageNode> tmp = new List<StageNode>();
            foreach(StageNode node in currLayer)
			{
                foreach(StageNode child in node.nextStages)
				{
                    if(tmp.Count == 0 || !SameRoom(tmp[tmp.Count - 1], child))
					{
                        tmp.Add(child);
					}
				}
			}
            currLayer = tmp;
        }

	}

    private void DrawIcon(int id, Vector2 pos)
	{
        GameObject newIcon = Instantiate(myIcon, transform);
        activeIcons.Add(newIcon);
        myIcon.GetComponent<SpriteRenderer>().sprite = icons[id];
        newIcon.transform.localPosition = new Vector3(pos.x, pos.y, newIcon.transform.position.z);
	}

    private void DrawLine(Vector2 p0, Vector2 p1, Color col, float thickness = 1f)
	{
        GameObject newLine = Instantiate(myLineRenderer);
        activeLines.Add(newLine);

        LineRenderer render = newLine.GetComponent<LineRenderer>();

        render.startColor = col;
        render.endColor = col;

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
