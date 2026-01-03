using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum Owner { Agent, Enemy }
    public Owner owner;

    public ShooterAgentContinuous agentOwner;
    public int damage = 10;
    private bool hit = false;

    public void InitAgentBullet(ShooterAgentContinuous agent)
    {
        owner = Owner.Agent;
        agentOwner = agent;
    }

    public void InitEnemyBullet()
    {
        owner = Owner.Enemy;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hit) return;
        hit = true;

        if (owner == Owner.Agent && collision.collider.CompareTag("Enemy"))
        {
            agentOwner.OnEnemyHit(collision.collider.transform);
        }

        if (owner == Owner.Enemy)
        {
            ShooterAgentContinuous agent =
                collision.collider.GetComponent<ShooterAgentContinuous>();

            if (agent != null)
            {
                agent.health -= damage;
            }
        }

        Destroy(gameObject);
    }
}
