using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public int maxLifetime = 500;
    public int damage = 1;
    public bool active = false;
    private float lifetimeLeft;
    public bool autoDestroy = false;
    public float knockback = 0f;
    public float knockbackDuration = 0f;
    public float constantVelocity = 0f;
    public Vector2 attackAngle = Vector2.right;
    private RoomSide attackDir;
    public Vector2 knockbackDirection = Vector2.zero;
    
    void Start()
    {
        lifetimeLeft = (float)maxLifetime;
    }

    void Update()
    {
        if (!active)
            return;
		if (autoDestroy)
        {
            lifetimeLeft -= Time.deltaTime * 100;
            if (lifetimeLeft <= 0)
            {
                Debug.Log("Attack destroyed!");
                Destroy(gameObject);
            }
        }   
    }

    public void StartAttack(RoomSide attackDir)
    {
        this.attackDir = attackDir;
        int attackDirInt = attackDir == RoomSide.Right ? 1 : -1;
        Rigidbody2D myBody = GetComponent<Rigidbody2D>();
        myBody.velocity = attackDirInt * constantVelocity * attackAngle.normalized;
        var myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer != null)
            myRenderer.flipX = attackDir == RoomSide.Left;
    }

    public void StartAttack(RoomSide attackDir, Vector2 knockbackDir)
    {
        this.attackDir = attackDir;
        this.knockbackDirection = knockbackDir;
        int attackDirInt = attackDir == RoomSide.Right ? 1 : -1;
        Rigidbody2D myBody = GetComponent<Rigidbody2D>();
        myBody.velocity = attackDirInt * constantVelocity * attackAngle.normalized;
        var myRenderer = GetComponent<SpriteRenderer>();
        if(myRenderer != null)
            myRenderer.flipX = attackDir == RoomSide.Left;
    }
}
