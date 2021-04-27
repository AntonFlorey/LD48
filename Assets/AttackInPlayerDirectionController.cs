using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AttackInPlayerDirectionController : MonoBehaviour
{
    public RoomSide movementDirection = RoomSide.Left;
    private Room myRoom;
    private Rigidbody2D myBody;
    private SpriteRenderer mySpriteRenderer;
    private HealthComponent myHealth;
    public int maxAttackCooldown = 500;
    private int attackCooldown;
    public AttackController attackPrefab;
    public float turnRatio = 0.2f;
    public float chargeSpeed = 1;
    public int maxChargeDurationTime;
    private int currentChargeTime;

    private void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        Physics2D.queriesStartInColliders = false;
        myBody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.flipX = (movementDirection == RoomSide.Right);
        myHealth = GetComponent<HealthComponent>();
        attackCooldown = maxAttackCooldown;
    }

    void FixedUpdate()
    {
        if (!myRoom.IsActive())
            return;
        if (myHealth.knockedBack)
            return;
        if (currentChargeTime > 0)
        {
            currentChargeTime -= 1;
            myBody.velocity = new Vector2(-chargeSpeed * Room.RoomSideToVec(movementDirection).x, myBody.velocity.y);
            return;
        }
        myBody.velocity = new Vector2(0, myBody.velocity.y);
        if (attackCooldown >= maxAttackCooldown * turnRatio)
        {
            movementDirection = (myRoom.roomNode.manager.player.transform.position.x >= transform.position.x)
                ? RoomSide.Right
                : RoomSide.Left;
            mySpriteRenderer.flipX = (movementDirection == RoomSide.Right);
        }

        attackCooldown -= 1;
        if (attackCooldown <= 0)
        {
            attackCooldown = maxAttackCooldown;
            currentChargeTime = maxChargeDurationTime;
            var attack = Instantiate(attackPrefab, transform.parent);
            var attackController = attack.GetComponent<AttackController>();
            attack.transform.position = transform.position + Room.RoomSideToVec(movementDirection) * (transform.localScale.x / 2f);
            attackController.active = true;
            attackController.StartAttack(movementDirection, myBody.velocity);
        }
    }
}