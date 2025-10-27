using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 3f;
    public float chaseTriggerDistance = 5f;
    public bool returnHome = true;

    [Header("Patrol Settings")]
    public bool patrol = true;
    public Vector3 patrolDirection = Vector3.right;
    public float patrolDistance = 3f;
    public float groundCheckDistance = 0.5f; // How far ahead to check for ground

    [Header("Stealing Settings")]
    public int coinsToSteal = 2;
    public float damageToPlayer = 1f;
    public float stealDistance = 1.5f;
    public float stealCooldown = 5f;

    [Header("Coin Drop Settings")]
    public GameObject coinPrefab; // Assign your coin prefab here
    public float coinDropSpread = 0.5f;

    private GameObject player;
    private Rigidbody2D rb;
    private Vector3 home;
    private bool isHome = true;
    private bool isReturningHome = false;
    private bool hasStolen = false;
    private float lastStealTime = -Mathf.Infinity;

    private int stolenCoins = 0;

    private GameObject targetCoin; // Current coin target to pick up

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        home = transform.position;

        rb.freezeRotation = true;
        rb.gravityScale = 1f; // Enable gravity

        patrolDirection.Normalize();
    }

    void Update()
    {
        if (player == null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        if (hasStolen)
        {
            RunAwayFromPlayer();
            return;
        }

        Vector3 toPlayer = player.transform.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // Steal coins from player if close enough and cooldown done
        if (distanceToPlayer <= stealDistance && Time.time >= lastStealTime + stealCooldown)
        {
            StealFromPlayer();
            return;
        }

        // Otherwise try to pick up world coins
        FindClosestCoin();

        if (targetCoin != null)
        {
            MoveToCoin();
            return;
        }

        // Chase player if close enough but not stealing
        if (distanceToPlayer < chaseTriggerDistance)
        {
            ChasePlayer(toPlayer);
        }
        else if (returnHome && !isHome)
        {
            ReturnHome();
        }
        else if (patrol && isHome)
        {
            Patrol();
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void StealFromPlayer()
    {
        Collectables playerCollect = player.GetComponent<Collectables>();
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerCollect != null && playerCollect.coins > 0)
        {
            int stolenAmount = Mathf.Min(coinsToSteal, playerCollect.coins);
            playerCollect.coins -= stolenAmount;
            stolenCoins += stolenAmount;
            Debug.Log($"Thief stole {stolenAmount} coins from player!");
        }

        if (playerHealth != null)
        {
            playerHealth.health -= damageToPlayer;
            playerHealth.healthBar.fillAmount = playerHealth.health / playerHealth.maxHealth;
            Debug.Log($"Thief damaged player for {damageToPlayer} health!");
        }

        hasStolen = true;
        lastStealTime = Time.time;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void FindClosestCoin()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");

        GameObject closest = null;
        float closestDist = Mathf.Infinity;

        foreach (GameObject coin in coins)
        {
            float dist = Vector3.Distance(transform.position, coin.transform.position);

            if (dist < closestDist && dist <= chaseTriggerDistance)
            {
                closestDist = dist;
                closest = coin;
            }
        }

        targetCoin = closest;
    }

    void MoveToCoin()
    {
        if (targetCoin == null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        Vector3 toCoin = targetCoin.transform.position - transform.position;
        float distToCoin = toCoin.magnitude;

        if (distToCoin <= stealDistance)
        {
            PickUpCoin();
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        else
        {
            // Only move if there's ground ahead
            if (IsGroundAhead(toCoin.normalized.x))
                rb.velocity = new Vector2(toCoin.normalized.x * chaseSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(0, rb.velocity.y);

            isHome = false;
            isReturningHome = false;
        }
    }

    void PickUpCoin()
    {
        if (targetCoin == null) return;

        Destroy(targetCoin);
        stolenCoins++;
        Debug.Log("Thief picked up a coin!");

        if (stolenCoins >= coinsToSteal)
        {
            hasStolen = true;
            lastStealTime = Time.time;
        }

        targetCoin = null;
    }

    void RunAwayFromPlayer()
    {
        Vector3 runDir = (transform.position - player.transform.position).normalized;

        if (IsGroundAhead(runDir.x))
            rb.velocity = new Vector2(runDir.x * chaseSpeed * 1.5f, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);

        if (Vector3.Distance(transform.position, player.transform.position) > chaseTriggerDistance * 2)
        {
            hasStolen = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void ChasePlayer(Vector3 toPlayer)
    {
        if (IsGroundAhead(toPlayer.normalized.x))
            rb.velocity = new Vector2(toPlayer.normalized.x * chaseSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);

        isHome = false;
        isReturningHome = false;
    }

    void ReturnHome()
    {
        Vector3 homeDir = home - transform.position;
        float distToHome = homeDir.magnitude;
        float stopThreshold = 0.1f;

        if (distToHome > stopThreshold)
        {
            float speed = chaseSpeed;
            if (distToHome < 1f)
                speed = Mathf.Lerp(0, chaseSpeed, distToHome / 1f);

            if (IsGroundAhead(homeDir.normalized.x))
                rb.velocity = new Vector2(homeDir.normalized.x * speed, rb.velocity.y);
            else
                rb.velocity = new Vector2(0, rb.velocity.y);

            isReturningHome = true;
            isHome = false;
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isHome = true;
            isReturningHome = false;
            transform.position = home;
        }
    }

    void Patrol()
    {
        Vector3 displacement = transform.position - home;
        float distance = displacement.magnitude;
        float buffer = 0.2f;

        if (distance > patrolDistance + buffer)
        {
            Vector3 clampedPos = home + displacement.normalized * patrolDistance;
            transform.position = clampedPos;

            patrolDirection = -patrolDirection;
            patrolDirection.Normalize();
        }

        if (IsGroundAhead(patrolDirection.x))
            rb.velocity = new Vector2(patrolDirection.x * chaseSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);
    }

    // Check if there's ground ahead
    bool IsGroundAhead(float dir)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * 0.1f;
        Vector2 direction = Vector2.right * Mathf.Sign(dir);

        RaycastHit2D hit = Physics2D.Raycast(origin + direction * 0.3f, Vector2.down, groundCheckDistance);

        // Uncomment to debug raycast
        // Debug.DrawRay(origin + direction * 0.3f, Vector2.down * groundCheckDistance, Color.red);

        return hit.collider != null;
    }

    public void DropStolenCoins()
    {
        if (stolenCoins <= 0) return;

        for (int i = 0; i < stolenCoins; i++)
        {
            Vector2 dropPos = (Vector2)transform.position + Random.insideUnitCircle * coinDropSpread;
            Instantiate(coinPrefab, dropPos, Quaternion.identity);
        }

        Debug.Log($"Thief dropped {stolenCoins} coins!");
        stolenCoins = 0;
    }
}
