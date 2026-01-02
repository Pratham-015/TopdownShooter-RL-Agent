using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody2D))]
public class ShooterAgentContinuous : Agent
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Camera agentCamera;

    [Header("Game Objects")]
    public List<Transform> enemies;
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Rewards")]
    public float deathPenalty = -1.0f;
    public float timePenalty = -0.001f;
    public float hitReward = 0.5f;
    public float randomShootingPenalty=-0.01f;

    [Header("Parameters")]
    public float moveSpeed = 5f;
    public float turnSpeed = 120f;
    public float rayLength = 10f;

    public int health = 100;
    public int maxHealth = 100;
    public int ammo = 10;
    public int maxAmmo = 10;

    public float bulletForce=15f;
    public float shootCooldown=0.25f;

    bool visible=false;
    private Vector2 startPos;
    private Quaternion startRot;
    private Vector2 lastPosition;
    private float lastShootTime;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;
        startRot = transform.rotation;
        lastPosition = rb.position;
    }

    public override void OnEpisodeBegin()
    {
        health = maxHealth;
        ammo = maxAmmo;

        rb.velocity = Vector2.zero;
        rb.position = startPos;
        transform.rotation = startRot;
        lastPosition = rb.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 delta = rb.position - lastPosition;
        lastPosition = rb.position;
        sensor.AddObservation(delta.x/moveSpeed);         // 1 float
        sensor.AddObservation(delta.y/moveSpeed);         // 1 float

        float wallDist = 1f;
        RaycastHit2D hit = Physics2D.Raycast(rb.position, transform.up, rayLength);
        if (hit.collider!=null)
            wallDist = hit.distance / rayLength;
        sensor.AddObservation(wallDist);        // 1 float

        sensor.AddObservation((float)health / maxHealth);   // 1 float
        sensor.AddObservation((float)ammo / maxAmmo);      // 1 float

        visible = false;
        Vector3 dir = Vector3.zero;
        float minD = float.MaxValue;

        foreach (Transform e in enemies)
        {
            if (!IsVisible(e)) continue;

            float d = Vector2.Distance(transform.position, e.position);
            if (d < minD)
            {
                minD = d;
                dir = (e.position - transform.position).normalized;
                visible = true;
            }
        }

        sensor.AddObservation(visible ? 1f : 0f);       // 1 float
        sensor.AddObservation(visible ? dir.x : 0f);    // 1 float
        sensor.AddObservation(visible ? dir.y : 0f);    // 1 float
        // Direction of nearest enemy
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = Mathf.Clamp(actions.ContinuousActions[0], 0f, 1f);
        // move = 0 (doesn't move) or 1 (moves)
        float turn = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        // turn = -1 (clockwise) or 0 (doesn't turn) or 1 (anti-clockwise)
        float shoot = actions.ContinuousActions[2];
        // shoot = 0 (doesn't shoot) or 1 (shoots)

        rb.MovePosition(transform.position +
                        transform.up * move * moveSpeed * Time.fixedDeltaTime);

        transform.Rotate(Vector3.forward, turn * turnSpeed * Time.fixedDeltaTime);

        if (shoot > 0.5f && ammo > 0 && Time.time - lastShootTime > shootCooldown)
        {
            AgentShoot();
            ammo--;
            lastShootTime = Time.time;

            if (!visible)
            {
                AddReward(randomShootingPenalty);
                // Discourages agent to shoot when enemy is not visible
            }
        }

        AddReward(timePenalty);
        if (health <= 0)
        {
            AddReward(deathPenalty);
            EndEpisode();
        }
    }
    void AgentShoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();   
        bullet.GetComponent<Bullet>().Init(this);

        rb.AddForce(firePoint.up * bulletForce , ForceMode2D.Impulse);
    }
    public void OnEnemyHit()
    {
        AddReward(hitReward);
    }
    bool IsVisible(Transform t)
    {
        Vector3 v = agentCamera.WorldToViewportPoint(t.position);
        return v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
    }
}
