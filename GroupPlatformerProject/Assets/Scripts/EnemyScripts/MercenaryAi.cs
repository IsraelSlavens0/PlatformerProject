using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class MercenaryAI2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float detectionRange = 10f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public int attackDamage = 3;
    public float attackCooldown = 1f;

    private Rigidbody2D rb;
    private Transform player;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Automatically find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Player not found! Make sure your player has the tag 'Player'.");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Only move horizontally toward the player
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

            // Flip sprite based on direction
            if (direction != 0) transform.localScale = new Vector3(direction, 1, 1);

            // Attack if in range and cooldown passed
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Stop horizontal movement if player is far
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void AttackPlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.GetComponent<Powerups>().isInvincible)
        {
            playerHealth.health -= attackDamage;

            if (playerHealth.health < 0) playerHealth.health = 0;
            playerHealth.healthBar.fillAmount = playerHealth.health / playerHealth.maxHealth;

            if (playerHealth.health <= 0)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            Debug.Log($"Mercenary hits player for {attackDamage} damage!");
        }
    }

}
