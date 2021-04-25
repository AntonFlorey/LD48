using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int touchDamage = 1;
    public float damageCooldown = 1f;
    public float damageAnimTime = 0.2f;
    private Color resetColor;
    public Color damageColor = Color.red;
    public GameObject text = null;
    private Text myText;
    private Collider2D myCollider;
    private Room myRoom;
    private SpriteRenderer myRenderer;
    private bool showingAnimation = false;
    private bool takingDamage = false;
    private float damageAnimTimeLeft = 0;
    private float damageCooldownLeft = 0;

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
        myRenderer = GetComponent<SpriteRenderer>();
        UpdateText();
        resetColor = myRenderer.color;
    }

    private void UpdateText()
    {
        if (text == null)
            return;
        myText.text = health + " / " + maxHealth;
    }

    public void Update()
    {
        if (showingAnimation)
        {
            damageAnimTimeLeft -= Time.deltaTime;
            float time = Mathf.Abs(Mathf.Sin(damageAnimTimeLeft * 10));
            myRenderer.color = Color.Lerp(resetColor, damageColor, time);
            if (damageAnimTimeLeft <= 0)
            {
                showingAnimation = false;
                myRenderer.color = resetColor;
            }
        }

        if (takingDamage)
        {
            damageCooldownLeft -= Time.deltaTime;
            if (damageCooldownLeft <= 0)
                takingDamage = false;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();
        if (gameObject.CompareTag("enemy") && health <= 0)
        {
            DoDie();
        }

        takingDamage = true;
        damageCooldownLeft = damageCooldown;
        showingAnimation = true;
        damageAnimTimeLeft = damageAnimTime;
        myRenderer.color = damageColor;
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
