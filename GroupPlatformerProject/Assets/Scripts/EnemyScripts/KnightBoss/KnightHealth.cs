using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Phase Settings")]
    [Tooltip("Reference to the Phase 1 attack script")]
    public MonoBehaviour phase1Script;

    [Tooltip("Reference to the Phase 2 attack script")]
    public MonoBehaviour phase2Script;

    [Tooltip("Health percentage to trigger Phase 2 (0-1)")]
    [Range(0f, 1f)]
    public float phase2Threshold = 0.5f;

    private bool phase2Triggered = false;

    [Header("Drop Settings")]
    public GameObject[] dropPrefabs;
    public int minDropCount = 1;
    public int maxDropCount = 3;
    public Vector3 dropOffset = Vector3.zero;

    private void Start()
    {
        currentHealth = maxHealth;

        // Ensure Phase 2 starts disabled
        if (phase2Script != null)
            phase2Script.enabled = false;

        if (phase1Script != null)
            phase1Script.enabled = true;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Trigger Phase 2 if threshold reached
        if (!phase2Triggered && currentHealth <= Mathf.CeilToInt(maxHealth * phase2Threshold))
        {
            TriggerPhase2();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void TriggerPhase2()
    {
        phase2Triggered = true;

        // Disable Phase 1
        if (phase1Script != null)
            phase1Script.enabled = false;

        // Enable Phase 2
        if (phase2Script != null)
            phase2Script.enabled = true;

        Debug.Log("Knight Boss Phase 2 triggered!");
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

    public int GetCurrentHealth() => currentHealth;
    public float GetHealthPercent() => currentHealth / (float)maxHealth;
    public bool IsPhase2() => phase2Triggered;
}
