using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObelokAssemblyManager : MonoBehaviour
{
    [Header("Fragments")]
    [Tooltip("Assign all fragment FragmentHealth components here (top-left, top-right, etc.).")]
    public List<FragmentHealth> fragments = new List<FragmentHealth>();

    [Header("Reassembly / Vulnerability")]
    [Tooltip("How long the boss stays reassembled and vulnerable (seconds).")]
    public float reassembleDuration = 10f;

    [Tooltip("Enable to hide individual fragment objects while assembled into the boss.")]
    public bool hideFragmentsWhileAssembled = true;

    [Tooltip("Local positions where fragments will be placed relative to the boss when reassembled. If empty, all fragments are placed at (0,0,0).")]
    public List<Vector3> assembleLocalPositions = new List<Vector3>();

    [Tooltip("Collider (or hurtbox) representing the whole boss being vulnerable. Enable/disable this to allow damage to the boss.")]
    public Collider2D bossHurtbox;

    [Tooltip("Optional: the GameObject that visually represents boss being vulnerable (e.g., flash, animation).")]
    public GameObject vulnerableVisual;

    // runtime
    int subduedCount = 0;
    bool isAssembled = false;
    Coroutine reassembleCoroutine;

    void Start()
    {
        // sanity: hook manager in fragments if not assigned
        foreach (var f in fragments)
        {
            if (f != null && f.assemblyManager == null)
                f.assemblyManager = this;
        }

        // fill assembleLocalPositions if empty with zeros to match fragments count
        while (assembleLocalPositions.Count < fragments.Count)
            assembleLocalPositions.Add(Vector3.zero);

        SetBossVulnerable(false);
    }

    /// <summary>
    /// Called by fragments when they are subdued.
    /// </summary>
    public void NotifyFragmentSubdued(FragmentHealth frag)
    {
        subduedCount = 0;
        foreach (var f in fragments)
            if (f != null && f.IsSubdued) subduedCount++;

        if (subduedCount >= fragments.Count && !isAssembled)
        {
            // start reassemble
            if (reassembleCoroutine != null) StopCoroutine(reassembleCoroutine);
            reassembleCoroutine = StartCoroutine(ReassembleAndOpenWindow());
        }
    }

    IEnumerator ReassembleAndOpenWindow()
    {
        isAssembled = true;

        // Move fragments to parent and optionally hide them
        for (int i = 0; i < fragments.Count; i++)
        {
            var f = fragments[i];
            if (f == null) continue;

            Vector3 localPos = (i < assembleLocalPositions.Count) ? assembleLocalPositions[i] : Vector3.zero;
            f.AssembleUnder(transform, localPos, hideFragmentsWhileAssembled);
        }

        // enable boss vulnerability
        SetBossVulnerable(true);

        // wait the configured time window
        float timer = 0f;
        while (timer < reassembleDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // After window ends: respawn / reset fragments and make boss invulnerable again
        foreach (var f in fragments)
        {
            if (f == null) continue;

            // restore fragment to root (optional) and reset state
            f.transform.SetParent(null, worldPositionStays: true); // unparent so fragments return to their own behavior
            if (hideFragmentsWhileAssembled)
            {
                // if the fragment was hidden while assembled, make sure active before resetting
                f.gameObject.SetActive(true);
            }

            f.ResetFragment();
        }

        // reset counters + flags
        subduedCount = 0;
        isAssembled = false;
        SetBossVulnerable(false);
        reassembleCoroutine = null;
    }

    void SetBossVulnerable(bool on)
    {
        if (bossHurtbox != null)
            bossHurtbox.enabled = on;

        if (vulnerableVisual != null)
            vulnerableVisual.SetActive(on);

        // you can also set a public flag for other systems to check
        // e.g. public bool BossIsVulnerable { get; private set; } = false;
    }
}
