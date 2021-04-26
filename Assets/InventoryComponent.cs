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
    InsaneAttackSpeed
}

public class InventoryComponent : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private PlayerController myPlayer;
    public List<InventoryItem> itemsLeftToSpawn = new List<InventoryItem>();
    
    // Start is called before the first frame update
    void Start()
    {
        Assert.AreEqual(Enum.GetValues(typeof(InventoryItem)).Length, itemsLeftToSpawn.Count + 1);
        myPlayer = GetComponent<PlayerController>();
    }

    public void AddItem(InventoryItem item)
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
        }
        Debug.LogWarning("unhandeled item " + item);
    }
}
