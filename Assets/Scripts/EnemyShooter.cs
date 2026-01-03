using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Objects")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Bullet Parameters")]
    public float initialBulletDelay=2f;
    public float bulletForce = 20f;
    public float bulletCooldown=0.1f;
    private bool canShoot=false;

    [Header("Visual Objects")]
    public GameObject flashEffect;
    public Camera cam;
    [SerializeField] private bool enemy = true;

    void Start()
    {
        Invoke(nameof(EnableShooting),initialBulletDelay);
    }
    void Update()
    {
        if (!canShoot) return;
        
        StartCoroutine(ShootWithDelay(bulletCooldown));
        // If cam is null, perform a raycast from firepoint forward to infinity
        /*if (enemy == true)
        {
            Vector2 direction = -firePoint.up; // Forward direction from firepoint
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, Mathf.Infinity);
            
            if (hit.collider != null && hit.collider.tag!="Boundary")
            {
                Debug.Log($"Enemy Raycast hit: {hit.collider.name} at distance {hit.distance}");
                Debug.DrawRay(firePoint.position, direction * hit.distance, Color.red, 2f);
            }
            else
            {
                Debug.Log("Enemy Raycast hit nothing");
                Debug.DrawRay(firePoint.position, direction * 1000f, Color.yellow, 2f);
            }
        }*/
    }

    void EnableShooting()
    {
        canShoot=true;
    }
    IEnumerator ShootWithDelay(float DelayTime)
    {
        canShoot=false;
        yield return new WaitForSeconds(DelayTime);
        Shoot();
        canShoot=true;
    }
    void Shoot()
    {
        Quaternion flashRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, -90f);
        GameObject flash = Instantiate(flashEffect, firePoint.position, flashRotation, firePoint.parent);
        Destroy(flash, 0.1f);
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        bullet.GetComponent<Bullet>().InitEnemyBullet();
        
        rb.AddForce(firePoint.up * bulletForce , ForceMode2D.Impulse);
    }
}
