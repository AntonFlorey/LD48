using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public List<GameObject> leftDoors;
    public List<GameObject> rightDoors;
    public RoomNode roomNode;
    public GameObject tiles;
    public GameObject groundTiles;
    public bool WayDown = false;
    private Tilemap tileMap;
    private Tilemap groundTileMap;

    public GameObject GetDoor(RoomSide side, int doorNum)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftDoors[doorNum];
            case RoomSide.Right:
                return rightDoors[doorNum];
            default:
                return null;
        }
    }

    public List<GameObject> GetDoors(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftDoors;
            case RoomSide.Right:
                return rightDoors;
            default:
                Assert.IsTrue(false);
                return null;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (groundTiles == null)
        {
            groundTiles = Instantiate(tiles, tiles.transform);
            var otherTileMap = tiles.GetComponent<Tilemap>();
            var groundTileMap = groundTiles.GetComponent<Tilemap>();
            var size = groundTileMap.size;
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y - 1; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (!groundTileMap.HasTile(pos))
                        continue;
                    if (groundTileMap.HasTile(new Vector3Int(x, y + 1, 0)))
                    {
                        groundTileMap.SetTile(pos, null);
                    }
                    else
                    {
                        otherTileMap.SetTile(pos, null);
                    }
                }
            }

            groundTiles.tag = "ground";
            var oldPos = groundTiles.transform.position;
            groundTiles.transform.position = new Vector3(oldPos.x, oldPos.y, 10);
        }
        tileMap = tiles.GetComponent<Tilemap>();
        groundTileMap = groundTiles.GetComponent<Tilemap>();
    }

    public Vector3Int GetTilePos(Vector3 pos)
    {
        // maybe broken, idk?
        pos -= tileMap.transform.position;
        var size = tileMap.cellSize;
        return new Vector3Int((int) (pos.x / size.x), (int) (pos.y / size.y), 0);
    }

    public static RoomSide OppositeSide(RoomSide s)
    {
        switch (s)
        {
            case RoomSide.Left: return RoomSide.Right;
            case RoomSide.Right: return RoomSide.Left;
            default:
                Assert.IsTrue(false);
                return RoomSide.Left;  // return any
        }
    }

    public static Vector3 RoomSideToVec(RoomSide s)
    {
        switch (s)
        {
            case RoomSide.Left: return new Vector3(1, 0, 0);
            case RoomSide.Right: return new Vector3(-1, 0, 0);
            default:
                Assert.IsTrue(false);
                return new Vector3(0, 0, 0);  // return any
        }
    }
}
