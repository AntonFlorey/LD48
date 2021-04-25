using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAttackController : MonoBehaviour
{
    public int maxLifetime = 5;
    public int damage = 1;
    private int lifetimeLeft;
    private bool used = false;
    private bool active = true;
    
    void Start()
    {
        lifetimeLeft = maxLifetime;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!active)
            return;
        if (other.collider.CompareTag("enemy"))
        {
            HealthComponent health = other.collider.GetComponent<HealthComponent>();
            health.TakeDamage(damage);
            used = true;
        }
    }

    void Update()
    {
        lifetimeLeft -= 1;
        if (lifetimeLeft <= 0)
        {
            Destroy(this);
        }

        if (used)
            active = false;
    }
}
