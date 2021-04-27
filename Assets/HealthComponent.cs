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
    public bool immuneToknockback = false;
    private Color resetColor;
    public Color damageColor = Color.red;
    public Color healColor = Color.green;
    public Color cutoutColor = Color.black;
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
    private bool droppedItems = false;
    [SerializeField] private float knockBacktime = 0f;
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material cutoutMat;
    
    void Start()
    {
        if (GetComponent<PlayerController>() != null)
        {
            myRoom = GetComponent<PlayerController>().currentRoomNode.myRoom;
        } else if (transform.parent != null)
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

	private void UpdateText()
    {
        if (heartImage == null)
            return;
        myHeartImageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * health);
    }

    public void Update()
    {
        ToggleMaterial();
        knockedBack = knockBacktime > 0f;
        if (showingAnimation)
        {
            damageAnimTimeLeft -= Time.deltaTime;
            float time = Mathf.Abs(Mathf.Sin(damageAnimTimeLeft * 10));
            myRenderer.color = Color.Lerp(cutoutColor, lastDamage > 0 ? damageColor : healColor, time);
            if (damageAnimTimeLeft <= 0)
            {
                showingAnimation = false;
                myRenderer.color = resetColor;
            }
            if (IsDead())
                UpdateWhileDead();
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
    
    private void UpdateWhileDead()
    {
        if (CompareTag("enemy"))
        {
            transform.localScale = fullScale * (damageAnimTimeLeft / damageAnimTime);
        }
        if (CompareTag("Player"))
        {
            // TODO!
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    private void ToggleMaterial()
	{
		if (showingAnimation)
		{
            myRenderer.material = cutoutMat;
		}
		else
		{
            myRenderer.material = defaultMat;
		}
	}

    public void TakeDamage(AttackController atk)
    {
        if(!immuneToknockback && atk.knockback != 0)
		{
            StartCoroutine(Knockback((transform.position - atk.transform.position).normalized * atk.knockback, atk.knockbackDuration));
        }
        TakeDamage(atk.damage);
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
        if (health <= 0 && !droppedItems)
        {
            Debug.Log("drop items now!");
            var dropComp = gameObject.GetComponent<DropItemsComponent>();
            if (dropComp != null)
            {
                dropComp.DoDrop();
                droppedItems = true;
            }
        }
    }

    public IEnumerator Knockback(Vector2 force, float time)
    {
        knockBacktime += time;
        knockedBack = true;
        myBody.velocity = Vector3.zero;
        myBody.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(time);
        knockBacktime -= time;
    }

    private void DoDie()
    {
        if (gameObject.CompareTag("enemy"))
        {
            myRoom.MarkEnemyDeath(gameObject);
            Destroy(gameObject);
        }
        if (gameObject.CompareTag("Player"))
        {
            myRoom.roomNode.manager.OnPlayerDie();
        }
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
