using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearOnEnter : MonoBehaviour
{
    private Room myRoom;
    public float duration = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
    }

    void FixedUpdate()
    {
        if (myRoom.IsActive())
        {
            duration -= 1;
            if (duration <= 0)
                Destroy(gameObject);
        }
    }
}
