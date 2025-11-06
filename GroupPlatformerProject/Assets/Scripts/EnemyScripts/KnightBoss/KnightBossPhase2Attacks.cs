using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(KnightBossMovement))]
public class KnightBossPhase2Attacks : MonoBehaviour
{
    [System.Serializable]
    public class Attack
    {
        public string name;
        public float damage;
        public float range;
        public float duration;
    }

    [Header("Phase 2 Attacks")]
    public Attack flamingSlam;
    public Attack infernalTorrent;
    public Attack groundBreaker;
    public Attack moltenEruption;

    [Header("Hitbox Settings")]
    public LayerMask playerLayer;
    public float slamRadius = 2.5f;
    public float eruptionRadius = 4f;
    public Vector2 groundBreakerSize = new Vector2(3f, 1.2f);

    private Rigidbody2D rb;
    private KnightBossMovement movement;
    private Transform player;
    [HideInInspector] public bool isAttacking = false;

    private List<(Vector3 pos, float size, bool box, float time)> debugHits = new List<(Vector3, float, bool, float)>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<KnightBossMovement>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // --------------------------------------------------
    // ATTACK CHOOSER
    // --------------------------------------------------
    public void TryRandomAttack(Transform target)
    {
        if (isAttacking) return;
        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: StartCoroutine(FlamingSlamRoutine(flamingSlam)); break;
            case 1: StartCoroutine(InfernalTorrentRoutine(infernalTorrent)); break;
            case 2: StartCoroutine(GroundBreakerRoutine(groundBreaker)); break;
            case 3: StartCoroutine(MoltenEruptionRoutine(moltenEruption)); break;
        }
    }

    // --------------------------------------------------
    // FLAMING SLAM — jump then slam down
    // --------------------------------------------------
    private IEnumerator FlamingSlamRoutine(Attack atk)
    {
        Debug.Log("🔥 Flaming Slam!");
        isAttacking = true;

        rb.velocity = new Vector2(rb.velocity.x, atk.range + 10f);
        yield return new WaitUntil(() => rb.velocity.y <= 0);

        rb.velocity = new Vector2(0, -atk.range * 3f);
        yield return new WaitUntil(() => movementGrounded());

        DoAOEDamage(transform.position, slamRadius, atk.damage, 4f, 3f);
        CreateDebugHit(transform.position, slamRadius, false);

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // --------------------------------------------------
    // INFERNAL TORRENT — spinning rush
    // --------------------------------------------------
    private IEnumerator InfernalTorrentRoutine(Attack atk)
    {
        Debug.Log("🔥 Infernal Torrent!");
        isAttacking = true;

        float elapsed = 0f;
        while (elapsed < atk.duration)
        {
            if (player != null)
                MoveTowardsPlayer(3.5f);

            DoAOEDamage(transform.position, 1.6f, atk.damage * 0.75f, 2.5f, 2f);
            CreateDebugHit(transform.position, 1.6f, false);

            yield return new WaitForSeconds(0.4f);
            elapsed += 0.4f;
        }

        rb.velocity = Vector2.zero;
        isAttacking = false;
    }

    // --------------------------------------------------
    // GROUND BREAKER — ground-only smash
    // --------------------------------------------------
    private IEnumerator GroundBreakerRoutine(Attack atk)
    {
        Debug.Log("💥 Ground Breaker!");
        isAttacking = true;

        rb.velocity = new Vector2(0, 8f);
        yield return new WaitUntil(() => rb.velocity.y <= 0 && movementGrounded());

        Vector2 boxCenter = (Vector2)transform.position + Vector2.down * 0.5f;
        DoBoxDamage(boxCenter, groundBreakerSize, atk.damage, 0f, 0f);
        CreateDebugHit(boxCenter, groundBreakerSize.x, true);

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    // --------------------------------------------------
    // MOLTEN ERUPTION — AoE around Knight
    // --------------------------------------------------
    private IEnumerator MoltenEruptionRoutine(Attack atk)
    {
        Debug.Log("🌋 Molten Eruption!");
        isAttacking = true;

        yield return new WaitForSeconds(atk.duration * 0.4f);

        DoAOEDamage(transform.position, eruptionRadius, atk.damage * 0.8f, 5f, 2.5f);
        CreateDebugHit(transform.position, eruptionRadius, false);

        yield return new WaitForSeconds(atk.duration * 0.6f);
        isAttacking = false;
    }

    // --------------------------------------------------
    // DAMAGE HELPERS
    // --------------------------------------------------
    private void DoAOEDamage(Vector2 center, float radius, float damage, float burnDuration, float burnTick)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph == null) continue;

            ph.TakeDamage(damage);

            if (burnDuration > 0)
            {
                FireDebuff burn = hit.GetComponent<FireDebuff>();
                if (burn == null) burn = hit.gameObject.AddComponent<FireDebuff>();
                burn.ApplyBurn(burnDuration, burnTick);
            }
        }
    }

    private void DoBoxDamage(Vector2 center, Vector2 size, float damage, float burnDuration, float burnTick)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph == null) continue;

            ph.TakeDamage(damage);

            if (burnDuration > 0)
            {
                FireDebuff burn = hit.GetComponent<FireDebuff>();
                if (burn == null) burn = hit.gameObject.AddComponent<FireDebuff>();
                burn.ApplyBurn(burnDuration, burnTick);
            }
        }
    }

    private bool movementGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
    }

    private void MoveTowardsPlayer(float speed)
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    // --------------------------------------------------
    // DEBUG VISUALS
    // --------------------------------------------------
    private void CreateDebugHit(Vector3 pos, float size, bool box)
    {
        debugHits.Add((pos, size, box, Time.time + 0.5f));

        // Show a quick visual indicator in Play mode
        if (Application.isPlaying)
        {
            if (box)
            {
                DebugDrawBox(pos, new Vector2(size, size / 3f), Color.red, 0.5f);
            }
            else
            {
                DebugDrawCircle(pos, size, Color.red, 0.5f);
            }
        }
    }

    private void Update()
    {
        // Remove expired debug markers
        debugHits.RemoveAll(d => Time.time > d.time);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.3f);
        foreach (var d in debugHits)
        {
            if (d.box)
                Gizmos.DrawWireCube(d.pos, new Vector3(d.size, d.size / 3f, 1));
            else
                Gizmos.DrawWireSphere(d.pos, d.size);
        }
    }

    // --------------------------------------------------
    // Debug Draw helpers (visible in Game view during Play)
    // --------------------------------------------------
    private void DebugDrawCircle(Vector3 center, float radius, Color color, float duration)
    {
        int segments = 20;
        Vector3 prev = center + Vector3.right * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Debug.DrawLine(prev, next, color, duration);
            prev = next;
        }
    }

    private void DebugDrawBox(Vector3 center, Vector2 size, Color color, float duration)
    {
        Vector3 half = (Vector3)size / 2f;
        Vector3 topLeft = center + new Vector3(-half.x, half.y);
        Vector3 topRight = center + new Vector3(half.x, half.y);
        Vector3 bottomRight = center + new Vector3(half.x, -half.y);
        Vector3 bottomLeft = center + new Vector3(-half.x, -half.y);

        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }
}
