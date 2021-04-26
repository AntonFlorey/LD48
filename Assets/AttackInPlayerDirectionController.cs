using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ChargeMovementController : MonoBehaviour
{
    public RoomSide walkingDirection = RoomSide.Left;
    public float velocity = 1;
    private Room myRoom;
    private Rigidbody2D myBody;
    private SpriteRenderer mySpriteRenderer;
    private HealthComponent myHealth;
    public int maxAttackCooldown;
    private int attackCooldown;
    public AttackController attackPrefab;
    public float turnRatio = 0.2f;

    private void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        Physics2D.queriesStartInColliders = false;
        myBody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.flipX = (walkingDirection == RoomSide.Right);
        myHealth = GetComponent<HealthComponent>();
        attackCooldown = maxAttackCooldown;
    }

    void FixedUpdate()
    {
        if (!myRoom.IsActive())
            return;
        if (myHealth.knockedBack)
            return;
        if (attackCooldown <= maxAttackCooldown * turnRatio)
            walkingDirection = (myRoom.roomNode.manager.player.transform.position.x >= transform.position.x) ? RoomSide.Right : RoomSide.Left;
        attackCooldown -= 1;
        if (attackCooldown <= 0)
        {
            attackCooldown = maxAttackCooldown;
            var attack = Instantiate(attackPrefab, transform.parent);
            var attackController = attack.GetComponent<AttackController>();
            attack.transform.position = transform.position + Room.RoomSideToVec(walkingDirection) * (transform.localScale.x / 2f);
            attackController.active = true;
            attackController.StartAttack(walkingDirection);
        }
    }
}
