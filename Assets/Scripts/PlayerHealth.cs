using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth=100;
    public int bulletDamage=5;
    public int health = 0;
    public int ammo = 10;
    public Rigidbody2D rb;
    [SerializeField] public Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    void OnCollisionEnter2D(Collision2D col){
        if(col.collider.tag == "bullet"){
            TakeDamage(bulletDamage);
        }

    }

    void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);
        Vector3 scale = healthBar.rectTransform.localScale;
        scale.x = 4* health / 100f;
        healthBar.rectTransform.localScale = scale;
    }
}
