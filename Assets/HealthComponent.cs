using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int touchDamage = 1;
    public float damageCooldown = 1f;
    public float damageAnimTime = 0.2f;
    public bool invulnerable = false;
    private Color resetColor;
    public Color damageColor = Color.red;
    public Color healColor = Color.green;
    public GameObject heartImage = null;
    private RectTransform myHeartImageTransform = null;
    private Rigidbody2D myBody = null;
    private Collider2D myCollider;
    private Room myRoom;
    private SpriteRenderer myRenderer;
    private bool showingAnimation = false;
    private bool takingDamage = false;
    private float damageAnimTimeLeft = 0;
    private float damageCooldownLeft = 0;
    private Vector3 fullScale;
    private int lastDamage;
    public bool knockedBack = false;
    [SerializeField] private float knockBacktime = 0f;

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
        myBody = GetComponent<Rigidbody2D>();
        UpdateText();
        resetColor = myRenderer.color;
        knockBacktime = 0f;
    }

	private void FixedUpdate()
	{
		
	}

	private void UpdateText()
    {
        if (heartImage == null)
            return;
        myHeartImageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * health);
    }

    public void Update()
    {
        knockedBack = knockBacktime > 0f;
        if (showingAnimation)
        {
            damageAnimTimeLeft -= Time.deltaTime;
            float time = Mathf.Abs(Mathf.Sin(damageAnimTimeLeft * 10));
            myRenderer.color = Color.Lerp(resetColor, lastDamage > 0 ? damageColor : healColor, time);
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

    public void TakeDamage(AttackController atk)
    {
        health -= atk.damage;
        UpdateText();
        if(atk.knockback != 0)
		{
            StartCoroutine(Knockback((transform.position - atk.transform.position).normalized * atk.knockback, atk.knockbackDuration));
        }
        takingDamage = true;
        damageCooldownLeft = damageCooldown;
        showingAnimation = true;
        damageAnimTimeLeft = damageAnimTime;
        myRenderer.color = damageColor;
        fullScale = transform.localScale;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();
        takingDamage = true;
        damageCooldownLeft = damageCooldown;
        showingAnimation = true;
        damageAnimTimeLeft = damageAnimTime;
        myRenderer.color = damage > 0 ? damageColor : healColor;
        fullScale = transform.localScale;
        lastDamage = damage;
    }

    public IEnumerator Knockback(Vector2 force, float time)
    {
        Debug.LogWarning("Warning!");
        knockBacktime += time;
        myBody.velocity = Vector3.zero;
        myBody.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(time);
        knockBacktime -= time;
    }

    private void DoDie()
    {
        myRoom.MarkEnemyDeath(gameObject);
        var dropComp = gameObject.GetComponent<DropItemsComponent>();
        if (dropComp != null)
            dropComp.DoDrop();
        Destroy(gameObject);
    }

    public void RecieveTriggerFromHitbox(Collider2D other)
	{
        if (other.CompareTag("attack") && !takingDamage)
        {
			if (!invulnerable)
			{
                Debug.Log("I am getting hurt!" + this.name + " from " + other.name);
                TakeDamage(other.gameObject.GetComponent<AttackController>());
            }
        }
    }
}
