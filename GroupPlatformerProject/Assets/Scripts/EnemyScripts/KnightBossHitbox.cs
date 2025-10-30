using System.Collections;
using UnityEngine;

public class KnightBossHitbox : MonoBehaviour
{
    [Header("Boss Reference")]
    public KnightBossAI boss;

    [Header("Damage Settings")]
    public float damage = 20f;
    public float activeTime = 0.5f; // How long hitbox stays active

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
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float finalDamage = damage;

            // Apply power boost if active
            if (boss != null && boss.IsPowerBoosted())
            {
                finalDamage *= 1.6f;
                boss.ConsumePowerBoost();
            }

            playerHealth.TakeDamage(finalDamage);
        }
    }
}
