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

        // Lunge or other hitboxes
        KnightBossHitbox hitbox = collision.GetComponent<KnightBossHitbox>();
        if (hitbox != null)
        {
            TakeDamage(hitbox.damage);
        }

       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (powerups != null && powerups.isInvincible) return;

        // Health packs
        if (collision.gameObject.CompareTag("HealthPack"))
        {
            Heal(1);
            Destroy(collision.gameObject);
            return;
        }

        // Normal enemies
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
            return;
        }

       
        KnightBossPhase1Attacks bossPhase1 = collision.gameObject.GetComponent<KnightBossPhase1Attacks>();
        if (bossPhase1 != null)
        {
            HandleBossPhase1Damage(bossPhase1);
            return;
        }
    }

    private void HandleBossPhase1Damage(KnightBossPhase1Attacks bossPhase1)
    {
        string currentAttack = bossPhase1.GetCurrentAttackName();
        float damage = 0f;

        switch (currentAttack)
        {
            case "Basic":
                damage = bossPhase1.basicAttack.damage;
                break;
            case "Lunge":
                damage = bossPhase1.lungeAttack.damage;
                break;
            case "Slam":
                damage = bossPhase1.slamAttack.damage;
                break;
            case "PowerBoost":
                return;
        }

        // Apply Power Boost modifier
        if (bossPhase1.IsPowerBoosted())
        {
            damage *= 1.6f;
            bossPhase1.ConsumePowerBoost();
        }

        if (damage > 0)
        {
            TakeDamage(damage);
            Debug.Log($"Player took {damage} damage from KnightBoss {currentAttack}");
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
