using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletForce = 20f;
    public GameObject flashEffect;
    public Camera cam;
    [SerializeField] private bool enemy = true;
    
    void Update()
    {
        Invoke("Shoot", 2f);
        // If cam is null, perform a raycast from firepoint forward to infinity
        if (enemy == true)
        {
            Vector2 direction = -firePoint.up; // Forward direction from firepoint
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, Mathf.Infinity);
            
            if (hit.collider != null)
            {
                Debug.Log($"Raycast hit: {hit.collider.name} at distance {hit.distance}");
                Debug.DrawRay(firePoint.position, direction * hit.distance, Color.red, 2f);
            }
            else
            {
                Debug.Log("Raycast hit nothing");
                Debug.DrawRay(firePoint.position, direction * 1000f, Color.yellow, 2f);
            }
        }
    }

    // IEnumerator ShootWithDelay(float DelayTime)
    // {
    //     yield return new WaitForSeconds(DelayTime);
    //     Shoot();
    // }

    void Shoot()
    {
        Quaternion flashRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, -90f);
        GameObject flash = Instantiate(flashEffect, firePoint.position, flashRotation, firePoint.parent);
        Destroy(flash, 0.1f);
        
        
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();   
        rb.AddForce(firePoint.up * bulletForce *-1, ForceMode2D.Impulse);
    }
}
