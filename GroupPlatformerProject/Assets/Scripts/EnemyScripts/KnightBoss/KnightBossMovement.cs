using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KnightBossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 5f;
    public float runAwaySpeed = 4f;
    public float attackRange = 2f;
    public float detectionRange = 10f;

    private Rigidbody2D rb;
    KnightBossPhase1Attacks p1Attacks;
    [HideInInspector] public bool isRunningAway = false;
    private float runAwayTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        p1Attacks = GetComponent<KnightBossPhase1Attacks>();
    }

    private void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform playerTransform = playerObj.transform;

        if (isRunningAway)
            RunAway(playerTransform);
        else
            HandleChaseAndDecision(playerTransform);

        if (runAwayTimer > 0)
            runAwayTimer -= Time.deltaTime;
    }

    private void HandleChaseAndDecision(Transform playerTransform)
    {
        Vector2 direction = (playerTransform.position - transform.position);
        float distance = direction.magnitude;

        if (distance < detectionRange)
        {
            if (!p1Attacks.isAttacking && !isRunningAway)
                rb.velocity = new Vector2(direction.normalized.x * chaseSpeed, rb.velocity.y);

            if (distance <= attackRange)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);

                int decision = Random.Range(0, 10);
                if (decision >= 8)
                    StartRunningAway();
                else
                    p1Attacks.TryRandomAttack(playerTransform);
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
}
