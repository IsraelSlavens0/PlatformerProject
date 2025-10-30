using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 10f;
    public float maxHealth;
    public Image healthBar;

    private Powerups powerups;

    private void Start()
    {
        maxHealth = health;
        powerups = GetComponent<Powerups>();
        UpdateHealthBar();
    }

    private void Update()
    {
        if (health > maxHealth) health = maxHealth;
        UpdateHealthBar();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (powerups != null && powerups.isInvincible) return;

        // Normal enemy collision
        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(1);
            return;
        }

        // Enemy bullets
        if (collision.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
            return;
        }

        // Health packs
        if (collision.CompareTag("HealthPack"))
        {
            Heal(1);
            Destroy(collision.gameObject);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (powerups != null && powerups.isInvincible) return;

        // Normal enemy collision
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
            return;
        }

        // KnightBoss collision (non-hitbox attacks)
        KnightBossAI boss = collision.gameObject.GetComponent<KnightBossAI>();
        if (boss != null)
        {
            float damage = 0f;
            string attack = boss.GetCurrentAttackName();

            switch (attack)
            {
                case "Basic":
                    damage = boss.basicAttack.damage;
                    break;
                case "Slam":
                    damage = boss.slamAttack.damage;
                    break;
            }

            if (boss.IsPowerBoosted())
            {
                damage *= 1.6f;
                boss.ConsumePowerBoost();
            }

            if (damage > 0)
                TakeDamage(damage);
        }

        // Health packs
        if (collision.gameObject.CompareTag("HealthPack"))
        {
            Heal(1);
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;

        health -= amount;
        if (health < 0) health = 0;

        Debug.Log($"Player took {amount} damage. Remaining health: {health}");
        UpdateHealthBar();

        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Heal(float amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = health / maxHealth;
    }
}
