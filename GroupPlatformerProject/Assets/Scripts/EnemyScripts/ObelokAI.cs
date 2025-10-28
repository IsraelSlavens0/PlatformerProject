using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class ObelokAI : MonoBehaviour
{
    public enum BossState { Idle, Awakening, Targeting, Charging, Slamming, Returning }

    [Header("Aggro Settings")]
    public float chaseTriggerDistance = 10f;
    public float loseAggroDistance = 15f;
    public float hoverHeight = 4f;

    [Header("Slam Attack Settings")]
    public float hoverSpeed = 4f;
    public float slamSpeed = 20f;
    public float slamChargeTime = 0.5f;
    public float slamCooldown = 1.5f;
    public float groundOffset = 0.5f;
    public LayerMask groundLayer;

    [Header("Return Home Settings")]
    public bool returnHome = true;
    private Vector3 home;

    private Rigidbody2D rb;
    private Animator anim;
    private GameObject player;
    private BossState state = BossState.Idle;
    private bool canAttack = true;
    private Vector3 targetHoverPos;
    private bool awakeningPlayed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;

        PlayAnimation("ObelokAsleep");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        switch (state)
        {
            case BossState.Idle:
                if (distance < chaseTriggerDistance)
                {
                    state = BossState.Awakening;
                    StartCoroutine(PlayAwakening());
                }
                break;

            case BossState.Awakening:
                // handled in coroutine
                break;

            case BossState.Targeting:
                if (distance > loseAggroDistance)
                {
                    state = returnHome ? BossState.Returning : BossState.Idle;
                    break;
                }

                targetHoverPos = new Vector3(player.transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetHoverPos, hoverSpeed * Time.deltaTime);

                if (canAttack && Mathf.Abs(transform.position.x - player.transform.position.x) < 0.5f)
                {
                    StartCoroutine(SlamAttack());
                }
                break;

            case BossState.Slamming:
                // handled in coroutine
                break;

            case BossState.Returning:
                transform.position = Vector3.MoveTowards(transform.position, home, hoverSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, home) < 0.1f)
                {
                    state = BossState.Idle;
                    awakeningPlayed = false;
                    PlayAnimation("ObelokAsleep");
                }
                break;
        }
    }

    IEnumerator PlayAwakening()
    {
        if (!awakeningPlayed)
        {
            awakeningPlayed = true;
            PlayAnimation("ObelokAwakening");
            yield return new WaitForSeconds(GetAnimationLength("ObelokAwakening"));
        }
        state = BossState.Targeting;
        PlayAnimation("ObelokAwake");
    }

    IEnumerator SlamAttack()
    {
        canAttack = false;
        state = BossState.Charging;
        yield return new WaitForSeconds(slamChargeTime);

        state = BossState.Slamming;

        float groundY = FindGroundBelow(transform.position);
        Vector3 slamTarget = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);

        while (transform.position.y > slamTarget.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, slamTarget, slamSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        Vector3 riseTarget = new Vector3(transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
        while (Vector3.Distance(transform.position, riseTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, riseTarget, hoverSpeed * Time.deltaTime);
            yield return null;
        }

        state = BossState.Targeting;
        PlayAnimation("ObelokAwake");

        yield return new WaitForSeconds(slamCooldown);
        canAttack = true;
    }

    float FindGroundBelow(Vector3 fromPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(fromPos, Vector2.down, 100f, groundLayer);
        if (hit.collider != null)
            return hit.point.y;
        return fromPos.y - 5f;
    }

    void PlayAnimation(string name)
    {
        if (anim == null) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            anim.Play(name);
    }

    float GetAnimationLength(string name)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return 1f;
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
                return clip.length;
        }
        return 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseAggroDistance);
    }
}
