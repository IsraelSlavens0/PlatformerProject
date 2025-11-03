using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObelokAI : MonoBehaviour
{
    public enum BossState { Idle, Awakening, Targeting, Charging, Slamming, Returning, Splitting, Reforming, KO }
    public enum BossPhase { Phase1, Phase2 }

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

    [Header("Fragment References")]
    public GameObject fragmentTL;
    public GameObject fragmentTR;
    public GameObject fragmentBL;
    public GameObject fragmentBR;

    [Header("Phase 2 Settings")]
    public float reformTime = 3f;
    public float KODuration = 10f;
    public float KOHeight = 1f; // Position Y where Obelok falls and is KO'd

    private Rigidbody2D rb;
    private GameObject player;
    private BossState state = BossState.Idle;
    public BossPhase phase = BossPhase.Phase1;
    private bool canAttack = true;
    private Vector3 targetHoverPos;
    private bool awakeningPlayed = false;
    private bool isKO = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;

        PlayAnimationGroup("ObelokFragmentTLAsleep", "ObelokFragmentTRAsleep", "ObelokFragmentBLAsleep", "ObelokFragmentBRAsleep");
        SetFragmentsActive(false);
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

            case BossState.Splitting:
                SplitIntoFragments();
                break;

            case BossState.Reforming:
                break;

            case BossState.KO:
                // KO timer logic could go here
                break;
        }

        // Phase 2 reform check placeholder (you can hook your own logic here)
        if (phase == BossPhase.Phase2 && state != BossState.Reforming && !isKO)
        {
            // Example condition (replace with your own)
            if (AllFragmentsInactive())
            {
                StartCoroutine(ReformFragments());
            }
        }
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

        phase = BossPhase.Phase1;
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

    void SplitIntoFragments()
    {
        phase = BossPhase.Phase2;
        SetFragmentsActive(true);
        gameObject.SetActive(false); // Hide main body while fragments act
    }

    IEnumerator ReformFragments()
    {
        state = BossState.Reforming;
        SetFragmentsActive(false);

        // Show main boss above ground and fall
        gameObject.SetActive(true);
        Vector3 startPos = transform.position + Vector3.up * 5f; // Fall from above
        Vector3 endPos = new Vector3(transform.position.x, KOHeight, transform.position.z);
        float fallSpeed = 10f;

        while (Vector3.Distance(transform.position, endPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, fallSpeed * Time.deltaTime);
            yield return null;
        }

        // Start KO state
        state = BossState.KO;
        isKO = true;

        // Wait KO duration
        yield return new WaitForSeconds(KODuration);

        // Resume phase 2
        state = BossState.Targeting;
        isKO = false;
    }

    // Replaced old health check with a placeholder function
    bool AllFragmentsInactive()
    {
        return (fragmentTL != null && !fragmentTL.activeInHierarchy) &&
               (fragmentTR != null && !fragmentTR.activeInHierarchy) &&
               (fragmentBL != null && !fragmentBL.activeInHierarchy) &&
               (fragmentBR != null && !fragmentBR.activeInHierarchy);
    }

    void SetFragmentsActive(bool active)
    {
        if (fragmentTL != null) fragmentTL.SetActive(active);
        if (fragmentTR != null) fragmentTR.SetActive(active);
        if (fragmentBL != null) fragmentBL.SetActive(active);
        if (fragmentBR != null) fragmentBR.SetActive(active);
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
}
