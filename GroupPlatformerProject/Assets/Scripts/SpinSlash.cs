using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinSlash : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinDuration = 1f;
    public float damageInterval = 0.2f;
    public float attackRadius = 1f;
    public int damage = 10;
    public float manaCost = 25f;

    private bool isSpinning = false;
    private float spinTimer = 0f;
    private float damageTimer = 0f;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            Debug.LogWarning("PlayerController component not found!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isSpinning)
        {
            if (playerController != null && playerController.currentMana >= manaCost)
            {
                playerController.SpendMana(manaCost);
                playerController.UpdateManaUI();
                StartSpin();
            }
            else
            {
                Debug.Log("Not enough mana to perform Spin Slash!");
            }
        }

        if (isSpinning)
        {
            Spin();
        }
    }

    void StartSpin()
    {
        isSpinning = true;
        spinTimer = spinDuration;
        damageTimer = 0f;
        Debug.Log("Spin started!");
    }

    void Spin()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f)
        {
            DetectAndDamageEnemies();
            damageTimer = damageInterval;
        }

        spinTimer -= Time.deltaTime;
        if (spinTimer <= 0f)
        {
            isSpinning = false;
            Debug.Log("Spin ended!");
        }
    }

    void DetectAndDamageEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);

        if (hits.Length == 0)
        {
            Debug.Log("No enemies in range.");
            return;
        }

        foreach (Collider2D collider in hits)
        {
            if (collider.CompareTag("Enemy"))
            {
                // Check if enemy is strictly to the left or right of player
                float directionToEnemy = collider.transform.position.x - transform.position.x;

                if (directionToEnemy > 0)
                {
                    // Enemy is on the right side
                    DealDamage(collider);
                }
                else if (directionToEnemy < 0)
                {
                    // Enemy is on the left side
                    DealDamage(collider);
                }
                // If directionToEnemy == 0, enemy is exactly aligned on X axis, ignore
            }
        }
    }

    void DealDamage(Collider2D enemyCollider)
    {
        EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"Dealt {damage} damage to {enemyCollider.name}");
        }
        else
        {
            Debug.LogWarning($"EnemyHealth component missing on {enemyCollider.name}");
        }
    }
}
