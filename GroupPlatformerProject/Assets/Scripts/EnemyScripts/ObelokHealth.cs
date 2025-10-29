using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObelokHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 30f;
    private float maxHealth;

    [Header("UI Elements")]
    public Image healthBar; // Assign a UI Image in the inspector

    private ThiefAI thiefAI;

    [Header("Drop Settings")]
    public GameObject dropPrefab;
    public Vector3 dropOffset = Vector3.zero;

    void Start()
    {
        thiefAI = GetComponent<ThiefAI>();

        // Initialize health values
        maxHealth = health;

        // Initialize health bar fill amount
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
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

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        // Update the health bar fill
        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Drop stolen coins if this enemy is a thief
        if (thiefAI != null)
        {
            thiefAI.DropStolenCoins();
        }

        // Drop prefab if assigned
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position + dropOffset, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
