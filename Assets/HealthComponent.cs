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
    public GameObject heartImage = null;
    private RectTransform myHeartImageTransform = null;
    private Collider2D myCollider;
    private Room myRoom;
    private SpriteRenderer myRenderer;
    private bool showingAnimation = false;
    private bool takingDamage = false;
    private float damageAnimTimeLeft = 0;
    private float damageCooldownLeft = 0;
    private Vector3 fullScale;

    void Start()
    {
        if (transform.parent != null)
            myRoom = transform.parent.gameObject.GetComponent<Room>();
        health = maxHealth;
        if (heartImage != null)
            myHeartImageTransform = heartImage.GetComponent<RectTransform>();
        var colliders = GetComponentsInChildren(typeof(Collider2D), true);
        myCollider = (Collider2D)colliders[colliders.Length - 1];
        myRenderer = GetComponent<SpriteRenderer>();
        UpdateText();
        resetColor = myRenderer.color;
    }

    private void UpdateText()
    {
        if (heartImage == null)
            return;
        myHeartImageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * health);
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
            if (IsDead())
            {
                transform.localScale = fullScale * (damageAnimTimeLeft / damageAnimTime);
            }
        }

        if (takingDamage)
        {
            damageCooldownLeft -= Time.deltaTime;
            if (damageCooldownLeft <= 0)
            {
                takingDamage = false;
                if (IsDead())
                    DoDie();
            }
        }
    }

    public bool IsDead()
    {
        return gameObject.CompareTag("enemy") && health <= 0;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();

        takingDamage = true;
        damageCooldownLeft = damageCooldown;
        showingAnimation = true;
        damageAnimTimeLeft = damageAnimTime;
        myRenderer.color = damageColor;
        fullScale = transform.localScale;
    }

    private void DoDie()
    {
        myRoom.MarkEnemyDeath(gameObject);
        Destroy(gameObject);
    }

    public void RecieveTriggerFromHitbox(Collider2D other)
	{
        if (other.CompareTag("attack") && !takingDamage)
        {
            TakeDamage(other.gameObject.GetComponent<StaticAttackController>().damage);   
        }
    }
}
