using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Bounds bound;
    public Sprite[] icons;
    public GameObject myLineRenderer;

    private GameObject[] activeLines;
    private GameObject[] activeIcons;


    // Start is called before the first frame update
    void Start()
    {
        CreateMap();
    }

    public void CreateMap()
	{

	}

    private void CreateIcon(int id, Vector2 pos)
	{

	}

    private void DrawLine(Vector2 p1, Vector2 p2, Color col, float thickness = 1f)
	{

	}

    private void DrawPlayer(Vector2 pos)
	{

	}
}
