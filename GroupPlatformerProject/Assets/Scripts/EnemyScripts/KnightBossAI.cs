using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightBossAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 5f;
    public float runAwaySpeed = 4f;
    public float attackRange = 2f;
    public float detectionRange = 10f;

    [Header("Attack Settings")]
    public float abilityCooldown = 5f;
    public float basicAttackCooldown = 2f;

    private Rigidbody2D rb;
    private Transform player;

    // State
    private bool isAttacking = false;
    private bool isRunningAway = false;
    private string currentAttack = "";
    private float attackTimer = 0f;

    // Timers
    private float abilityTimer = 0f;
    private float basicAttackTimer = 0f;
    private float runAwayTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find player automatically
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
            player = foundPlayer.transform;
        else
            Debug.LogWarning("KnightEnemy2D: No player found with tag 'Player'!");
    }

    private void Update()
    {
        if (player == null) return;

        abilityTimer -= Time.deltaTime;
        basicAttackTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        runAwayTimer -= Time.deltaTime;

        if (isAttacking)
        {
            HandleAttack();
        }
        else if (isRunningAway)
        {
            RunAway();
        }
        else
        {
            HandleChaseAndDecision();
        }
    }

    private void HandleChaseAndDecision()
    {
        Vector2 direction = (player.position - transform.position);
        float distance = direction.magnitude;

        if (distance < detectionRange)
        {
            // Chase continuously
            if (!isAttacking && !isRunningAway)
            {
                float verticalVel = rb.velocity.y;
                Vector2 moveDir = direction.normalized;
                rb.velocity = new Vector2(moveDir.x * chaseSpeed, verticalVel);

                // Flip sprite
                if (moveDir.x > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (moveDir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }

            // Decide attack or run-away if close
            if (distance <= attackRange)
            {
                // Stop horizontal movement but keep vertical (gravity)
                rb.velocity = new Vector2(0, rb.velocity.y);

                int decision = Random.Range(0, 10);
                if (decision >= 8)
                    StartRunningAway();
                else
                    TryAttack();
            }
        }
        else
        {
            // Outside detection range: stop horizontal but keep vertical
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void StartRunningAway()
    {
        isRunningAway = true;
        runAwayTimer = Random.Range(1f, 2f);
        Debug.Log("Knight is running away!");
    }

    private void RunAway()
    {
        if (runAwayTimer <= 0)
        {
            isRunningAway = false;
            return;
        }

        Vector2 direction = (transform.position - player.position).normalized;
        rb.velocity = new Vector2(direction.x * runAwaySpeed, rb.velocity.y);

        // Flip sprite
        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void TryAttack()
    {
        if (abilityTimer <= 0)
        {
            int roll = Random.Range(0, 4); // 0–2 = ability, 3 = basic

            if (roll == 0) StartAttack("Lunge", 1f);
            else if (roll == 1) StartAttack("Slam", 1.2f);
            else if (roll == 2) StartAttack("Charge", 1.5f);
            else if (basicAttackTimer <= 0) StartAttack("Basic", 0.8f);

            abilityTimer = abilityCooldown;
            basicAttackTimer = basicAttackCooldown;
        }
        else if (basicAttackTimer <= 0)
        {
            StartAttack("Basic", 0.8f);
            basicAttackTimer = basicAttackCooldown;
        }
    }

    private void StartAttack(string type, float duration)
    {
        isAttacking = true;
        currentAttack = type;
        attackTimer = duration;

        rb.velocity = new Vector2(0, rb.velocity.y); // horizontal stops, vertical keeps gravity
        Debug.Log($"Knight uses {type}!");

        Vector2 dir = (player.position - transform.position).normalized;

        if (type == "Lunge")
        {
            rb.AddForce(new Vector2(dir.x * 15f, 0), ForceMode2D.Impulse); // horizontal lunge
        }
        else if (type == "Charge")
        {
            rb.velocity = new Vector2(dir.x * 10f, rb.velocity.y);
        }
        else if (type == "Slam")
        {
            // Jump upward with a horizontal push toward player
            float jumpForce = 12f; // vertical
            float horizontalPush = dir.x * 5f; // small horizontal movement
            rb.velocity = new Vector2(horizontalPush, jumpForce);
        }
    }

    private void HandleAttack()
    {
        if (attackTimer > 0)
        {
            Vector2 dir = player.position - transform.position;
            if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);

            if (currentAttack == "Charge")
            {
                Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                rb.MovePosition(rb.position + new Vector2(forward.x * 10f * Time.deltaTime, rb.velocity.y * Time.deltaTime));
            }
            else if (currentAttack == "Slam")
            {
                // Wait until enemy is falling near the ground to land Slam
                if (rb.velocity.y <= 0)
                {
                    attackTimer = 0; // trigger landing
                }
            }
        }
        else
        {
            FinishAttack();
        }
    }

    private void FinishAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < attackRange + 0.3f)
        {
            switch (currentAttack)
            {
                case "Basic": Debug.Log("Player hit by Basic Attack! (10 dmg)"); break;
                case "Lunge": Debug.Log("Player hit by Lunge! (20 dmg)"); break;
                case "Slam": Debug.Log("Player hit by Slam! (25 dmg)"); break;
                case "Charge": Debug.Log("Player hit by Charge! (30 dmg)"); break;
            }
        }

        currentAttack = "";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
