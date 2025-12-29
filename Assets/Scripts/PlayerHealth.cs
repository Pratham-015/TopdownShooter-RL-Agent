using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 0;
    public int ammo = 0;
    [SerializeField] public Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F pressed");
            TakeDamage(10);
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
