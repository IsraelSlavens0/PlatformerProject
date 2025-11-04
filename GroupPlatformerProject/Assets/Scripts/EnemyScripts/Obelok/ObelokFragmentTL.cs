using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObelokFragmentTL : MonoBehaviour
{
    public enum FragmentState { Idle, Targeting, Charging, Slamming }

    [Header("Aggro Settings")]
    public float chaseTriggerDistance = 10f;
    public float hoverHeight = 4f;

    [Header("Slam Attack Settings")]
    public float hoverSpeed = 4f;
    public float slamSpeed = 20f;
    public float slamChargeTime = 0.5f;
    public float slamCooldown = 1.5f;
    public float groundOffset = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private GameObject player;
    private FragmentState state = FragmentState.Idle;
    private bool canAttack = true;
    private Vector3 targetHoverPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        switch (state)
        {
            case FragmentState.Idle:
                if (distance < chaseTriggerDistance)
                    state = FragmentState.Targeting;
                break;

            case FragmentState.Targeting:
                // Hover above player
                targetHoverPos = new Vector3(player.transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetHoverPos, hoverSpeed * Time.deltaTime);

                // Slam if horizontally aligned
                if (canAttack && Mathf.Abs(transform.position.x - player.transform.position.x) < 0.5f)
                    StartCoroutine(SlamAttack());
                break;

            case FragmentState.Charging:
            case FragmentState.Slamming:
                // Handled in coroutine
                break;
        }
    }

    IEnumerator SlamAttack()
    {
        canAttack = false;
        state = FragmentState.Charging;
        yield return new WaitForSeconds(slamChargeTime);

        state = FragmentState.Slamming;

        float groundY = FindGroundBelow(transform.position);
        Vector3 slamTarget = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);

        // Slam down
        while (transform.position.y > slamTarget.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, slamTarget, slamSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // Rise back above player
        Vector3 riseTarget = new Vector3(transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
        while (Vector3.Distance(transform.position, riseTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, riseTarget, hoverSpeed * Time.deltaTime);
            yield return null;
        }

        state = FragmentState.Targeting;
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
    void OnDrawGizmosSelected()
    {
        // Draw chase trigger distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);

        // Draw slam target position (downward)
        Gizmos.color = Color.red;
        float groundY = transform.position.y - 5f;
#if UNITY_EDITOR
        if (Application.isPlaying) groundY = FindGroundBelow(transform.position);
#endif
        Vector3 slamTarget = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);
        Gizmos.DrawLine(transform.position, slamTarget);

        // Draw hover position above player (if assigned)
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 hoverTarget = new Vector3(player.transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
            Gizmos.DrawLine(transform.position, hoverTarget);
            Gizmos.DrawSphere(hoverTarget, 0.2f);
        }
    }

}
