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
        if (collision.gameObject.tag == "HealthPack")
        {
            Heal(1);
            Destroy(collision.gameObject);
            return;
        }

        // Normal enemy collision
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
            return;
        }

        // Health packs
        if (collision.gameObject.CompareTag("HealthPack"))
        {
            Heal(1);
            Destroy(collision.gameObject);
            return;
        }

        // Check for KnightBossAI
        KnightBossAI bossAI = collision.gameObject.GetComponent<KnightBossAI>();
        if (bossAI != null)
        {
            HandleBossDamage(
                bossAI.GetCurrentAttackName(),
                bossAI.basicAttack.damage,
                bossAI.slamAttack.damage,
                bossAI.IsPowerBoosted(),
                bossAI.ConsumePowerBoost
            );
            return;
        }

        // Check for KnightBossPhase1Attacks
        KnightBossPhase1Attacks bossPhase1 = collision.gameObject.GetComponent<KnightBossPhase1Attacks>();
        if (bossPhase1 != null)
        {
            HandleBossDamage(
                bossPhase1.GetCurrentAttackName(),
                bossPhase1.basicAttack.damage,
                bossPhase1.slamAttack.damage,
                bossPhase1.IsPowerBoosted(),
                bossPhase1.ConsumePowerBoost
            );
        }
    }

    private void HandleBossDamage(string currentAttack, float basicDamage, float slamDamage, bool isBoosted, System.Action consumeBoost)
    {
        float damage = 0f;

        switch (currentAttack)
        {
            case "Basic":
                damage = basicDamage;
                break;
            case "Slam":
                damage = slamDamage;
                break;
        }

        if (isBoosted)
        {
            damage *= 1.6f;
            consumeBoost?.Invoke();
        }

        if (damage > 0)
            TakeDamage(damage);
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
