using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PickupableItem : MonoBehaviour
{
    public GameObject moveTowardsObject = null;
    public float maxMoveTowardsDistance = 50f;
    public float moveTowardsForce = 10f;
    public int healAmount = 0;
    private Rigidbody2D myBody;

    public void Start()
    {
        if (moveTowardsObject != null)
        {
            myBody = GetComponent<Rigidbody2D>();
        }
    }

    public void FixedUpdate()
    {
        if (moveTowardsObject == null)
            return;
        var diff = moveTowardsObject.transform.position - transform.position;
        if (diff.sqrMagnitude <= maxMoveTowardsDistance * maxMoveTowardsDistance)
            myBody.velocity = new Vector2(diff.normalized.x * moveTowardsForce, myBody.velocity.y);
        else
            myBody.velocity = new Vector2(0, myBody.velocity.y);
    }

    public void OnPickup(GameObject player)
    {
        if (healAmount > 0)
        {
            player.GetComponent<HealthComponent>().TakeDamage(-healAmount);
        }
        Destroy(gameObject);
    }
}
