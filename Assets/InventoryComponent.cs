using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum InventoryItem
{
    Ignore,
    DoubleJumpBoots,
    InsaneKnockback,
    EvenMoreInsaneKnockback,
    InsaneAttackSpeed,
    Arrow
}

public class InventoryComponent : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private PlayerController myPlayer;
    public List<InventoryItem> itemsLeftToSpawn = new List<InventoryItem>();
    public GameObject arrowItemAttackPrefab;
    public List<Sprite> allItemSprites;
    
    // Start is called before the first frame update
    void Start()
    {
        Assert.AreEqual(Enum.GetValues(typeof(InventoryItem)).Length, itemsLeftToSpawn.Count + 1);
        Assert.AreEqual(Enum.GetValues(typeof(InventoryItem)).Length, allItemSprites.Count);

        myPlayer = GetComponent<PlayerController>();
    }

    public void AddItemEffect(InventoryItem item)
    {
        items.Add(item);
        switch (item)
        {
            case InventoryItem.DoubleJumpBoots:
                myPlayer.maxjumps++;
                return;
            case InventoryItem.InsaneKnockback:
                myPlayer.knockbackMultiplier += 1.0f;
                return;
            case InventoryItem.EvenMoreInsaneKnockback:
                myPlayer.knockbackMultiplier += 1.0f;
                return;
            case InventoryItem.InsaneAttackSpeed:
                myPlayer.attackSpeed += 0.5f;
                return;
            case InventoryItem.Arrow:
                myPlayer.attackPrefab = arrowItemAttackPrefab;
                return;
        }
        Debug.LogWarning("unhandeled item " + item);
    }
}
