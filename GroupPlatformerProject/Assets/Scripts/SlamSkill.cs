using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamSkill : MonoBehaviour
{
    public int slamDamage = 30;
    public float slamRadius = 2.0f;
    public float slamManaCost = 40f;
    public float slamPushForce = 20f;  // downward push force

    // -------------------------------
    // NEW: Animation Fields
    // -------------------------------
    [Header("Animation Settings")]
    [Tooltip("Animator component for playing the slam animation.")]
    public Animator animator;

    [Tooltip("Trigger name for slam animation (optional).")]
    public string slamTrigger = "Slam";

    [Tooltip("Optional animation clip to play instead of a trigger.")]
    public AnimationClip slamAnimationClip;

    private PlayerController playerController;
    private Rigidbody2D rb;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (playerController == null)
        {
            Debug.LogError("SlamSkill: PlayerController component not found on the same GameObject!");
        }
        if (rb == null)
        {
            Debug.LogError("SlamSkill: Rigidbody2D component not found on the same GameObject!");
        }

        // Auto-assign animator if missing
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySlam();
        }
    }

    void TrySlam()
    {
        if (playerController == null) return;

        if (playerController.currentMana >= slamManaCost)
        {
            Slam();
            playerController.SpendMana(slamManaCost);
            playerController.UpdateManaUI();

            // 🎞️ NEW: Play slam animation
            PlaySlamAnimation();
        }
        else
        {
            Debug.Log("Not enough mana for Slam!");
        }
    }

    void Slam()
    {
        Vector2 slamCenter = (Vector2)transform.position + Vector2.down * 1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(slamCenter, slamRadius);

        //just copy and paste the same bit but change the health script name
        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(slamDamage);
                continue; // avoid double-processing the same collider
            }

            KnightHealth KnightBoss = hit.GetComponent<KnightHealth>();
            if (KnightBoss != null)
            {
                KnightBoss.TakeDamage(slamDamage);
                Debug.Log($"SlamSkill dealt {slamDamage} damage to {KnightBoss.name}");
            }
        }

        // Apply downward push to the player
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, -slamPushForce);
        }

        Debug.Log("Slam activated!");
        // Optional: add visual effects, sounds, etc.
    }

    // ---------------------------------------
    // NEW: Animation Helper
    // ---------------------------------------
    void PlaySlamAnimation()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(slamTrigger))
        {
            animator.SetTrigger(slamTrigger);
        }
        else if (slamAnimationClip != null)
        {
            animator.Play(slamAnimationClip.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 slamCenter = (Vector2)transform.position + Vector2.down * 1f;
        Gizmos.DrawWireSphere(slamCenter, slamRadius);
    }
}
