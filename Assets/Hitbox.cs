using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{

    public HealthComponent myHealthComponent;

	private void Start()
	{
		myHealthComponent = GetComponentInParent<HealthComponent>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		myHealthComponent.RecieveTriggerFromHitbox(collision);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		myHealthComponent.RecieveTriggerFromHitbox(other);
	}
}
