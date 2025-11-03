using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KnightBossPhase2Attacks : MonoBehaviour
{
    [System.Serializable]
    public class Attack
    {
        public string name = "Attack";
        public float duration = 1f;
        public float horizontalForce = 5f;
        public float verticalForce = 0f;
        public float damage = 10f;
        public float range = 3f; // Range of the attack
        public float knockback = 0f; // For heavy hits
    }

    [Header("Polearm Attacks")]
    public Attack poleSmash = new Attack { name = "PoleSmash", duration = 1f, damage = 25f, range = 2.5f, knockback = 3f };
    public Attack poleThrust = new Attack { name = "PoleThrust", duration = 0.8f, damage = 20f, range = 3.5f, knockback = 1f };
    public Attack flamingSlam = new Attack { name = "FlamingSlam", duration = 1.2f, damage = 15f, range = 3f, knockback = 1f };

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    private bool projectileFired = false;

    [Header("Cooldowns")]
    public float abilityCooldown = 5f;
    public float basicAttackCooldown = 2f;

    private float abilityTimer = 0f;
    private float basicAttackTimer = 0f;

    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private KnightBossMovement movement;
    private Transform player;
    public bool isAttacking = false;
    private string currentAttack = "";
    private float attackTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        movement = GetComponent<KnightBossMovement>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Fire one-time projectile if assigned
        if (!projectileFired && projectilePrefab != null && projectileSpawnPoint != null)
        {
            Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectileFired = true;
            Debug.Log("Phase 2: Fired one-time projectile!");
        }
    }

    private void Update()
    {
        abilityTimer -= Time.deltaTime;
        basicAttackTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (isAttacking)
            HandleAttack();

        if (!isAttacking)
            TryRandomAttack(player);
    }

    public void TryRandomAttack(Transform playerTransform)
    {
        if (isAttacking || playerTransform == null) return;

        if (abilityTimer <= 0)
        {
            Attack[] abilities = { poleSmash, poleThrust, flamingSlam };
            int roll = Random.Range(0, abilities.Length);
            StartAttack(abilities[roll], playerTransform);
            abilityTimer = abilityCooldown;
            basicAttackTimer = basicAttackCooldown;
        }
        else if (basicAttackTimer <= 0)
        {
            StartAttack(poleThrust, playerTransform); // Basic fallback
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

        if (movement != null)
            movement.enabled = false;

        // Move slightly forward for the attack
        rb.velocity = new Vector2(dir.x * attack.horizontalForce, rb.velocity.y);

        if (attack.name == "PoleSmash")
            StartCoroutine(PoleSmashRoutine(dir, attack));
        else if (attack.name == "FlamingSlam")
            StartCoroutine(FlamingSlamRoutine(attack));
        else
            StartCoroutine(EndAfterDuration(attack.duration));

        Debug.Log($"Phase 2: Performing {attack.name} (Damage: {attack.damage}, Range: {attack.range}, Knockback: {attack.knockback})");
    }

    private IEnumerator PoleSmashRoutine(Vector2 dir, Attack attack)
    {
        bool hitPlayer = false;
        float elapsed = 0f;

        while (elapsed < attack.duration)
        {
            rb.velocity = new Vector2(0, -attack.verticalForce); // smash downward
            elapsed += Time.deltaTime;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, attack.range, LayerMask.GetMask("Player"));
            if (hit != null && !hitPlayer)
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attack.damage);
                    hitPlayer = true;
                    Debug.Log($"PoleSmash hit player for {attack.damage} damage!");
                }
            }

            yield return null;
        }

        // If missed, small AOE damage
        if (!hitPlayer)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attack.range);
            foreach (Collider2D col in hits)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(attack.damage * 0.5f); // half damage for AOE
                        Debug.Log("PoleSmash missed direct hit but dealt AOE damage!");
                    }
                }
            }
        }

        rb.velocity = Vector2.zero;
        FinishAttack();
    }

    private IEnumerator FlamingSlamRoutine(Attack attack)
    {
        // Slam animation movement
        rb.velocity = new Vector2(0, -attack.verticalForce);

        yield return new WaitForSeconds(attack.duration);

        // Code-only AOE (no prefab)
        float aoeRadius = attack.range; // Use attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attack.damage);
                Debug.Log($"Flaming Slam hit player for {attack.damage} damage!");
            }
        }

        FinishAttack();
    }

    private IEnumerator EndAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        FinishAttack();
    }

    private void HandleAttack()
    {
        if (attackTimer <= 0)
            FinishAttack();
    }

    private void FinishAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;
        currentAttack = "";
        if (movement != null)
            movement.enabled = true;
    }
}
