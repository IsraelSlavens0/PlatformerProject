using UnityEngine;

public class ObelokFragmentHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 10f;
    public bool isSubdued = false;

    private float currentHealth;
    private ObelokAI obelokParent; // Reference to main ObelokAI

    private void Start()
    {
        currentHealth = maxHealth;
        obelokParent = FindObjectOfType<ObelokAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSubdued) return;

        if (collision.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isSubdued) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Subdue();
        }
    }

    private void Subdue()
    {
        isSubdued = true;

        // Disable attack script
        MonoBehaviour attackScript = GetComponent<MonoBehaviour>();
        if (attackScript != null)
            attackScript.enabled = false;
    }

    public void Revive()
    {
        isSubdued = false;
        currentHealth = maxHealth;

        // Re-enable attack logic if it exists
        MonoBehaviour attackScript = GetComponent<MonoBehaviour>();
        if (attackScript != null)
            attackScript.enabled = true;
    }
}
