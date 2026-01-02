using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject impactEffect;
    public ShooterAgentContinuous playerAgent;

    public void Init(ShooterAgentContinuous agent)
    {
        playerAgent=agent;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
        Destroy(impact, 0.3f);

        if (playerAgent!=null && collision.collider.tag == "Enemy")
        {
            playerAgent.OnEnemyHit();
        }

        Destroy(gameObject);
    }
}
