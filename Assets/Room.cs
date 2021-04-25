using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public List<GameObject> leftDoors;
    public List<GameObject> rightDoors;
    internal RoomNode roomNode;
    public GameObject tiles;
    public GameObject groundTiles;
    public GameObject entryPoint = null;
    public GameObject wayDown = null;
    private Tilemap tileMap;
    private Tilemap groundTileMap;
    private List<GameObject> aliveEnemies;

    public Bounds getBounds()
	{
        Bounds tileBounds = tileMap.localBounds;
        return new Bounds(tileMap.transform.TransformPoint(tileBounds.center), tileBounds.size);
    }

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
            groundTiles = Instantiate(tiles, transform);
            groundTiles.name = tiles.name + "_Ground";
            var deepTileMap = tiles.GetComponent<Tilemap>();
            var groundTileMap = groundTiles.GetComponent<Tilemap>();
            var bounds = groundTileMap.cellBounds;
            Assert.AreEqual(bounds, deepTileMap.cellBounds);
            for (var x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (var y = bounds.yMin; y < bounds.yMax - 1; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (!groundTileMap.HasTile(pos))
                    {
                        Assert.IsFalse(groundTileMap.HasTile(pos));
                        continue;
                    }
                    if (groundTileMap.HasTile(new Vector3Int(x, y + 1, 0)))
                    {
                        groundTileMap.SetTile(pos, null);
                    }
                    else
                    {
                        deepTileMap.SetTile(pos, null);
                    }
                }
            }

            groundTiles.tag = "ground";
            var oldPos = groundTiles.transform.position;
            groundTiles.transform.position = new Vector3(oldPos.x, oldPos.y, 10);
        }
        tileMap = tiles.GetComponent<Tilemap>();
        groundTileMap = groundTiles.GetComponent<Tilemap>();

        aliveEnemies = new List<GameObject>();
        for (int enemyIdx = 0; enemyIdx < transform.childCount; enemyIdx++)
        {
            var enemyTransform = transform.GetChild(enemyIdx);
            if (enemyTransform.gameObject.CompareTag("enemy"))
            {
                aliveEnemies.Add(enemyTransform.gameObject);
            }
        }

        UpdateDoors();
    }

    private void UpdateDoors()
    {
        var open = IsCleared();
        foreach (var door in leftDoors)
        {
            door.GetComponent<BoxCollider2D>().isTrigger = open;
            if (open)
                Destroy(door.GetComponent<SpriteRenderer>());
        }
        foreach (var door in rightDoors)
        {
            door.GetComponent<BoxCollider2D>().isTrigger = open;
            if (open)
                Destroy(door.GetComponent<SpriteRenderer>());
        }
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

    public bool IsActive()
    {
        return roomNode.manager.GetCurrentRoom().roomObject.Equals(roomNode.roomObject);
    }

    public void MarkEnemyDeath(GameObject enemy)
    {
        aliveEnemies.Remove(enemy);
        UpdateDoors();
    }

    public bool IsCleared()
    {
        return aliveEnemies.Count == 0;
    }
}
