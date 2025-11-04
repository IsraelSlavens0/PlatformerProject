using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int health = 100;

    [Header("Phase 2 Trigger")]
    [Tooltip("Reference to the Knight Phase 2 Trigger component.")]
    public KnightPhase2Trigger phase2Trigger;

    private bool phase2Triggered = false;

    [Header("Drop Settings")]
    public GameObject[] dropPrefabs;
    public int minDropCount = 1;
    public int maxDropCount = 3;
    public Vector3 dropOffset = Vector3.zero;

    private void Start()
    {
        health = maxHealth;

        // Make sure Phase 2 starts disabled through the trigger
        if (phase2Trigger != null)
            phase2Trigger.InitializePhaseState();
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }

        // Check if we should trigger phase 2
        if (!phase2Triggered && phase2Trigger != null && phase2Trigger.ShouldTriggerPhase2(health, maxHealth))
        {
            phase2Triggered = true;
            phase2Trigger.TriggerPhase2();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    private void Die()
    {
        // Drop items
        if (dropPrefabs != null && dropPrefabs.Length > 0)
        {
            int dropCount = Random.Range(minDropCount, maxDropCount + 1);
            for (int i = 0; i < dropCount; i++)
            {
                GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
                Instantiate(prefab, transform.position + dropOffset, Quaternion.identity);
            }
        }

        Destroy(gameObject);
        Debug.Log("Knight Boss defeated!");
    }

    public int GetCurrentHealth() => health;
    public float GetHealthPercent() => health / (float)maxHealth;
    public bool IsPhase2() => phase2Triggered;
}
