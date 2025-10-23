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
        rb.gravityScale = 0f;

        patrolDirection.Normalize(); // Normalize once on start
    }

    void Update()
    {
        if (player == null)
        {
            rb.velocity = Vector2.zero;
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
            rb.velocity = toPlayer.normalized * chaseSpeed;
            isHome = false;
            isReturningHome = false;
        }
        else if (returnHome && !isHome)
        {
            ReturnHomeSmoothly();
        }
        else if (patrol && isHome)
        {
            Patrol();
        }
        else
        {
            rb.velocity = Vector2.zero;
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
        rb.velocity = Vector2.zero;
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
            rb.velocity = Vector2.zero;
            return;
        }

        Vector3 toCoin = targetCoin.transform.position - transform.position;
        float distToCoin = toCoin.magnitude;

        if (distToCoin <= stealDistance)
        {
            PickUpCoin();
            rb.velocity = Vector2.zero;
            return;
        }
        else
        {
            rb.velocity = toCoin.normalized * chaseSpeed;
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
        rb.velocity = runDir * chaseSpeed * 1.5f;

        if (Vector3.Distance(transform.position, player.transform.position) > chaseTriggerDistance * 2)
        {
            hasStolen = false;
            rb.velocity = Vector2.zero;
        }
    }

    void ReturnHomeSmoothly()
    {
        Vector3 homeDir = home - transform.position;
        float distToHome = homeDir.magnitude;
        float stopThreshold = 0.1f;

        if (distToHome > stopThreshold)
        {
            // Slow down when very close to home to avoid jitter
            float speed = chaseSpeed;
            if (distToHome < 1f)
            {
                speed = Mathf.Lerp(0, chaseSpeed, distToHome / 1f);
            }
            rb.velocity = homeDir.normalized * speed;
            isReturningHome = true;
            isHome = false;
        }
        else
        {
            // Snap velocity to zero and mark home reached only once inside threshold
            rb.velocity = Vector2.zero;
            isHome = true;
            isReturningHome = false;

            // Correct position precisely at home to prevent jitter
            transform.position = home;
        }
    }

    void Patrol()
    {
        Vector3 displacement = transform.position - home;
        float distance = displacement.magnitude;
        float buffer = 0.2f;

        // Flip direction only if beyond patrolDistance + buffer
        if (distance > patrolDistance + buffer)
        {
            // Clamp position to patrol boundary to avoid overshoot jitter
            Vector3 clampedPos = home + displacement.normalized * patrolDistance;
            transform.position = clampedPos;

            patrolDirection = -patrolDirection;
            patrolDirection.Normalize();
        }

        rb.velocity = patrolDirection * chaseSpeed;
    }

    // Called by EnemyHealth when enemy dies to drop stolen coins
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
