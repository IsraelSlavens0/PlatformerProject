using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class KnightAttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Base damage dealt to the player on contact.")]
    public float damage = 10f;

    [Header("Burn Effect")]
    [Tooltip("Duration (seconds) of the burn debuff applied to the player.")]
    public float burnDuration = 3f;
    [Tooltip("Damage per second while burning.")]
    public float burnTickDamage = 2f;

    [Header("Lifetime")]
    [Tooltip("If > 0, the hitbox automatically disables after this time.")]
    public float activeTime = 0f;

    private void OnEnable()
    {
        // Optional timed auto-disable for temporary hitboxes
        if (activeTime > 0f)
            StartCoroutine(AutoDisable());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CompareTag("KnightHitbox"))
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Apply direct damage
                ph.TakeDamage(damage);
                Debug.Log($"Player hit by Knight hitbox ({name}) — {damage} damage dealt.");

                // Always apply burn effect
                BurnDebuff burn = collision.GetComponent<BurnDebuff>();
                if (burn == null)
                    burn = collision.gameObject.AddComponent<BurnDebuff>();

                burn.ApplyBurn(burnDuration, burnTickDamage);
            }
        }
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(activeTime);
        gameObject.SetActive(false);
    }
}
