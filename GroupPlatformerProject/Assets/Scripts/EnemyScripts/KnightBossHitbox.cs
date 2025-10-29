using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightBossHitbox : MonoBehaviour
{
    [Header("Boss Reference")]
    public KnightBossAI boss;

    [Header("Damage Settings")]
    public float damage = 20f; // Must be public
    public float activeTime = 0.5f; // How long the hitbox is active

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
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
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float finalDamage = damage;

                // Apply power boost if the boss has it
                if (boss != null && boss.IsPowerBoosted())
                {
                    finalDamage *= 1.6f;
                    boss.ConsumePowerBoost();
                }

                playerHealth.TakeDamage(finalDamage);
            }
        }
    }
}
