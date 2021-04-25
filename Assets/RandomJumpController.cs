using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.UI;
using UnityEngine;

public class RandomJumpController : MonoBehaviour
{
    private Room myRoom;
    public int maxTime = 10;
    public int jumpForce;
    public int currentTime;
    private Rigidbody2D myBody;

    private void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        myBody = GetComponent<Rigidbody2D>();
        Physics2D.queriesStartInColliders = false;
        currentTime = maxTime;
    }

    void FixedUpdate()
    {
        if (!myRoom.IsActive())
            return;
        float epsilon = 0.1f;
        var radius = transform.localScale / 2;
        var obstacleAbove = Physics2D.Raycast(
            transform.position, new Vector2(0, 1),
            radius.y + epsilon).collider != null;
        if (obstacleAbove)
            return;
        currentTime -= 1;
        if (currentTime <= 0)
        {
            // flap!
            myBody.AddForce(new Vector2(0, jumpForce));
        }
    }
}
