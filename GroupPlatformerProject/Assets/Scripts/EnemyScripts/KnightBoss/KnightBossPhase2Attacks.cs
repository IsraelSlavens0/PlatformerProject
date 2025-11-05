using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KnightBossPhase2Attacks : MonoBehaviour
{
    [System.Serializable]
    public class Attack
    {
        public string name = "Attack";
        public float duration = 1f;
        public float damage = 10f;
        public float range = 3f;
        public float knockback = 0f;
        public float cooldown = 5f;
        public GameObject hitbox; // reference to hitbox GameObject
    }

    [Header("Attacks")]
    public Attack flamingSlam = new Attack { name = "FlamingSlam", duration = 1.2f, damage = 25f, range = 3f, cooldown = 6f };
    public Attack infernalTorrent = new Attack { name = "InfernalTorrent", duration = 2.8f, damage = 8f, range = 2.5f, cooldown = 7f };
    public Attack groundBreaker = new Attack { name = "GroundBreaker", duration = 1.3f, damage = 22f, range = 3f, cooldown = 8f };
    public Attack moltenEruption = new Attack { name = "MoltenEruption", duration = 1.5f, damage = 20f, range = 3f, cooldown = 9f };
    
    [Header("Fallback Attack")] 
    public Attack basicAttack = new Attack { name = "BasicAttack", duration = 1f, damage = 5f, range = 1.5f, cooldown = 1f};

    [Header("Firebolt (Opener)")]
    public GameObject fireboltPrefab;
    public Transform fireboltSpawnPoint;
    private bool firedBolt = false;

    [Header("Cooldown Timers")]
    private float slamTimer;
    private float torrentTimer;
    private float groundTimer;
    private float eruptionTimer;

    private KnightBossMovement move;
    private Rigidbody2D rb;
    private Transform player;

    [HideInInspector] public bool isAttacking;
    private string currentAttack = "";
    private float attackTimer;

    private float gizmoRadius = 0f;
    private Color gizmoColor = Color.clear;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<KnightBossMovement>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Disable all hitboxes at start
        DisableAllHitboxes();

        // Fire once at start
        if (!firedBolt && fireboltPrefab && fireboltSpawnPoint && player)
        {
            Vector2 dir = (player.position - fireboltSpawnPoint.position).normalized;
            GameObject bolt = Instantiate(fireboltPrefab, fireboltSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rbBolt = bolt.GetComponent<Rigidbody2D>();
            if (rbBolt != null) rbBolt.velocity = dir * 10f;
            firedBolt = true;
        }
    }

    private void Update()
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0) FinishAttack();
            return;
        }

        slamTimer -= Time.deltaTime;
        torrentTimer -= Time.deltaTime;
        groundTimer -= Time.deltaTime;
        eruptionTimer -= Time.deltaTime;
    }

    public void TryRandomAttack(Transform playerTransform)
    {
        if (isAttacking || playerTransform == null) return;

        List<Attack> available = new List<Attack>();
        if (slamTimer <= 0) available.Add(flamingSlam);
        if (torrentTimer <= 0) available.Add(infernalTorrent);
        if (groundTimer <= 0) available.Add(groundBreaker);
        if (eruptionTimer <= 0) available.Add(moltenEruption);

        Attack chosen;
        if (available.Count > 0)
        {
            chosen = available[Random.Range(0, available.Count)];
        }
        else
        {
            // Fallback if all on cooldown
            chosen = basicAttack;
        }

        StartAttack(chosen);
    }


    private void StartAttack(Attack atk)
    {
        if (isAttacking) return;

        isAttacking = true;
        currentAttack = atk.name;
        attackTimer = atk.duration;

        Debug.Log($"Knight Phase 2 starting attack: {atk.name}");


        gizmoRadius = atk.range;
        gizmoColor = Color.red;

        if (move != null)
            move.enabled = false;

        // Attach hitbox damage dynamically
        if (atk.hitbox != null)
        {
            KnightBossHitbox hitboxComponent = atk.hitbox.GetComponent<KnightBossHitbox>();
            if (hitboxComponent == null)
                hitboxComponent = atk.hitbox.AddComponent<KnightBossHitbox>();

            hitboxComponent.damage = atk.damage;
            atk.hitbox.SetActive(true);
        }

        switch (atk.name)
        {
            case "FlamingSlam": StartCoroutine(FlamingSlamRoutine(atk)); slamTimer = atk.cooldown; break;
            case "InfernalTorrent": StartCoroutine(InfernalTorrentRoutine(atk)); torrentTimer = atk.cooldown; break;
            case "GroundBreaker": StartCoroutine(GroundBreakerRoutine(atk)); groundTimer = atk.cooldown; break;
            case "MoltenEruption": StartCoroutine(MoltenEruptionRoutine(atk)); eruptionTimer = atk.cooldown; break;
        }
    }

    private void DisableAllHitboxes()
    {
        if (flamingSlam.hitbox != null) flamingSlam.hitbox.SetActive(false);
        if (infernalTorrent.hitbox != null) infernalTorrent.hitbox.SetActive(false);
        if (groundBreaker.hitbox != null) groundBreaker.hitbox.SetActive(false);
        if (moltenEruption.hitbox != null) moltenEruption.hitbox.SetActive(false);
    }

    // ------------------- Attack Routines -------------------

    private IEnumerator FlamingSlamRoutine(Attack atk)
    {
        Debug.Log("Flaming Slam!");
        yield return new WaitForSeconds(atk.duration * 0.5f);
        moveTowardsPlayer(0);
        yield return new WaitForSeconds(atk.duration * 0.5f);
        FinishAttack();
    }

    private IEnumerator InfernalTorrentRoutine(Attack atk)
    {
        Debug.Log("Infernal Torrent!");
        float elapsed = 0f;
        while (elapsed < atk.duration)
        {
            moveTowardsPlayer(2.5f);
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        FinishAttack();
    }

    private IEnumerator GroundBreakerRoutine(Attack atk)
    {
        Debug.Log("GroundBreaker!");
        moveTowardsPlayer(0);
        yield return new WaitForSeconds(atk.duration);
        FinishAttack();
    }

    private IEnumerator MoltenEruptionRoutine(Attack atk)
    {
        Debug.Log("Molten Eruption!");
        yield return new WaitForSeconds(atk.duration);
        FinishAttack();
    }
    private IEnumerator BasicAttackRoutine(Attack atk)
    {
        Debug.Log("Basic Attack!");
        moveTowardsPlayer(1f); // slow approach

        // Only deal direct damage, no burn
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, atk.range);
        foreach (var h in hits)
        {
            if (h.CompareTag("KnightHitbox"))
            {
                PlayerHealth ph = h.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(atk.damage);
                }
            }
        }

        yield return new WaitForSeconds(atk.duration);
        FinishAttack();
    }



    // ------------------- Shared Logic -------------------

    private void moveTowardsPlayer(float speed)
    {
        if (move == null || player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    private void FinishAttack()
    {
        Debug.Log($"{currentAttack} finished.");
        DisableAllHitboxes();
        rb.velocity = Vector2.zero;
        gizmoColor = Color.clear;
        isAttacking = false;
        currentAttack = "";

        if (move != null)
            move.enabled = true;
    }


    // Helper method for active attack lookup
    private Attack GetAttackByName(string attackName)
    {
        switch (attackName)
        {
            case "FlamingSlam": return flamingSlam;
            case "InfernalTorrent": return infernalTorrent;
            case "GroundBreaker": return groundBreaker;
            case "MoltenEruption": return moltenEruption;
            case "BasicAttack": return basicAttack;
            default: return null;
        }
    }

}
