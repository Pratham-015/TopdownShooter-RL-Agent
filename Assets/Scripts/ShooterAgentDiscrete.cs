using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class ShooterAgentDiscrete : Agent
{
    public Rigidbody rb;
    public Transform enemy;
    public Camera agentCamera;

    public GameObject bulletPrefab;
    public Transform firePoint;

    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
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
        // Raycast distance
        float wallDist = 1f;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayLength))
            wallDist = hit.distance / rayLength;

        sensor.AddObservation(wallDist);

        // Self state
        sensor.AddObservation(health / 100f);
        sensor.AddObservation(ammo / 10f);

        // Enemy visibility
        bool visible = IsEnemyVisible();
        sensor.AddObservation(visible ? 1f : 0f);

        if (visible)
            sensor.AddObservation((enemy.position - transform.position).normalized);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions
        // [0] Move: 0 = none, 1 = forward
        // [1] Turn: 0 = none, 1 = left, 2 = right
        // [2] Shoot: 0 = no, 1 = yes

        int move = actions.DiscreteActions[0];
        int turn = actions.DiscreteActions[1];
        int shoot = actions.DiscreteActions[2];

        if (move == 1)
            rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime);

        if (turn == 1)
            transform.Rotate(Vector3.up, -turnSpeed * Time.fixedDeltaTime);
        else if (turn == 2)
            transform.Rotate(Vector3.up, turnSpeed * Time.fixedDeltaTime);

        if (shoot == 1 && ammo > 0)
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
