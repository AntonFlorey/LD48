using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.UI;
using UnityEngine;

public class CrawlMovementController : MonoBehaviour
{
    public RoomSide walkingDirection = RoomSide.Left;
    public bool walksOffEdges = false;
    public float velocity = 1;
    private Room myRoom;
    private Rigidbody2D myBody;
    private SpriteRenderer mySpriteRenderer;

    private void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        Physics2D.queriesStartInColliders = false;
        myBody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.flipX = (walkingDirection == RoomSide.Right);
    }

    void FixedUpdate()
    {
        if (!myRoom.IsActive())
            return;
        float epsilon = 0.1f;
        var radius = transform.localScale / 2;
        var obstacleInFront = Physics2D.Raycast(
            transform.position, Room.RoomSideToVec(walkingDirection),
            radius.x + epsilon).collider != null;
        var groundInFront = Physics2D.Raycast(
            transform.position + Room.RoomSideToVec(walkingDirection) * (radius.x + epsilon),
            new Vector2(0, -1), radius.y + epsilon).collider != null;
        // Debug.DrawRay(transform.position + Room.RoomSideToVec(walkingDirection) * (radius.x + epsilon), new Vector2(0, -1));
        // Debug.Log("obstacle:" + obstacleInFront + ";ground:" + groundInFront);
        if (obstacleInFront || (!walksOffEdges && !groundInFront))
        {
            // turn, do not update pos
            walkingDirection = Room.OppositeSide(walkingDirection);
            mySpriteRenderer.flipX = (walkingDirection == RoomSide.Right);
            return;
        }
        myBody.velocity = new Vector2(velocity * Room.RoomSideToVec(walkingDirection).x, myBody.velocity.y);
    }
}
