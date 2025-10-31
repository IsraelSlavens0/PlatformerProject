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

    [HideInInspector] public bool isRunningAway = false;
    private float runAwayTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform playerTransform = playerObj.transform;

        if (isRunningAway)
            RunAway(playerTransform);
        else
            ChasePlayer(playerTransform);

        if (runAwayTimer > 0)
            runAwayTimer -= Time.deltaTime;
    }

    private void ChasePlayer(Transform playerTransform)
    {
        Vector2 direction = playerTransform.position - transform.position;
        float distance = direction.magnitude;

        if (distance < detectionRange)
        {
            rb.velocity = new Vector2(direction.normalized.x * chaseSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void StartRunningAway()
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

    public void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
}
