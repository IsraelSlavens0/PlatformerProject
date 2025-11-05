using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightPhase2Trigger : MonoBehaviour
{
    [Header("Phase Scripts")]
    public KnightBossPhase1Attacks phase1Attacks;
    public KnightBossPhase2Attacks phase2Attacks;
    public KnightBossMovement knightMovement; // ✅ Add this

    [Header("Phase Settings")]
    [Tooltip("Health percentage (0-1) at which Phase 2 should start.")]
    [Range(0f, 1f)]
    public float phase2Threshold = 0.5f;

    private bool phase2Triggered = false;

    private void Start()
    {
        InitializePhaseState();
        if (knightMovement == null)
            knightMovement = GetComponent<KnightBossMovement>();

    }

    // Called from KnightHealth.Start() and Start() here
    public void InitializePhaseState()
    {
        if (phase2Attacks != null)
            phase2Attacks.enabled = false;

        if (phase1Attacks != null)
            phase1Attacks.enabled = true;

        if (knightMovement != null)
            knightMovement.isPhase2 = false;

        phase2Triggered = false;
    }

    // Called from KnightHealth when health drops below threshold
    public bool ShouldTriggerPhase2(int currentHealth, int maxHealth)
    {
        return !phase2Triggered && currentHealth <= Mathf.CeilToInt(maxHealth * phase2Threshold);
    }

    public void TriggerPhase2()
    {
        if (phase2Triggered) return;
        phase2Triggered = true;

        // Disable Phase 1
        if (phase1Attacks != null)
        {
            phase1Attacks.enabled = false;
            Debug.Log("Knight Phase 1 attacks disabled.");
        }

        // Enable Phase 2
        if (phase2Attacks != null)
        {
            phase2Attacks.enabled = true;
            Debug.Log("Knight Phase 2 attacks enabled!");
        }
      
        if (knightMovement != null)
        {
            knightMovement.isPhase2 = true;
            Debug.Log("Knight movement switched to Phase 2 behavior!");
        }
    }
}
