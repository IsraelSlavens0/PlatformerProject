using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private KnightBossMovement movement;
    private Transform player;

    [HideInInspector] public bool isAttacking = false;
    private string currentAttack = "";
    private float attackTimer = 0f;
    private bool slamJumping = false;
    private bool powerBoostActive = false;

    private Vector2 lungeDirection;
    private float abilityTimer = 0f;
    private float basicAttackTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        movement = GetComponent<KnightBossMovement>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

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
        if (isAttacking) return;

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

    public void StartAttack(Attack attack, Transform playerTransform)
    {
        if (isAttacking) return;

        isAttacking = true;
        currentAttack = attack.name;
        attackTimer = attack.duration;

        Vector2 dir = (playerTransform.position - transform.position).normalized;

        // Disable normal movement during attacks
        if (movement != null)
            movement.enabled = false;

        if (attack.name == "Lunge")
        {
            StartCoroutine(DoLunge(dir));
        }
        else if (attack.name == "Slam")
        {
            StartCoroutine(DoSlam(playerTransform));
        }
        else if (attack.name == "PowerBoost")
        {
            powerBoostActive = true;
            Debug.Log("Knight Boss activating PowerBoost!");
            FinishAttack();
        }
        else if (attack.name == "Basic")
        {
            rb.velocity = new Vector2(attack.horizontalForce * dir.x, rb.velocity.y);
            Debug.Log("Knight Boss performing Basic Attack!");
            StartCoroutine(EndAfterDuration(attack.duration));
        }
    }


    private IEnumerator DoLunge(Vector2 dir)
    {
        Debug.Log("Knight Boss lunging toward player!");
        rb.velocity = Vector2.zero;

        // Temporarily disable main collider so the knight can pass through the player
        bossCollider.enabled = false;

        // Turn lunge hitbox on
        if (lungeHitbox != null)
        {
            lungeHitbox.gameObject.SetActive(true);
            lungeHitbox.Activate(lungeAttack);
        }

        // Variables for direct detection
        float elapsed = 0f;
        bool hasHitPlayer = false;

        // Start lunge motion
        while (elapsed < lungeAttack.duration)
        {
            rb.velocity = dir * lungeAttack.horizontalForce;
            elapsed += Time.deltaTime;

          
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.2f, LayerMask.GetMask("Default"));
            if (hit != null && hit.CompareTag("Player") && !hasHitPlayer)
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float finalDamage = lungeAttack.damage;
                    if (powerBoostActive)
                    {
                        finalDamage *= 1.6f;
                        powerBoostActive = false;
                    }

                    playerHealth.TakeDamage(finalDamage);
                    Debug.Log($"💥 Lunge directly hit player for {finalDamage} damage!");
                    hasHitPlayer = true;
                }
            }
 
            yield return null;
        }

        // Stop motion
        rb.velocity = Vector2.zero;

        // Restore collider
        bossCollider.enabled = true;

        // Disable hitbox
        if (lungeHitbox != null)
            lungeHitbox.gameObject.SetActive(false);

        FinishAttack();
    }

    private IEnumerator DoSlam(Transform playerTransform)
    {
        Debug.Log("Knight Boss performing Slam!");
        slamJumping = true;

        rb.velocity = new Vector2(0, slamAttack.verticalForce);
        yield return new WaitUntil(() => rb.velocity.y <= 0);

        Vector2 diveDir = (playerTransform.position - transform.position).normalized;
        rb.velocity = diveDir * (slamAttack.verticalForce + 8f);

        if (lungeHitbox != null)
            lungeHitbox.Activate(slamAttack);

        yield return new WaitForSeconds(0.4f);

        slamJumping = false;
        FinishAttack();
    }

    private IEnumerator EndAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        FinishAttack();
    }

    private void HandleAttack()
    {
        if (attackTimer <= 0 && !slamJumping)
            FinishAttack();
    }

    private void FinishAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;
        slamJumping = false;
        currentAttack = "";

        // Re-enable boss collider just in case
        if (bossCollider != null)
            bossCollider.enabled = true;

        if (movement != null)
            movement.enabled = true;
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

        if (powerBoostActive)
        {
            dmg *= 1.6f;
            powerBoostActive = false;
        }

        if (dmg > 0)
        {
            playerHealth.TakeDamage(dmg);
            Debug.Log($"💥 {currentAttack} hit player for {dmg} damage!");
        }

        FinishAttack();
    }

    public bool IsPowerBoosted() => powerBoostActive;
    public void ConsumePowerBoost() => powerBoostActive = false;
    public string GetCurrentAttackName() => currentAttack;
}
