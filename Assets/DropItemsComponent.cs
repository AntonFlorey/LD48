using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DropItemsComponent : MonoBehaviour
{
    public List<GameObject> droppedItems = new List<GameObject>();
    public List<float> dropChances = new List<float>();
    public Vector3 dropOffset = new Vector3(0, 0.5f, 0);
    
    void Start()
    {
        Assert.AreEqual(droppedItems.Count, dropChances.Count);
    }

    public void DoDrop()
    {
        for (var itemNum = 0; itemNum < droppedItems.Count; itemNum++)
        {
            var rnd = Random.Range(0f, 1f);
            if (rnd < dropChances[itemNum])
            {
                var parent = transform.parent.gameObject.transform;
                var drop = Instantiate(droppedItems[itemNum], parent);
                drop.transform.position = transform.position += dropOffset;
            }
        }
    }
}
