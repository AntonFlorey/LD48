using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> roomPrefabs;
    public GameObject player;
    
    public void Start()
    {
        EnterRoom(roomPrefabs[0], RoomSide.Right);
    }

    private void EnterRoom(GameObject roomPrefab, RoomSide side)
    {
        var roomTransform = roomPrefab.transform;
        var roomObject = Instantiate(roomPrefab, roomTransform.position, roomTransform.rotation);
        var room = roomObject.GetComponent<Room>();
        var door = room.GetDoor(side);
        player.transform.position = room.transform.position + door.transform.position;
    }
}
