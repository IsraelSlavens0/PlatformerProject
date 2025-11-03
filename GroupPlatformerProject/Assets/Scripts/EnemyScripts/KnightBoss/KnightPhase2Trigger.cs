using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightPhase2Trigger : MonoBehaviour
{
    [Header("Phase Scripts")]
    public KnightBossPhase1Attacks phase1Attacks;
    public KnightBossPhase2Attacks phase2Attacks;

    [Header("Health Settings")]
    public int phase2HealthThreshold = 50; // Example: trigger at half HP
    public EnemyHealth enemyHealth;

    private bool phase2Triggered = false;

    private void Update()
    {
        if (phase2Triggered) return;

        if (enemyHealth != null && enemyHealth.health <= phase2HealthThreshold)
        {
            TriggerPhase2();
        }
    }

    private void TriggerPhase2()
    {
        phase2Triggered = true;

        // Disable Phase 1 attacks
        if (phase1Attacks != null)
        {
            phase1Attacks.enabled = false;
            Debug.Log("Phase 1 attacks disabled.");
        }

        // Enable Phase 2 attacks
        if (phase2Attacks != null)
        {
            phase2Attacks.enabled = true;
            Debug.Log("Phase 2 attacks enabled.");
        }
    }
}
