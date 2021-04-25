using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int touchDamage = 1;
    public GameObject text = null;
    private Text myText;

    void Start()
    {
        health = maxHealth;
        if (text != null)
        {
            myText = text.GetComponent<Text>();
        }
        UpdateText();
    }

    void UpdateText()
    {
        if (text == null)
            return;
        myText.text = health + " / " + maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();
        // TODO: Die?
    }
}
