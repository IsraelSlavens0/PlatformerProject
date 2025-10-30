using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
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
    public float abilityCooldown = 5f;
    public float basicAttackCooldown = 2f;

    public Attack basicAttack = new Attack { name = "Basic", duration = 0.8f, damage = 10f };
    public Attack lungeAttack = new Attack { name = "Lunge", duration = 1f, horizontalForce = 15f, damage = 20f };
    public Attack slamAttack = new Attack { name = "Slam", duration = 1.5f, horizontalForce = 5f, verticalForce = 12f, damage = 25f };
    public Attack powerBoostAttack = new Attack { name = "PowerBoost", duration = 0.8f, damage = 0f };

    [Header("References")]
    public KnightBossHitbox lungeHitbox; // Assign in inspector

    private Rigidbody2D rb;
    private Collider2D bossCollider;

    // State
    private bool isAttacking = false;
    private bool isRunningAway = false;
    private string currentAttack = "";
    private float attackTimer = 0f;
    private bool slamJumping = false;
    private bool powerBoostActive = false;

    // Timers
    private float abilityTimer = 0f;
    private float basicAttackTimer = 0f;
    private float runAwayTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();

        if (lungeHitbox != null)
            lungeHitbox.boss = this;
    }

    private void Update()
    {
        abilityTimer -= Time.deltaTime;
        basicAttackTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        runAwayTimer -= Time.deltaTime;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;
        Transform playerTransform = playerObj.transform;

        if (isAttacking)
            HandleAttack();
        else if (isRunningAway)
            RunAway(playerTransform);
        else
            HandleChaseAndDecision(playerTransform);
    }

    private void HandleChaseAndDecision(Transform playerTransform)
    {
        Vector2 direction = (playerTransform.position - transform.position);
        float distance = direction.magnitude;

        if (distance < detectionRange)
        {
            if (!isAttacking && !isRunningAway)
                rb.velocity = new Vector2(direction.normalized.x * chaseSpeed, rb.velocity.y);

            if (distance <= attackRange)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);

                int decision = Random.Range(0, 10);
                if (decision >= 8)
                    StartRunningAway();
                else
                    TryRandomAttack(playerTransform);
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
    }

    private void RunAway(Transform playerTransform)
    {
        if (runAwayTimer <= 0)
        {
            isRunningAway = false;
            return;
        }

        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.velocity = new Vector2(direction.x * runAwaySpeed, rb.velocity.y);
    }

    private void TryRandomAttack(Transform playerTransform)
    {
        if (abilityTimer <= 0)
        {
            Attack[] abilities = { lungeAttack, slamAttack, powerBoostAttack, basicAttack };
            int roll = Random.Range(0, abilities.Length);
            StartAttack(abilities[roll], playerTransform);

            abilityTimer = abilityCooldown;
            basicAttackTimer = basicAttackCooldown;
        }
        else if (basicAttackTimer <= 0)
        {
            StartAttack(basicAttack, playerTransform);
            basicAttackTimer = basicAttackCooldown;
        }
    }

    private void StartAttack(Attack attack, Transform playerTransform)
    {
        isAttacking = true;
        currentAttack = attack.name;
        attackTimer = attack.duration;

        Vector2 dir = (playerTransform.position - transform.position).normalized;

        if (attack.name == "Lunge")
        {
            // Ignore collisions with player while lunging
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            if (playerCollider != null)
                Physics2D.IgnoreCollision(bossCollider, playerCollider, true);

            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(attack.horizontalForce * dir.x, 0), ForceMode2D.Impulse);

            if (lungeHitbox != null)
            {
                lungeHitbox.damage = attack.damage;
                lungeHitbox.Activate();
            }
        }
        else if (attack.name == "Slam")
        {
            slamJumping = true;
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, attack.verticalForce);
        }
        else if (attack.name == "PowerBoost")
        {
            powerBoostActive = true;
        }
        else if (attack.name == "Basic")
        {
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, rb.velocity.y);
        }
    }

    private void HandleAttack()
    {
        if (currentAttack == "Slam" && slamJumping)
        {
            if (rb.velocity.y <= 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -20f);
                slamJumping = false;
            }
        }

        if (attackTimer <= 0)
            FinishAttack();
    }

    private void FinishAttack()
    {
        // Re-enable collision with player after lunge
        if (currentAttack == "Lunge")
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
                if (playerCollider != null)
                    Physics2D.IgnoreCollision(bossCollider, playerCollider, false);
            }
        }

        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;
        slamJumping = false;
        currentAttack = "";
    }

    public bool IsPowerBoosted() => powerBoostActive;
    public void ConsumePowerBoost() => powerBoostActive = false;
    public string GetCurrentAttackName() => currentAttack;
}
