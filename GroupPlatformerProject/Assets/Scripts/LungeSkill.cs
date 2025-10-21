using UnityEngine;
using System.Collections.Generic;

public class LungeSkill : MonoBehaviour
{
    [Header("Lunge Settings")]
    public float lungeDistance = 5f;
    public float lungeSpeed = 20f;
    public float lungeDamage = 30f;
    public float lungeManaCost = 30f;
    public float lungeHitboxWidth = 1.2f;
    public float lungeHitboxHeight = 1.0f;
    public float lungeCooldown = 4f;

    private bool isLunging = false;
    private float lungeCooldownTimer = 0f;
    private Vector2 lungeTargetPosition;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private bool facingRight = true;

    // Timer for damage ticks during lunge
    private float damageTimer = 0f;
    public float damageInterval = 1f;  // damage every 1 second

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("LungeSkill requires PlayerController component on the same GameObject.");
        }
    }

    void Update()
    {
        if (lungeCooldownTimer > 0)
        {
            lungeCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isLunging && lungeCooldownTimer <= 0)
        {
            TryStartLunge();
        }

        if (isLunging)
        {
            PerformLunge();

            // Update damage timer
            damageTimer -= Time.deltaTime;

            if (damageTimer <= 0f)
            {
                DealLungeDamage();
                damageTimer = damageInterval;  // reset timer
            }
        }
    }

    void TryStartLunge()
    {
        if (playerController == null) return;

        if (playerController.currentMana >= lungeManaCost)
        {
            playerController.SpendMana(lungeManaCost);
            playerController.UpdateManaUI();

            StartLunge();
        }
        else
        {
            Debug.Log("Not enough mana to lunge!");
        }
    }

    void StartLunge()
    {
        isLunging = true;
        lungeCooldownTimer = lungeCooldown;
        facingRight = playerController.transform.localScale.x >= 0;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        lungeTargetPosition = (Vector2)transform.position + direction * lungeDistance;

        damageTimer = 0f;  // reset damage timer so damage triggers immediately
    }

    void PerformLunge()
    {
        Vector2 currentPosition = rb.position;

        Vector2 newPosition = Vector2.MoveTowards(currentPosition, lungeTargetPosition, lungeSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, lungeTargetPosition) < 0.1f)
        {
            isLunging = false;
        }
    }

    void DealLungeDamage()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        float halfWidth = lungeHitboxWidth / 2f;
        float clampedOffset = Mathf.Max(halfWidth, 0.6f);

        // Forward hitbox in front of player
        Vector2 forwardHitboxCenter = (Vector2)transform.position + direction * clampedOffset;

        // Center hitbox on player to cover close enemies (blind spot)
        Vector2 centerHitboxCenter = (Vector2)transform.position;

        // Hitbox sizes
        Vector2 forwardHitboxSize = new Vector2(lungeHitboxWidth, lungeHitboxHeight);
        Vector2 centerHitboxSize = new Vector2(lungeHitboxWidth * 0.6f, lungeHitboxHeight);

        // Check forward hitbox
        Collider2D[] forwardHits = Physics2D.OverlapBoxAll(forwardHitboxCenter, forwardHitboxSize, 0f);

        // Check center hitbox
        Collider2D[] centerHits = Physics2D.OverlapBoxAll(centerHitboxCenter, centerHitboxSize, 0f);

        // Combine hits to avoid double damage
        HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();

        foreach (var hit in forwardHits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemiesHit.Add(enemy);
            }
        }

        foreach (var hit in centerHits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemiesHit.Add(enemy);
            }
        }

        // Apply damage once per enemy
        foreach (EnemyHealth enemy in enemiesHit)
        {
            enemy.TakeDamage((int)lungeDamage);
            Debug.Log("Lunge dealt " + lungeDamage + " damage to " + enemy.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && isLunging)
        {
            Gizmos.color = Color.cyan;

            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            float halfWidth = lungeHitboxWidth / 2f;
            float clampedOffset = Mathf.Max(halfWidth, 0.6f);

            Vector2 forwardHitboxCenter = (Vector2)transform.position + direction * clampedOffset;
            Vector2 centerHitboxCenter = (Vector2)transform.position;

            Vector3 forwardHitboxSize = new Vector3(lungeHitboxWidth, lungeHitboxHeight, 0);
            Vector3 centerHitboxSize = new Vector3(lungeHitboxWidth * 0.6f, lungeHitboxHeight, 0);

            Gizmos.DrawWireCube(forwardHitboxCenter, forwardHitboxSize);
            Gizmos.DrawWireCube(centerHitboxCenter, centerHitboxSize);
        }
    }
}
