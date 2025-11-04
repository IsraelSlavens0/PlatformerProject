using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class FragmentHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    [Tooltip("If true, fragment will be disabled when subdued (instead of staying visible under the boss).")]
    public bool hideWhenAssembled = false;

    [Header("References")]
    public ObelokAssemblyManager assemblyManager;

    // runtime
    int currentHealth;
    public bool IsSubdued { get; private set; }

    Collider2D col;
    Rigidbody2D rb;
    MonoBehaviour[] aiComponents; // cached AI scripts to disable when subdued

    void Awake()
    {
        currentHealth = maxHealth;
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // cache AI-like MonoBehaviours (exclude this script)
        aiComponents = GetComponents<MonoBehaviour>();
    }

    public void TakeDamage(int dmg)
    {
        if (IsSubdued) return;

        currentHealth -= Mathf.Max(0, dmg);
        if (currentHealth <= 0)
        {
            Subdue();
        }
    }

    void Subdue()
    {
        if (IsSubdued) return;
        IsSubdued = true;

        // disable physics and collisions so the fragment stops interacting
        if (col) col.enabled = false;
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // disable other MonoBehaviours (AI, movement scripts). Keep this script enabled.
        foreach (var mb in aiComponents)
        {
            if (mb != null && mb != this)
                mb.enabled = false;
        }

        // notify manager
        if (assemblyManager != null)
            assemblyManager.NotifyFragmentSubdued(this);
        else
            Debug.LogWarning($"FragmentHealth on {gameObject.name} has no assemblyManager assigned.");
    }

    /// <summary>
    /// Called by the Assembly Manager when it's time to re-enable / respawn the fragment.
    /// Resets health, reenables components.
    /// </summary>
    public void ResetFragment()
    {
        IsSubdued = false;
        currentHealth = maxHealth;

        // re-enable physics and collider
        if (col) col.enabled = true;
        if (rb) rb.simulated = true;

        // re-enable other components
        foreach (var mb in aiComponents)
        {
            if (mb != null && mb != this)
                mb.enabled = true;
        }

        // ensure the fragment is visible / active
        if (hideWhenAssembled)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when assembly manager wants fragment moved under the boss.
    /// The manager will disable or hide it if configured to do so.
    /// </summary>
    public void AssembleUnder(Transform parent, Vector3 localPosition, bool hide)
    {
        // parent it under the boss so it follows the boss position
        transform.SetParent(parent, worldPositionStays: true);

        // place it to the requested local position (worldToLocal conversion)
        transform.localPosition = localPosition;

        // optionally hide it (e.g. disappear into the boss)
        if (hide)
            gameObject.SetActive(false);
    }

    // --- NEW: Take damage when hit by player bullets ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsSubdued) return;

        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
        }
    }
}
