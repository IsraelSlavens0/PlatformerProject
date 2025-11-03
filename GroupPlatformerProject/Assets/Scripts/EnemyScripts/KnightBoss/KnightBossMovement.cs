using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightBossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 3f;
    public float randomChangeIntervalMin = 1f;
    public float randomChangeIntervalMax = 3f;
    public float attackChance = 0.05f; // Chance per update to attack player

    [Header("Dash Settings")]
    public float dashDistance = 1f;
    public float dashCooldown = 3f;
    public float dashDuration = 0.2f;

    private Rigidbody2D rb;
    private KnightBossPhase1Attacks p1Attacks;
    private Transform player;

    private Vector2 randomDirection = Vector2.zero;
    private float directionTimer = 0f;
    private float dashTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        p1Attacks = GetComponent<KnightBossPhase1Attacks>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickNewDirection();
    }

    private void Update()
    {
        if (p1Attacks != null && p1Attacks.isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop moving during attacks
            return;
        }

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
            PickNewDirection();

        if (dashTimer > 0f)
            dashTimer -= Time.deltaTime;
        else if (Random.Range(0f, 1f) < 0.05f) // 5% chance per frame to dash
        {
            StartCoroutine(MicroDash());
            dashTimer = dashCooldown;
        }

        // Randomly try to attack the player
        if (player != null && Random.Range(0f, 1f) < attackChance)
        {
            p1Attacks.TryRandomAttack(player);
        }
    }

    private void FixedUpdate()
    {
        if (p1Attacks != null && p1Attacks.isAttacking)
            return; // Don't move during attacks

        rb.velocity = new Vector2(randomDirection.x * baseMoveSpeed, rb.velocity.y);
    }

    private void PickNewDirection()
    {
        // Completely random horizontal movement: left, right, or still
        randomDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
        directionTimer = Random.Range(randomChangeIntervalMin, randomChangeIntervalMax);
    }

    private IEnumerator MicroDash()
    {
        float elapsed = 0f;
        Vector2 dashDir = new Vector2(Random.Range(-1f, 1f), 0).normalized;
        float dashSpeed = baseMoveSpeed * 3f; // Dash speed multiplier

        while (elapsed < dashDuration)
        {
            rb.velocity = new Vector2(dashDir.x * dashSpeed, rb.velocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
