using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<GameObject> roomPrefabs;
    public GameObject player;
    
    public void Start()
    {
        EnterRoom(roomPrefabs[0]);
    }

    private void EnterRoom(GameObject roomPrefab)
    {
        var roomTransform = roomPrefab.transform;
        var roomObject = Instantiate(roomPrefab, roomTransform.position, roomTransform.rotation);
        var room = roomObject.GetComponent<Room>();
        player.transform.position = room.transform.position + room.door.transform.position;   
    }
}
