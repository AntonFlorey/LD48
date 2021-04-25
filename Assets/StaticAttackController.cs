using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAttackController : MonoBehaviour
{
    public int maxLifetime = 500;
    public int damage = 1;
    public bool active = false;
    private int lifetimeLeft;
    
    void Start()
    {
        lifetimeLeft = maxLifetime;
    }

    void Update()
    {
        if (!active)
            return;
        lifetimeLeft -= 1;
        if (lifetimeLeft <= 0)
        {
            Destroy(gameObject);
        }
    }
}
