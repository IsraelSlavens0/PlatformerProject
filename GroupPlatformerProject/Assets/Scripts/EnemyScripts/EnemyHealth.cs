using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 30;

    private ThiefAI thiefAI;

    // PUBLIC DROP SETTINGS - edit these in the Inspector
    [Header("Drop Settings")]
    [Tooltip("Array of prefabs that can be dropped when the enemy dies")]
    public GameObject[] dropPrefabs;

    [Tooltip("Minimum number of objects to drop (inclusive)")]
    public int minDropCount = 1;

    [Tooltip("Maximum number of objects to drop (inclusive)")]
    public int maxDropCount = 3;

    [Tooltip("Offset applied to each dropped object relative to the enemy position")]
    public Vector3 dropOffset = Vector3.zero;

    void Start()
    {
        thiefAI = GetComponent<ThiefAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When hit by a player bullet
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // If this enemy is a thief, drop stolen coins
        if (thiefAI != null)
        {
            thiefAI.DropStolenCoins();
        }

        // DROP LOGIC
        if (dropPrefabs != null && dropPrefabs.Length > 0)
        {
            // Clamp the range so we never get negative or inverted values
            int dropCount = Random.Range(
                Mathf.Max(0, minDropCount),
                Mathf.Max(1, maxDropCount + 1)   // +1 because Range max is exclusive
            );

            for (int i = 0; i < dropCount; i++)
            {
                // Pick a random prefab from the array
                GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];

                // Spawn it with the optional offset
                Instantiate(prefab, transform.position + dropOffset, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}