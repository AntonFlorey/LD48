using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class CrawlMovementController : MonoBehaviour
{
    public RoomSide walkingDirection = RoomSide.Left;
    public bool walksOffEdges = false;
    public float velocity = 1;
    private Room room;
    private Rigidbody2D myBody;

    private void Start()
    {
        room = transform.parent.gameObject.GetComponent<Room>();
        Physics2D.queriesStartInColliders = false;
        myBody = this.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
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
            return;
        }
        myBody.velocity = new Vector2(velocity * Room.RoomSideToVec(walkingDirection).x, myBody.velocity.y);
    }
}
