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
    public float maxAttackCooldown = 5;
    private int attackCooldown;
    public AttackController attackPrefab;
    public float turnRatio = 0.2f;
    public float chargeSpeed = 1;
    public float maxChargeDurationTime;
    private float currentChargeTime;
    [SerializeField] private float range = 10;
    private float currWaitTime;
    private Transform target;
    private CapsuleCollider2D myCollider;
    [SerializeField] private GameObject attackCollider;

    private void Start()
    {
        myRoom = transform.parent.gameObject.GetComponent<Room>();
        Physics2D.queriesStartInColliders = false;
        myBody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.flipX = (movementDirection == RoomSide.Right);
        myHealth = GetComponent<HealthComponent>();
        myCollider = GetComponentsInChildren<CapsuleCollider2D>()[1];
        currWaitTime = maxAttackCooldown;
        currentChargeTime = 0f;
    }

	private void Update()
	{
        if (!myRoom.IsActive())
            return;
        if (myHealth.knockedBack)
		{
            //currWaitTime = maxAttackCooldown;
            return;
        }
        currWaitTime = Mathf.Max(0f, currWaitTime - Time.deltaTime);
        currentChargeTime = Mathf.Max(0f, currentChargeTime - Time.deltaTime);
        target = myRoom.roomNode.manager.player.transform;
        if(currentChargeTime == 0)
		{
            myBody.velocity = new Vector2(0f, myBody.velocity.y);
            //myCollider.enabled = true;
            attackCollider.GetComponent<PolygonCollider2D>().enabled = false;
        }

        // check if waiting
        if (currWaitTime <= 0)
		{
            // Start the attack
            if((target.position - transform.position).magnitude <= range)
			{
                // calculate the attack direction
                Vector2 lookDirection = movementDirection == RoomSide.Left ? Vector2.left : Vector2.right;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
                Vector2 direction = lookDirection - Vector2.Dot(hit.normal, lookDirection) * hit.normal;
                // Add an impulse to create the dash movement
                myBody.AddForce(direction.normalized * chargeSpeed, ForceMode2D.Impulse);
                currentChargeTime = maxChargeDurationTime;
                // Spawn in the attack
                //myCollider.enabled = false;
                attackCollider.GetComponent<PolygonCollider2D>().enabled = true;
                attackCollider.GetComponent<AttackController>().knockbackDirection = Room.RoomSideToVec(movementDirection);
                // collider rotation
                attackCollider.transform.rotation = movementDirection == RoomSide.Left ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
                // Reset attack cooldown
                currWaitTime = maxAttackCooldown;
            }
		}else if(currWaitTime <= maxAttackCooldown * (1 - turnRatio))
		{
            // Turn towards player
            movementDirection = (myRoom.roomNode.manager.player.transform.position.x >= transform.position.x)
                ? RoomSide.Right
                : RoomSide.Left;
            mySpriteRenderer.flipX = (movementDirection == RoomSide.Left);
        } 
    }

	//void FixedUpdate()
 //   {
 //       if (!myRoom.IsActive())
 //           return;
 //       if (myHealth.knockedBack)
 //           return;
 //       if (currentChargeTime > 0)
 //       {
 //           currentChargeTime -= 1;
 //           myBody.velocity = new Vector2(-chargeSpeed * Room.RoomSideToVec(movementDirection).x, myBody.velocity.y);
 //           return;
 //       }
 //       myBody.velocity = new Vector2(0, myBody.velocity.y);
 //       if (attackCooldown >= maxAttackCooldown * turnRatio)
 //       {
 //           movementDirection = (myRoom.roomNode.manager.player.transform.position.x >= transform.position.x)
 //               ? RoomSide.Right
 //               : RoomSide.Left;
 //           mySpriteRenderer.flipX = (movementDirection == RoomSide.Right);
 //       }

 //       attackCooldown -= 1;
 //       if (attackCooldown <= 0)
 //       {
 //           attackCooldown = maxAttackCooldown;
 //           currentChargeTime = maxChargeDurationTime;
 //           var attack = Instantiate(attackPrefab, transform.parent);
 //           var attackController = attack.GetComponent<AttackController>();
 //           attack.transform.position = transform.position + Room.RoomSideToVec(movementDirection) * (transform.localScale.x / 2f);
 //           attackController.active = true;
 //           attackController.StartAttack(movementDirection, myBody.velocity);
 //       }
 //   }
}