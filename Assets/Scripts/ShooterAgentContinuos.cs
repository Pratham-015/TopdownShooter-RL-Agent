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
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Enemy Spawning")]
    public GameObject enemyPrefab;
    public int enemiesPerEpisode=5;
    public float minSpawnDistance = 8f;
    public float maxSpawnDistance = 12f;
    public int maxSpawnTries=30;
    private List<Transform> enemies = new List<Transform>();


    [Header("Episode Limits")]
    public int maxEpisodeSteps=1500;
    private int episodeStepCount;
    
    [Header("Rewards")]
    public float deathPenalty = -1.0f;
    public float timePenalty = -0.001f;
    public float hitReward = 1.0f;
    public float enemyReward = 10.0f;

    [Header("Parameters")]
    public float moveSpeed = 5f;
    public float turnSpeed = 120f;
    public float rayLength = 10f;

    public int health = 100;
    public int maxHealth = 100;
    //public int ammo = 10;
    //public int maxAmmo = 10;

    public float bulletForce=15f;
    public float shootCooldown=0.25f;

    [Header("Metric")]
    private int totalShots = 0;
    private int totalHits = 0;
    private int totalEpisodes = 0;
    private int totalStepsAlive = 0;
    private int episodeStepsAlive = 0;

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
        if (episodeStepsAlive > 0)
        {
            totalEpisodes++;
            totalStepsAlive+=episodeStepsAlive;
        }
        episodeStepCount=0;
        episodeStepsAlive = 0;

        if (totalEpisodes>0 && totalEpisodes % 10 == 0)
        {
            LogTensorBoardStats();
        }
        

        health = maxHealth;
        //ammo = maxAmmo;

        rb.velocity = Vector2.zero;
        rb.position = startPos;
        transform.rotation = startRot;
        lastPosition = rb.position;

        SpawnEnemies();
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
        //sensor.AddObservation((float)ammo / maxAmmo);      // 1 float

        bool visible = false;
        Vector3 dir = Vector3.zero;
        float minD = float.MaxValue;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Transform e = enemies[i];
            if (e == null || !e.gameObject.activeInHierarchy)
            {
                enemies.RemoveAt(i);
                continue;
            }

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
        episodeStepCount++;
        episodeStepsAlive++;

        float move = Mathf.Clamp(actions.ContinuousActions[0], 0f, 1f);
        // move = 0 (doesn't move) or 1 (moves)
        float turnL = Mathf.Clamp(actions.ContinuousActions[1], 0f, 1f);
        float turnR = Mathf.Clamp(actions.ContinuousActions[2], 0f, 1f);
        // turn = -1 (clockwise) or 0 (doesn't turn) or 1 (anti-clockwise)
        float shoot = actions.ContinuousActions[3];
        // shoot = 0 (doesn't shoot) or 1 (shoots)

        rb.MovePosition(transform.position -
                        transform.up * move * moveSpeed * Time.fixedDeltaTime);

        transform.Rotate(Vector3.forward, (turnL-turnR) * turnSpeed * Time.fixedDeltaTime);

        if (shoot > 0.5f && Time.time - lastShootTime > shootCooldown)
        {
            AgentShoot();
            //if (ammo>0) ammo--;
            lastShootTime = Time.time;

        }

        AddReward(timePenalty);
        if (health <= 0)
        {
            AddReward(deathPenalty);
            EndEpisode();
        }

        if (enemies.Count == 0)
        {
            EndEpisode();
        }
        if (episodeStepCount >= maxEpisodeSteps)
        {
            EndEpisode();
        }
    }
    void AgentShoot()
    {
        totalShots++;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();   
        bullet.GetComponent<Bullet>().InitAgentBullet(this);

        rb.AddForce(firePoint.up * bulletForce , ForceMode2D.Impulse);
    }
    public void OnEnemyHit(Transform enemy)
    {
        if (!enemies.Contains(enemy)) return;

        PlayerHealth h=enemy.gameObject.GetComponent<PlayerHealth>();
        if (h==null) return;

        totalHits++;
        AddReward(hitReward);

        if (h.health <= 0)
        {
            enemies.Remove(enemy);
            enemy.gameObject.SetActive(false);
            AddReward(enemyReward);
        }
    }
    bool IsVisible(Transform t)
    {
        if (agentCamera==null || t==null) return false;
        Vector3 v = agentCamera.WorldToViewportPoint(t.position);
        if (v.z<=0f || v.x<=0f || v.y<=0f || v.x>=1f || v.y>=1f) return false;
        
        Vector2 origin = rb.position;
        Vector2 direction = (Vector2)t.position - origin;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(origin,direction.normalized,
            distance,LayerMask.GetMask("Wall", "Enemy"));

        // Visible only if first hit is the enemy
        return hit.collider != null && hit.collider.transform == t;
    }
    void SpawnEnemies()
    {
        ClearEnemies();

        for (int i = 0; i < enemiesPerEpisode; i++)
        {
            Vector2 spawnPos = GetValidEnemySpawnPosition();
            float randomAngle = Random.Range(0f, 360f);

            GameObject enemy = Instantiate( enemyPrefab,spawnPos,
                Quaternion.Euler(0f, 0f, randomAngle));

            enemies.Add(enemy.transform);
        }
    }
    Vector2 GetValidEnemySpawnPosition()
    {
        for (int i = 0; i < maxSpawnTries; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 candidate = startPos + dir * dist;

            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.4f, LayerMask.GetMask("Wall"));

            if (hit == null)
                return candidate;
        }
        
        return startPos + Random.insideUnitCircle.normalized * maxSpawnDistance;
    }

    void ClearEnemies()
    {
        foreach (Transform e in enemies)
        {
            if (e != null)
                Destroy(e.gameObject);
        }
        enemies.Clear();
    }
    void LogTensorBoardStats()
    {
        if (totalEpisodes == 0) return;

        float accuracy = totalShots > 0 ? (float)totalHits / totalShots : 0f;
        float meanSurvivalTime = ((float)totalStepsAlive / totalEpisodes) / maxEpisodeSteps;
        float finalScore = accuracy * meanSurvivalTime;

        var stats = Academy.Instance.StatsRecorder;

        stats.Add("Eval/Accuracy", accuracy);
        stats.Add("Eval/MeanSurvivalTime", meanSurvivalTime);
        stats.Add("Eval/FinalScore", finalScore);
        stats.Add("Eval/Shots", totalShots);
        stats.Add("Eval/Hits", totalHits);
    }

}
