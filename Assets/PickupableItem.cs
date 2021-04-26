using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableItem : MonoBehaviour
{
    public int healAmount = 0;
    
    public void OnPickup(GameObject player)
    {
        if (healAmount > 0)
        {
            player.GetComponent<HealthComponent>().TakeDamage(-healAmount);
        }
        Destroy(gameObject);
    }
}
