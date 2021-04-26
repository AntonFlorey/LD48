using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PickupableItem : MonoBehaviour
{
    public GameObject moveTowardsObject = null;
    public float maxMoveTowardsDistance = 20f;
    public float moveTowardsForce = 10f;
    public int healAmount = 0;
    private Rigidbody2D myBody;

    public void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        if (moveTowardsObject == null)
            return;
        var diff = moveTowardsObject.transform.position - transform.position;
        Debug.DrawRay(transform.position, diff, Color.red, 2);
        if (diff.sqrMagnitude <= maxMoveTowardsDistance * maxMoveTowardsDistance)
        {
            myBody.AddForce(diff.normalized * moveTowardsForce);
        }
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
