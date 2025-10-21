using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamSkill : MonoBehaviour
{
    public int slamDamage = 30;
    public float slamRadius = 2.0f;
    public float slamManaCost = 40f;
    public float slamPushForce = 20f;  // new: downward push force

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

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(slamDamage);
            }
        }

        // Apply downward push to the player
        if (rb != null)
        {
            // Set the downward velocity instantly (overwrites current y velocity)
            rb.velocity = new Vector2(rb.velocity.x, -slamPushForce);
        }

        Debug.Log("Slam activated!");
        // Optional: add visual effects, sounds, etc.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector2 slamCenter = (Vector2)transform.position + Vector2.down * 1f;
        Gizmos.DrawSphere(slamCenter, slamRadius);
    }
}
