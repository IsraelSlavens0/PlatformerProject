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
    private bool hasStolen = false;
    private float lastStealTime = -Mathf.Infinity;

    private int stolenCoins = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        home = transform.position;
    }

    void Update()
    {
        if (player == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector3 toPlayer = player.transform.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // Attempt steal if close enough and cooldown passed
        if (!hasStolen && distanceToPlayer <= stealDistance && Time.time >= lastStealTime + stealCooldown)
        {
            StealAndRun();
            rb.velocity = Vector2.zero;
            return;
        }

        if (!hasStolen)
        {
            // Normal movement: chase, return home, patrol
            if (distanceToPlayer < chaseTriggerDistance)
            {
                // Chase player
                Vector3 chaseDir = toPlayer.normalized;
                rb.velocity = chaseDir * chaseSpeed;
                isHome = false;
            }
            else if (returnHome && !isHome)
            {
                // Return home
                Vector3 homeDir = home - transform.position;
                if (homeDir.magnitude > 0.2f)
                {
                    rb.velocity = homeDir.normalized * chaseSpeed;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    isHome = true;
                }
            }
            else if (patrol)
            {
                // Patrol behavior
                Vector3 displacement = transform.position - home;
                if (displacement.magnitude > patrolDistance)
                {
                    patrolDirection = -displacement;
                }
                patrolDirection.Normalize();
                rb.velocity = patrolDirection * chaseSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // Run away after stealing
            Vector3 runDir = (transform.position - player.transform.position).normalized;
            rb.velocity = runDir * chaseSpeed * 1.5f;

            // Reset steal state if far enough away
            if (Vector3.Distance(transform.position, player.transform.position) > chaseTriggerDistance * 2)
            {
                hasStolen = false;
                rb.velocity = Vector2.zero;
            }
        }
    }

    void StealAndRun()
    {
        hasStolen = true;
        lastStealTime = Time.time;

        // Steal coins
        Collectables playerCollect = player.GetComponent<Collectables>();
        if (playerCollect != null && playerCollect.coins > 0)
        {
            int stolenAmount = Mathf.Min(coinsToSteal, playerCollect.coins);
            playerCollect.coins -= stolenAmount;
            stolenCoins += stolenAmount;
            Debug.Log($"Thief stole {stolenAmount} coins!");
        }

        // Damage player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.health -= damageToPlayer;
            playerHealth.healthBar.fillAmount = playerHealth.health / playerHealth.maxHealth;
            Debug.Log($"Thief damaged player for {damageToPlayer} health!");
        }
    }

    // Call this when the thief dies to drop stolen coins

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
