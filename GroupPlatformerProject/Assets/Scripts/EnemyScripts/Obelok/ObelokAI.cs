using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Fragment Animators")]
    public Animator animTL;
    public Animator animTR;
    public Animator animBL;
    public Animator animBR;

    [Header("Phase Settings")]
    public ObelokHealth obelokHealth; // Reference to health script
    public GameObject fragmentParent; // Empty parent holding fragments
    public MonoBehaviour[] fragmentAIs; // Fragment AI scripts (TL, TR, BL, BR)
    private bool phaseTwoActivated = false;

    private Rigidbody2D rb;
    private GameObject player;
    private BossState state = BossState.Idle;
    private bool canAttack = true;
    private Vector3 targetHoverPos;
    private bool awakeningPlayed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;

        // Disable fragment AIs initially so they only activate in phase two
        foreach (var ai in fragmentAIs)
        {
            if (ai != null)
                ai.enabled = false;
        }

        PlayAnimationGroup("ObelokFragmentTLAsleep", "ObelokFragmentTRAsleep", "ObelokFragmentBLAsleep", "ObelokFragmentBRAsleep");
    }

    void Update()
    {
        if (player == null) return;

        // Check for phase two activation
        if (!phaseTwoActivated && obelokHealth != null && obelokHealth.health <= 60f)
        {
            ActivatePhaseTwo();
        }

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
                break;

            case BossState.Returning:
                transform.position = Vector3.MoveTowards(transform.position, home, hoverSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, home) < 0.1f)
                {
                    state = BossState.Idle;
                    awakeningPlayed = false;
                    PlayAnimationGroup("ObelokFragmentTLAsleep", "ObelokFragmentTRAsleep", "ObelokFragmentBLAsleep", "ObelokFragmentBRAsleep");
                }
                break;
        }
    }

    void ActivatePhaseTwo()
    {
        phaseTwoActivated = true;

        // Detach fragments from parent so they can move freely
        if (fragmentParent != null)
        {
            fragmentParent.transform.DetachChildren();
            Destroy(fragmentParent); // optional
        }

        // Enable fragment AI scripts now that threshold is reached
        foreach (var ai in fragmentAIs)
        {
            if (ai != null)
                ai.enabled = true;
        }

        Debug.Log("Obelok Phase Two Activated!");
    }

    IEnumerator PlayAwakening()
    {
        if (!awakeningPlayed)
        {
            awakeningPlayed = true;
            PlayAnimationGroup(
                "ObelokFragmentTLAwakening",
                "ObelokFragmentTRAwakening",
                "ObelokFragmentBLAwakening",
                "ObelokFragmentBRAwakening"
            );

            yield return new WaitForSeconds(GetLongestAnimationLength(
                "ObelokFragmentTLAwakening",
                "ObelokFragmentTRAwakening",
                "ObelokFragmentBLAwakening",
                "ObelokFragmentBRAwakening"
            ));
        }

        // ENABLE COLLISIONS NOW
        rb.isKinematic = false;
        rb.bodyType = RigidbodyType2D.Dynamic; // optional if you want gravity/forces
        rb.velocity = Vector2.zero;

        state = BossState.Targeting;
        PlayAnimationGroup(
            "ObelokFragmentTLAwake",
            "ObelokFragmentTRAwake",
            "ObelokFragmentBLAwake",
            "ObelokFragmentBRAwake"
        );
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
        PlayAnimationGroup(
            "ObelokFragmentTLAwake",
            "ObelokFragmentTRAwake",
            "ObelokFragmentBLAwake",
            "ObelokFragmentBRAwake"
        );

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

    void PlayAnimationGroup(string animTLName, string animTRName, string animBLName, string animBRName)
    {
        PlayAnimation(animTL, animTLName);
        PlayAnimation(animTR, animTRName);
        PlayAnimation(animBL, animBLName);
        PlayAnimation(animBR, animBRName);
    }

    void PlayAnimation(Animator animator, string name)
    {
        if (animator == null) return;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(name))
            animator.Play(name);
    }

    float GetLongestAnimationLength(string tlName, string trName, string blName, string brName)
    {
        float maxLen = 1f;
        maxLen = Mathf.Max(maxLen, GetAnimationLength(animTL, tlName));
        maxLen = Mathf.Max(maxLen, GetAnimationLength(animTR, trName));
        maxLen = Mathf.Max(maxLen, GetAnimationLength(animBL, blName));
        maxLen = Mathf.Max(maxLen, GetAnimationLength(animBR, brName));
        return maxLen;
    }

    float GetAnimationLength(Animator animator, string name)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 1f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
                return clip.length;
        }
        return 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseAggroDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * hoverHeight);

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            float groundY = FindGroundBelow(transform.position);
            Vector3 slamTarget = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, slamTarget);
            Gizmos.DrawSphere(slamTarget, 0.3f);
        }
#endif
    }
}
