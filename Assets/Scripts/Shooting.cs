using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shooting : MonoBehaviour
{
    [Header("Objects")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Bullet Parameters")]
    public float bulletForce = 20f;
    public int ammo=0;
    public int maxAmmo=10;
    public float reloadTime=3f;
    private bool isReloading=false;

    [Header("Visual Objects")]
    public GameObject flashEffect;
    public Camera cam;
    public TMP_Text ammoText;
    public Image ammoBar;
    [SerializeField] private bool enemy = true;

    void Start()
    {
        ammo=maxAmmo;
    }
    void Update()
    {
        if (isReloading) return;
        if (ammo <= 0)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        // If cam is null, perform a raycast from firepoint forward to infinity
        /*if (enemy == true)
        {
            Vector2 direction = -firePoint.up; // Forward direction from firepoint
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, Mathf.Infinity);
            
            if (hit.collider != null && hit.collider.tag!="Boundary")
            {
                Debug.Log($"Raycast hit: {hit.collider.name} at distance {hit.distance}");
                Debug.DrawRay(firePoint.position, direction * hit.distance, Color.red, 2f);
            }
            else
            {
                Debug.Log("Raycast hit nothing");
                Debug.DrawRay(firePoint.position, direction * 1000f, Color.yellow, 2f);
            }
        }*/
    }
    void Shoot()
    {
        Quaternion flashRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, -90f);
        GameObject flash = Instantiate(flashEffect, firePoint.position, flashRotation, firePoint.parent);
        Destroy(flash, 0.1f);
            
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();   
        rb.AddForce(firePoint.up * bulletForce *-1, ForceMode2D.Impulse);

        ammo--;
        UpdateAmmoUI();
    }
    IEnumerator Reload()
    {
        isReloading=true;
        ammoText.text="Reloading";
        yield return new WaitForSeconds(reloadTime);
        ammo=maxAmmo;
        UpdateAmmoUI();
        isReloading=false;
    }
    void UpdateAmmoUI()
    {
        ammoText.text="Ammo - "+ammo.ToString();

        Vector3 scale = ammoBar.rectTransform.localScale;
        scale.x = (float)ammo / maxAmmo;
        ammoBar.rectTransform.localScale = scale;
    }
}
