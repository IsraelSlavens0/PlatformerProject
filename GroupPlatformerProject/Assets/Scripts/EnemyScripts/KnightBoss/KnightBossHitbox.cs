using System;
using System.Collections;
using System.Collections.Generic;
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

    // ✅ New: optional delegate for Phase 2 or custom behavior
    public Action<GameObject> OnHit;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true;
        gameObject.SetActive(false);
    }

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

        // ✅ Optional: clear OnHit when deactivated to avoid stale delegates
        OnHit = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        // ✅ If Phase 2 attached a custom handler, use it
        if (OnHit != null)
        {
            OnHit.Invoke(collision.gameObject);
            return;
        }

        // Default Phase 1 behavior
        Debug.Log($"Lunge hit player with {damage} damage");

        float finalDamage = damage;
        if (bossReference is KnightBossPhase1Attacks phaseBoss && phaseBoss.IsPowerBoosted())
        {
            finalDamage *= 1.6f;
            phaseBoss.ConsumePowerBoost();
        }

        playerHealth.TakeDamage(finalDamage);
    }
}
