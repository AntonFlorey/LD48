using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public class Room : MonoBehaviour
{
    public List<GameObject> leftDoors;
    public List<GameObject> rightDoors;

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
        
    }

    void Update()
    {
        
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
}
