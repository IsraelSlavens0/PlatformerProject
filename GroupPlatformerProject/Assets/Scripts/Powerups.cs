using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerups : MonoBehaviour
{
    // Duration of invincibility in seconds (editable in Inspector)
    public float invincibilityDuration = 10f;
    // Tracks whether the player is currently invincible
    public bool isInvincible = false;
    // Timer counting down remaining invincibility time
    private float invincibilityTimer = 0f;
    void Update()
    {
        // If player is invincible, reduce the timer by elapsed time
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
        
            // When timer reaches zero or below, end invincibility
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("Invincibility ended.");
                // TODO: Remove any invincibility visual/audio effects here
            }
        }
    }
    // Detect when player collides physically with something (used for enemies with solid colliders)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.tag == "MysteriousFragment")
        {
            //make the player invincible for [duration]
            StartInvincibility(invincibilityDuration); // Activate invincibility
            Destroy(collision.gameObject);             // Remove the pickup from the scene
        }
        else if (isInvincible && collision.gameObject.tag == "Enemy")
        {
            Destroy(collision.gameObject);             // Destroy the enemy
            Debug.Log("Enemy destroyed by invincible player.");
            // TODO: Add enemy death effects (particles, sounds) here
        }
        if (isInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);             // Destroy the enemy on contact
            Debug.Log("Enemy destroyed by invincible player.");
            // TODO: Add enemy death effects here
        }
        else if (!isInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player hit by enemy.");
            // TODO: Add player damage or knockback logic here
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);             // Destroy the enemy on contact
            Debug.Log("Enemy destroyed by invincible player.");
            // TODO: Add enemy death effects here
            
        }
        else if (!isInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player hit by enemy.");
            // TODO: Add player damage or knockback logic here
        }
    }

    // Activates invincibility for the specified duration
    private void StartInvincibility(float duration)
    {
        isInvincible = true;           // Set player as invincible
        invincibilityTimer = duration; // Reset timer to duration
        Debug.Log($"Invincibility started for {duration} seconds.");
        // TODO: Add visual/audio effects to show player is invincible
    }
}