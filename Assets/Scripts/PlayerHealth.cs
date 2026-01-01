using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth=100;
    public int bulletDamage=5;
    public int health = 0;
    public Rigidbody2D rb;
    public Image healthBar;
    public TMP_Text healthText;

    void Start()
    {
        health = maxHealth;
    }

    void OnCollisionEnter2D(Collision2D col){
        if(col.collider.tag == "bullet"){
            TakeDamage(bulletDamage);
            Debug.Log(health);
            if (health <= 0)
            {
                Death();
            }
        }

    }
    void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);

        UpdateHealthUI();
    }
    void UpdateHealthUI()
    {
        Vector3 scale = healthBar.rectTransform.localScale;
        scale.x = (float)health / maxHealth;
        healthBar.rectTransform.localScale = scale;

        healthText.text=gameObject.name+" Health";
    }
    void Death()
    {
        Destroy(gameObject);
        Debug.Log(gameObject.name+" Died");
    }
}
