using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 10;
    public float maxHealth;
    public Image healthBar;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !GetComponent<Powerups>().isInvincible)
        {
            health --;
            healthBar.fillAmount = health / maxHealth;
            if (health < -0)
            {
                //if health is too low reload the level
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        if (collision.gameObject.tag == "EnemyBullet")
        {
            health--;
            healthBar.fillAmount = health / maxHealth;
            if (health <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        KnightBossHitbox lunge = collision.GetComponent<KnightBossHitbox>();
        if (lunge != null && !GetComponent<Powerups>().isInvincible)
        {
            KnightBossAI boss = lunge.boss;
            if (boss != null)
            {
                float damage = boss.lungeAttack.damage;
                if (boss.IsPowerBoosted())
                {
                    damage *= 1.6f;
                    boss.ConsumePowerBoost();
                }
                TakeDamage(damage);
            }
        }

        // Take damage from enemy bullets
        if (collision.tag == "EnemyBullet")
        {
            TakeDamage(1);
        }
    }
    private void Start()
    {
        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
    }

    private void Update()
    {
        // Clamp health
        if (health > maxHealth) health = maxHealth;
        healthBar.fillAmount = health / maxHealth;
    }


        // Check for LungeHitbox from KnightBoss

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Take damage from normal enemies
        if (collision.gameObject.tag == "Enemy" && !GetComponent<Powerups>().isInvincible)
        {
            TakeDamage(1);
        }

        // Take damage from KnightBoss collisions
        KnightBossAI boss = collision.gameObject.GetComponent<KnightBossAI>();
        if (boss != null && !GetComponent<Powerups>().isInvincible)
        {
            float damage = 0f;
            string attackName = boss.GetCurrentAttackName();

            switch (attackName)
            {
                case "Basic":
                    damage = boss.basicAttack.damage;
                    break;
                case "Slam":
                    damage = boss.slamAttack.damage;
                    break;
                case "PowerBoost":
                    // PowerBoost itself doesn't deal damage, handled in next attack
                    return;
            }

            if (boss.IsPowerBoosted())
            {
                damage *= 1.6f;
                boss.ConsumePowerBoost();
            }

            TakeDamage(damage);
        }

        // Health pack logic
        if (collision.gameObject.tag == "HealthPack")
        {
            health++;
            if (health > maxHealth) health = maxHealth;
            healthBar.fillAmount = health / maxHealth;
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        healthBar.fillAmount = health / maxHealth;

        if (health <= 0)
        {
            // Reload scene instantly
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
