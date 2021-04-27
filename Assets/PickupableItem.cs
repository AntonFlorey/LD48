using System;
using UnityEngine;
using UnityEngine.Assertions;

public class PickupableItem : MonoBehaviour
{
    public GameObject moveTowardsObject = null;
    public float maxMoveTowardsDistance = 20f;
    public float moveTowardsForce = 10f;
    public int healAmount = 0;
    public bool givesItem = false;
    public InventoryItem itemType = InventoryItem.Ignore;
    public float floatFrequency = 0.5f;
    public float floatRadius = 0;
    public int spawnInAnimationDuration = 0;
    private Vector3 originPos;
    private int spawnInTimeLeft;
    
    private Rigidbody2D myBody;
    public void Start()
    {
        Assert.IsTrue(givesItem == (itemType != InventoryItem.Ignore));
        myBody = GetComponent<Rigidbody2D>();
        originPos = transform.localPosition;
        spawnInTimeLeft = spawnInAnimationDuration;
        if (spawnInAnimationDuration > 0)
            transform.localScale = Vector3.zero;
    }

    public void FixedUpdate()
    {
        if (spawnInAnimationDuration > 0)
        {
            spawnInTimeLeft = Math.Max(0, spawnInTimeLeft-1);
            transform.localScale = (1 - ((float) spawnInTimeLeft) / spawnInAnimationDuration) * Vector3.one;
        }

        if (moveTowardsObject != null)
        {
            var diff = moveTowardsObject.transform.position - transform.position;
            Debug.DrawRay(transform.position, diff, Color.red, 2);
            if (diff.sqrMagnitude <= maxMoveTowardsDistance * maxMoveTowardsDistance)
            {
                myBody.AddForce(diff.normalized * moveTowardsForce);
            }
        }

        if (floatRadius > 0)
        {
            transform.localPosition = originPos + floatRadius * Mathf.Sin(floatFrequency * Time.time) * Vector3.up;
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
            player.GetComponent<InventoryComponent>().GiveItem(itemType);
        }
        Destroy(gameObject);
    }
}
