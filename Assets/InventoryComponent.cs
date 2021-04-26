using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum InventoryItem
{
    Ignore = 0,
    DoubleJumpBoots = 1,
    InsaneKnockback = 2,
    EvenMoreInsaneKnockback = 3,
    InsaneAttackSpeed = 4,
    Arrow = 5
}

public class InventoryComponent : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private PlayerController myPlayer;
    public List<InventoryItem> itemsLeftToSpawn = new List<InventoryItem>();
    public GameObject arrowItemAttackPrefab;
    public List<Sprite> allItemSprites;

    public GameObject display;
    public GameObject iconPrefab;
    private List<GameObject> currentDisplayIcons = new List<GameObject>();
    public int displayIconSize = 100;
    public int displayIconPadding = 30;
    
    // Start is called before the first frame update
    void Start()
    {
        Assert.AreEqual(Enum.GetValues(typeof(InventoryItem)).Length, itemsLeftToSpawn.Count + 1);
        Assert.AreEqual(Enum.GetValues(typeof(InventoryItem)).Length, allItemSprites.Count);

        myPlayer = GetComponent<PlayerController>();
        RerenderInventory();
    }

    public void GiveItem(InventoryItem item)
    {
        AddItemEffect(item);
        RerenderInventory();
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

    public void RerenderInventory()
    {
        foreach (var obj in currentDisplayIcons)
        {
            Destroy(obj);
        }

        for (var iconNum = 0; iconNum < items.Count; iconNum++)
        {
            int offsetX = 0;
            int offsetY = iconNum * displayIconSize;
            if (iconNum > 0)
                offsetY += (iconNum - 1) * displayIconPadding;
            InventoryItem item = items[iconNum];
            var icon = Instantiate(iconPrefab, display.transform);
            icon.GetComponent<Image>().sprite = allItemSprites[(int) item];
            Debug.Log(offsetX + ":" + offsetY);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offsetY, displayIconSize);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, offsetX, displayIconSize);
            currentDisplayIcons.Add(icon);
        }
    }
}
