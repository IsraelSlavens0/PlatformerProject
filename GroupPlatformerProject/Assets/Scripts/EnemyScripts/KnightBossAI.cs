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

    [System.Serializable]
    public class Attack
    {
        public string name = "Attack";
        public float duration = 1f;
        public float horizontalForce = 5f;
        public float verticalForce = 0f;
        public float damage = 10f;
    }

    [Header("Attack Settings")]
    public float abilityCooldown = 5f; // combined cooldown for abilities
    public float basicAttackCooldown = 2f;

    public Attack basicAttack = new Attack { name = "Basic", duration = 0.8f, damage = 10f };
    public Attack lungeAttack = new Attack { name = "Lunge", duration = 1f, horizontalForce = 15f, damage = 20f };
    public Attack chargeAttack = new Attack { name = "Charge", duration = 1f, horizontalForce = 20f, damage = 30f }; // faster charge
    public Attack slamAttack = new Attack { name = "Slam", duration = 1.5f, horizontalForce = 5f, verticalForce = 12f, damage = 25f };

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

    // Slam state
    private bool slamJumping = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
            if (!isAttacking && !isRunningAway)
            {
                float verticalVel = rb.velocity.y;
                Vector2 moveDir = direction.normalized;
                rb.velocity = new Vector2(moveDir.x * chaseSpeed, verticalVel);

                if (moveDir.x > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (moveDir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }

            if (distance <= attackRange)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);

                int decision = Random.Range(0, 10);
                if (decision >= 8)
                    StartRunningAway();
                else
                    TryRandomAttack();
            }
        }
        else
        {
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

        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void TryRandomAttack()
    {
        if (abilityTimer <= 0)
        {
            Attack[] abilities = { lungeAttack, slamAttack, chargeAttack, basicAttack };
            int roll = Random.Range(0, abilities.Length);
            StartAttack(abilities[roll]);

            abilityTimer = abilityCooldown;
            basicAttackTimer = basicAttackCooldown;
        }
        else if (basicAttackTimer <= 0)
        {
            StartAttack(basicAttack);
            basicAttackTimer = basicAttackCooldown;
        }
    }

    private void StartAttack(Attack attack)
    {
        isAttacking = true;
        currentAttack = attack.name;
        attackTimer = attack.duration;

        rb.velocity = new Vector2(0, rb.velocity.y);
        Debug.Log($"Knight uses {attack.name}!");

        Vector2 dir = (player.position - transform.position).normalized;

        if (attack.name == "Lunge")
        {
            rb.AddForce(new Vector2(attack.horizontalForce * dir.x, 0), ForceMode2D.Impulse);
        }
        else if (attack.name == "Charge")
        {
            // Make Charge fast and pierce through player
            rb.velocity = new Vector2(attack.horizontalForce * Mathf.Sign(dir.x), 0);
        }
        else if (attack.name == "Slam")
        {
            slamJumping = true;
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, attack.verticalForce);
        }
    }

    private void HandleAttack()
    {
        Vector2 dir = player.position - transform.position;
        if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        if (currentAttack == "Charge")
        {
            // Continue moving fast through the player without stopping
            Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            rb.MovePosition(rb.position + forward * chargeAttack.horizontalForce * Time.deltaTime);
        }
        else if (currentAttack == "Slam" && slamJumping)
        {
            if (rb.velocity.y <= 0)
            {
                float horizontalDir = dir.x;
                rb.velocity = new Vector2(horizontalDir * slamAttack.horizontalForce, -20f);
                slamJumping = false;
            }
        }

        if (attackTimer <= 0)
        {
            FinishAttack();
        }
    }

    private void FinishAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;
        slamJumping = false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < attackRange + 0.3f)
        {
            Attack current = null;
            switch (currentAttack)
            {
                case "Basic": current = basicAttack; break;
                case "Lunge": current = lungeAttack; break;
                case "Slam": current = slamAttack; break;
                case "Charge": current = chargeAttack; break;
            }

            if (current != null)
                Debug.Log($"Player hit by {current.name}! ({current.damage} dmg)");
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
