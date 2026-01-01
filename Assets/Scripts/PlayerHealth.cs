using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth=100f;
    public float bulletDamage=5f;
    public float health = 0f;
    public int ammo = 10;
    public Rigidbody2D rb;
    [SerializeField] public Image healthBar;

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
    void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);
        Vector3 scale = healthBar.rectTransform.localScale;
        scale.x = health / maxHealth;
        healthBar.rectTransform.localScale = scale;
    }
    void Death()
    {
        Destroy(gameObject);
        Debug.Log(gameObject.name+" Died");
    }
}
