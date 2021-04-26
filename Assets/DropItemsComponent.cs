using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class DropItemsComponent : MonoBehaviour
{
    public List<GameObject> droppedItems = new List<GameObject>();
    public List<float> dropChances = new List<float>();
    public bool dropRandomInventoryItem = false;
    public GameObject randomInventoryItemPrefab = null;
    public Vector3 dropOffset = new Vector3(0, 0.5f, 0);
    public Vector3 dropForceMin = new Vector3(-0.5f, 0.2f, 0f);
    public Vector3 dropForceMax = new Vector3(0.5f, 0.6f, 0f);
    
    void Start()
    {
        Assert.AreEqual(droppedItems.Count, dropChances.Count);
    }

    private void DoDropSingleItem(GameObject drop, bool moveToPlayer=true)
    { 
        drop.transform.position = transform.position + dropOffset;
        drop.transform.position = new Vector3(drop.transform.position.x, drop.transform.position.y, 0);
        var player = gameObject.transform.parent.gameObject.GetComponent<Room>().roomNode.manager.player;
        drop.GetComponent<PickupableItem>().moveTowardsObject = player;
        var maybeParentBody = gameObject.GetComponent<Rigidbody2D>();
        var baseVelocity = maybeParentBody == null ? Vector2.zero : maybeParentBody.velocity;
        var rndForce = new Vector3(
            Random.Range(dropForceMin.x, dropForceMax.x),
            Random.Range(dropForceMin.y, dropForceMax.y),
            Random.Range(dropForceMin.z, dropForceMax.z));
        Debug.Log(rndForce);
        var itemBody = drop.GetComponent<Rigidbody2D>();
        itemBody.velocity = baseVelocity;
        itemBody.AddForce(rndForce);
    }

    public void DoDrop()
    {
        Debug.Log("Do drop!");
        var parent = transform.parent.gameObject.transform;
        for (var itemNum = 0; itemNum < droppedItems.Count; itemNum++)
        {
            var rnd = Random.Range(0f, 1f);
            if (rnd < dropChances[itemNum])
            {
                var drop = Instantiate(droppedItems[itemNum], parent);
                DoDropSingleItem(drop);
            }
        }

        if (dropRandomInventoryItem)
        {
            Debug.Log("drop inventory item");
            var inventory = parent.GetComponent<Room>().roomNode.manager.player.GetComponent<InventoryComponent>();
            if (inventory.itemsLeftToSpawn.Count == 0)
                Debug.LogWarning("could not drop any item! nothing left!");
            else
            {
                int rnd = Random.Range(0, inventory.itemsLeftToSpawn.Count);
                var itemType = inventory.itemsLeftToSpawn[rnd];
                Debug.Log("go for" + itemType);
                inventory.itemsLeftToSpawn.RemoveAt(rnd);
                var drop = Instantiate(randomInventoryItemPrefab, parent);
                // todo: add a texture.
                PickupableItem pickup = drop.GetComponent<PickupableItem>();
                drop.GetComponent<SpriteRenderer>().sprite = inventory.allItemSprites[(int) itemType];
                pickup.givesItem = true;
                pickup.itemType = itemType;
                DoDropSingleItem(drop);
            }
        }
    }
}
