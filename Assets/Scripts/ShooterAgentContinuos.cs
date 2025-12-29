using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class ShooterAgentContinuous : Agent
{
    public Rigidbody rb;
    public Transform enemy;
    public Camera agentCamera;

    public GameObject bulletPrefab;
    public Transform firePoint;

    public float moveSpeed = 5f;
    public float turnSpeed = 120f;
    public float rayLength = 10f;

    public int health = 100;
    public int ammo = 10;

    private Vector3 startPos;
    private Quaternion startRot;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        health = 100;
        ammo = 10;

        rb.velocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float wallDist = 1f;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayLength))
            wallDist = hit.distance / rayLength;

        sensor.AddObservation(wallDist);
        sensor.AddObservation(health / 100f);
        sensor.AddObservation(ammo / 10f);

        bool visible = IsEnemyVisible();
        sensor.AddObservation(visible ? 1f : 0f);

        if (visible)
            sensor.AddObservation((enemy.position - transform.position).normalized);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous actions
        // [0] Move forward/backward  ∈ [-1, 1]
        // [1] Turn left/right        ∈ [-1, 1]
        // [2] Shoot trigger          ∈ [0, 1]

        float move = actions.ContinuousActions[0];
        float turn = actions.ContinuousActions[1];
        float shoot = actions.ContinuousActions[2];

        rb.MovePosition(transform.position + transform.forward * move * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.fixedDeltaTime);

        if (shoot > 0.5f && ammo > 0)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            ammo--;
        }

        AddReward(-0.001f);

        if (health <= 0)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    bool IsEnemyVisible()
    {
        Vector3 v = agentCamera.WorldToViewportPoint(enemy.position);
        return v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
    }
}
