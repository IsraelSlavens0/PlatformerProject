using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefEnemy : MonoBehaviour
{
    public enum AIState
    {
        Patrolling,
        ChasingPlayer,
        SeekingDroppedCoins
    }

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 4.5f;
    public Transform[] patrolPoints;
    int patrolIndex = 0;

    [Header("Detection")]
    public float detectionRadius = 6f;

    [Header("Attack / Steal")]
    public float attackCooldown = 1.2f;
    float attackTimer = 0f;
    public int stealAmount = 3;

    Rigidbody2D rb;
    Transform player;

    public int thiefCoins = 0;

    public float droppedCoinsSeekDelay = 0.5f;
    float droppedCoinsSeekTimer = 0f;

    public GameObject coinPrefab;

    AIState currentState = AIState.Patrolling;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // State transitions based on detection and timers
        switch (currentState)
        {
            case AIState.SeekingDroppedCoins:
                droppedCoinsSeekTimer -= Time.deltaTime;
                if (droppedCoinsSeekTimer <= 0f)
                {
                    if (FindClosestCoin() == null)
                    {
                        // No coins to seek, switch to chasing player
                        currentState = AIState.ChasingPlayer;
                    }
                }
                break;

            case AIState.ChasingPlayer:
                if (player == null)
                {
                    currentState = AIState.Patrolling;
                }
                else
                {
                    float dist = Vector2.Distance(transform.position, player.position);
                    if (dist > detectionRadius)
                    {
                        currentState = AIState.Patrolling;
                    }
                }
                break;

            case AIState.Patrolling:
                if (player != null)
                {
                    float dist = Vector2.Distance(transform.position, player.position);
                    if (dist <= detectionRadius)
                    {
                        currentState = AIState.ChasingPlayer;
                    }
                }
                break;
        }
    }

    void FixedUpdate()
    {
        Vector2 pos = rb.position;
        Vector2 velocity = Vector2.zero;  // Default velocity

        switch (currentState)
        {
            case AIState.SeekingDroppedCoins:
                if (droppedCoinsSeekTimer <= 0f)
                {
                    GameObject closestCoin = FindClosestCoin();
                    if (closestCoin != null)
                    {
                        Vector2 dir = ((Vector2)closestCoin.transform.position - pos).normalized;
                        velocity = dir * chaseSpeed;
                    }
                    else
                    {
                        // No coins found, switch to chasing player
                        currentState = AIState.ChasingPlayer;
                    }
                }
                else
                {
                    // During delay, don’t stand still — keep moving slowly forward or patrol to avoid stuck
                    velocity = Vector2.zero; // you can set a small patrol velocity here if you want
                }
                break;

            case AIState.ChasingPlayer:
                if (player != null)
                {
                    Vector2 dir = ((Vector2)player.position - pos).normalized;
                    velocity = dir * chaseSpeed;
                }
                else
                {
                    currentState = AIState.Patrolling;
                    velocity = Vector2.zero;
                }
                break;

            case AIState.Patrolling:
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    Vector2 targetPos = patrolPoints[patrolIndex].position;
                    Vector2 dir = targetPos - pos;
                    if (dir.magnitude < 0.2f)
                    {
                        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                    }
                    velocity = dir.normalized * moveSpeed;
                }
                else
                {
                    velocity = Vector2.zero;
                }
                break;
        }

        // Always set velocity in FixedUpdate!
        rb.velocity = velocity;
    }

    GameObject FindClosestCoin()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        GameObject closest = null;
        float bestDist = Mathf.Infinity;
        Vector2 pos = transform.position;

        foreach (var coin in coins)
        {
            float dist = Vector2.SqrMagnitude((Vector2)coin.transform.position - pos);
            if (dist < bestDist && dist <= detectionRadius * detectionRadius)
            {
                bestDist = dist;
                closest = coin;
            }
        }
        return closest;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            TrySteal(collision.collider.gameObject);
        }
        else if (collision.collider.CompareTag("Coin"))
        {
            TryPickupCoin(collision.collider.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            TryPickupCoin(other.gameObject);
        }
    }

    void TrySteal(GameObject playerObj)
    {
        if (attackTimer > 0f) return;
        attackTimer = attackCooldown;

        Collectables playerCollectables = playerObj.GetComponent<Collectables>();
        if (playerCollectables == null) return;

        int coinsToSteal = Mathf.Min(stealAmount, playerCollectables.coins);
        if (coinsToSteal <= 0) return;

        playerCollectables.coins -= coinsToSteal;

        for (int i = 0; i < coinsToSteal; i++)
        {
            Vector2 spawnPos = (Vector2)playerObj.transform.position + Random.insideUnitCircle * 0.5f;
            SpawnDroppedCoin(spawnPos);
        }

        // Switch to seeking dropped coins with delay
        currentState = AIState.SeekingDroppedCoins;
        droppedCoinsSeekTimer = droppedCoinsSeekDelay;
    }

    void SpawnDroppedCoin(Vector2 position)
    {
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Coin Prefab is not assigned on ThiefEnemy!");
        }
    }

    void TryPickupCoin(GameObject coin)
    {
        thiefCoins++;
        Destroy(coin);
    }

}
