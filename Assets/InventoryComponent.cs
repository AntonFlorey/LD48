using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private PlayerController myPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        myPlayer = GetComponent<PlayerController>();
    }

    public void AddItem(InventoryItem item)
    {
        items.Add(item);
        switch (item)
        {
            case InventoryItem.DoubleJumpBoots:
                myPlayer.maxjumps++;
                break;
        }
        Debug.LogWarning("unhandeled item " + item);
    }
}
