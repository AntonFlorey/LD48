using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class FlyToTargetController : MonoBehaviour
{
    public GameObject moveTowardsObject = null;  // by default to the player
    public RoomSide movementDirection = RoomSide.Left;
    public float velocity = 1;
    public float viewDistance = 20f;
    public float movementForce = 10f;
    private Room myRoom;
    private Rigidbody2D myBody;
    private SpriteRenderer mySpriteRenderer;
    private HealthComponent myHealth;

    private void Start()
    {
            
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        if (moveTowardsObject == null)
            moveTowardsObject = myRoom.roomNode.manager.player;
        Physics2D.queriesStartInColliders = false;
        myBody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.flipX = (movementDirection == RoomSide.Right);
        myHealth = GetComponent<HealthComponent>();
    }

    void FixedUpdate()
    {
        if (!myRoom.IsActive())
            return;
        if (myHealth.knockedBack)
            return;
        var diff = moveTowardsObject.transform.position - transform.position;
        Debug.DrawRay(transform.position, diff, Color.red, 2);
        if (diff.sqrMagnitude <= viewDistance * viewDistance)
        {
            myBody.AddForce(diff.normalized * movementForce);
        }
    }
}
