using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KnightBossPhase1Attacks : MonoBehaviour
{
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
    public KnightBossHitbox lungeHitbox;

    private Rigidbody2D rb;
    private Collider2D bossCollider;

    // State
    [HideInInspector] public bool isAttacking = false;
    private string currentAttack = "";
    private float attackTimer = 0f;
    private bool slamJumping = false;
    private bool powerBoostActive = false;

    private Vector2 lungeDirection;

    // Timers
    private float abilityTimer = 0f;
    private float basicAttackTimer = 0f;

    private KnightBossMovement movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        movement = GetComponent<KnightBossMovement>();

        if (lungeHitbox != null)
            lungeHitbox.bossReference = this;
    }

    private void Update()
    {
        abilityTimer -= Time.deltaTime;
        basicAttackTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (isAttacking)
            HandleAttack();
    }

    public void TryRandomAttack(Transform playerTransform)
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
            rb.velocity = Vector2.zero;

            // Lock the direction toward the player at start
            lungeDirection = dir;

            // Apply horizontal impulse toward the player
            rb.AddForce(new Vector2(lungeAttack.horizontalForce * lungeDirection.x, 0), ForceMode2D.Impulse);

            // Temporarily ignore collision with player
            StartCoroutine(TemporaryIgnorePlayerCollision(0.15f));

            Debug.Log($"Knight Boss lunging toward player!");
        }
        else if (attack.name == "Slam")
        {
            slamJumping = true;
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, attack.verticalForce);
            Debug.Log("Knight Boss performing Slam!");
        }
        else if (attack.name == "PowerBoost")
        {
            powerBoostActive = true;
        Debug.Log("Knight Boss activating PowerBoost!");
        }
        else if (attack.name == "Basic")
        {
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, rb.velocity.y);
            Debug.Log("Knight Boss performing Basic Attack!");
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

        // Maintain lunge velocity if still lunging
        if (currentAttack == "Lunge")
        {
            rb.velocity = new Vector2(lungeDirection.x * lungeAttack.horizontalForce, rb.velocity.y);
        }

        if (attackTimer <= 0)
            FinishAttack();
    }

    private void FinishAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;
        slamJumping = false;
        currentAttack = "";
    }

    private IEnumerator TemporaryIgnorePlayerCollision(float duration)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, playerCollider, true);
                yield return new WaitForSeconds(duration);
                Physics2D.IgnoreCollision(bossCollider, playerCollider, false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        float dmg = 0f;

        if (currentAttack == "Slam")
            dmg = slamAttack.damage;
        else if (currentAttack == "Lunge")
            dmg = lungeAttack.damage;
        else
            return;

        if (powerBoostActive)
        {
            dmg *= 1.6f;
            powerBoostActive = false;
        }

        playerHealth.TakeDamage(dmg);
        Debug.Log($"💥 {currentAttack} hit player for {dmg} damage!");

        FinishAttack();
    }

    public bool IsPowerBoosted() => powerBoostActive;
    public void ConsumePowerBoost() => powerBoostActive = false;
    public string GetCurrentAttackName() => currentAttack;
}
