using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int touchDamage = 1;
    public GameObject text = null;
    private Text myText;
    private Collider2D myCollider;
    private Room myRoom;

    void Start()
    {
        if (transform.parent != null)
            myRoom = transform.parent.gameObject.GetComponent<Room>();
        health = maxHealth;
        if (text != null)
        {
            myText = text.GetComponent<Text>();
        }
        myCollider = GetComponent<Collider2D>();
        UpdateText();
    }

    private void UpdateText()
    {
        if (text == null)
            return;
        myText.text = health + " / " + maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();
        if (gameObject.CompareTag("enemy") && health <= 0)
        {
            DoDie();
        }
    }

    private void DoDie()
    {
        myRoom.MarkEnemyDeath(gameObject);
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (transform.gameObject.CompareTag("enemy") && other.CompareTag("attack"))
        {
            Physics2D.IgnoreCollision(myCollider, other);
            TakeDamage(other.gameObject.GetComponent<StaticAttackController>().damage);
        }
    }
}
