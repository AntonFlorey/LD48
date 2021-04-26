using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public int maxLifetime = 500;
    public int damage = 1;
    public bool active = false;
    private int lifetimeLeft;
    public bool autoDestroy = false;
    public float knockback = 0f;
    public float knockbackDuration = 0f;
    
    void Start()
    {
        lifetimeLeft = maxLifetime;
    }

    void Update()
    {
		if (autoDestroy)
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
}
