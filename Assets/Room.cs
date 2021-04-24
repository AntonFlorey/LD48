using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject leftDoor;
    public GameObject rightDoor;

    public GameObject GetDoor(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Left:
                return leftDoor;
            case RoomSide.Right:
                return rightDoor;
            default:
                return null;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
