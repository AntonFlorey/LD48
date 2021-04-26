using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class PickupableItem : MonoBehaviour
{
    public GameObject moveTowardsObject = null;
    public float maxMoveTowardsDistance = 20f;
    public float moveTowardsForce = 10f;
    public int healAmount = 0;
    public bool givesItem = false;
    public InventoryItem itemType = InventoryItem.Ignore;
    
    private Rigidbody2D myBody;

    public void Start()
    {
        Assert.IsTrue(givesItem == (itemType != InventoryItem.Ignore));
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

        if (givesItem)
        {
            player.GetComponent<InventoryComponent>().AddItemEffect(itemType);
        }
        Destroy(gameObject);
    }
}
