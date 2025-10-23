using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodePowerup : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffect; // Prefab for explosion effect (particle system, animation, etc.)
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionForce = 500f; // Force applied to nearby rigidbodies

    [Header("Powerup Timer Settings")]
    public float powerupDuration = 10f; // Time before powerup expires

    private bool powerupActive = false; // Is the powerup currently active?
    private bool powerupUsed = false;   // Has the powerup been used yet?
    private float timer = 0f;           // Tracks how long the powerup has been active

    void Update()
    {
        // Only track time and listen for input if active and not yet used
        if (powerupActive && !powerupUsed)
        {
            timer += Time.deltaTime;

            // Check if player pressed X to trigger explosion
            if (Input.GetKeyDown(KeyCode.X))
            {
                TriggerExplosion();
            }

            // If the time runs out without using it
            if (timer >= powerupDuration)
            {
                DeactivatePowerup();
            }
        }
    }

    // Detect when the player collides with a 2D trigger tagged "ExplosionPowerup"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ExplosionPowerup"))
        {
            ActivatePowerup();

            // Optionally destroy the powerup pickup object
            Destroy(collision.gameObject);

            Debug.Log("Picked up Explosion Powerup! You have 10 seconds to use it.");
        }
    }

    // Start the timer when player picks up the powerup
    void ActivatePowerup()
    {
        powerupActive = true;
        powerupUsed = false;
        timer = 0f;
    }

    // Handle the explosion when X is pressed
    void TriggerExplosion()
    {
        if (powerupUsed) return;

        powerupUsed = true;
        powerupActive = false;

        Debug.Log("?? Explosion triggered!");

        // Create explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply explosion force to nearby 2D rigidbodies
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D nearby in colliders)
        {
            Rigidbody2D rb = nearby.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Direction from explosion center to the object
                Vector2 direction = (rb.position - (Vector2)transform.position).normalized;
                float distance = Vector2.Distance(rb.position, transform.position);
                float force = Mathf.Lerp(explosionForce, 0, distance / explosionRadius);

                rb.AddForce(direction * force);
            }
        }

        // Reset timer after use
        timer = 0f;
    }

    // Called when time runs out without pressing X
    void DeactivatePowerup()
    {
        powerupActive = false;
        Debug.Log("Explosion Powerup expired without being used.");
    }

    // Optional: draw explosion radius in Scene view for testing
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
