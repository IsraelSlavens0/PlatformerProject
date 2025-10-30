using System.Collections;
using UnityEngine;

public class KnightBossHitbox : MonoBehaviour
{
    [Header("Boss Reference")]
    // Can reference either AI or Phase1Attacks
    public MonoBehaviour bossReference;

    private Collider2D col;

    // Current attack info
    public float damage = 10f;
    private float activeTime = 0.5f;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activate the hitbox using an attack from either AI or Phase1Attacks.
    /// </summary>
    public void Activate(object attack)
    {
        // Determine type of attack dynamically
        if (attack is KnightBossAI.Attack aiAttack)
        {
            damage = aiAttack.damage;
            activeTime = aiAttack.duration;
        }
        else if (attack is KnightBossPhase1Attacks.Attack phaseAttack)
        {
            damage = phaseAttack.damage;
            activeTime = phaseAttack.duration;
        }
        else
        {
            Debug.LogWarning("Invalid attack type passed to hitbox.");
            return;
        }

        gameObject.SetActive(true);
        StartCoroutine(DeactivateAfterTime());
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(activeTime);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float finalDamage = damage;

            // Apply power boost if active (works for Phase1Attacks only)
            if (bossReference is KnightBossPhase1Attacks phaseBoss && phaseBoss.IsPowerBoosted())
            {
                finalDamage *= 1.6f;
                phaseBoss.ConsumePowerBoost();
            }

            playerHealth.TakeDamage(finalDamage);
        }
    }
}
