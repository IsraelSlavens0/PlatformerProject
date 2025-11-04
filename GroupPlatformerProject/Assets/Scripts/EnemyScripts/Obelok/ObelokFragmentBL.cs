using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObelokFragmentBL : MonoBehaviour
{
    public enum FragmentState { Idle, Targeting, Charging, Dashing }

    [Header("Aggro Settings")]
    public float chaseTriggerDistance = 10f;
    public float hoverOffset = 4f; // horizontal distance to hover beside player

    [Header("Dash Attack Settings")]
    public float hoverSpeed = 4f;
    public float dashSpeed = 20f;          // renamed from slamSpeed
    public float dashChargeTime = 0.5f;    // renamed from slamChargeTime
    public float dashCooldown = 1.5f;      // renamed from slamCooldown
    public float dashDistance = 8f;        // how far it dashes horizontally

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
                // Hover beside player
                float side = transform.position.x < player.transform.position.x ? -1f : 1f;
                targetHoverPos = new Vector3(player.transform.position.x + hoverOffset * side, player.transform.position.y, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetHoverPos, hoverSpeed * Time.deltaTime);

                // Dash if vertically aligned
                if (canAttack && Mathf.Abs(transform.position.y - player.transform.position.y) < 0.5f)
                    StartCoroutine(DashAttack());
                break;

            case FragmentState.Charging:
            case FragmentState.Dashing:
                // handled in coroutine
                break;
        }
    }

    IEnumerator DashAttack()
    {
        canAttack = false;
        state = FragmentState.Charging;
        yield return new WaitForSeconds(dashChargeTime);

        state = FragmentState.Dashing;

        // Determine horizontal direction
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);

        // Calculate dash target X using dashDistance
        Vector3 dashTarget = new Vector3(transform.position.x + direction * dashDistance, transform.position.y, transform.position.z);

        // Dash horizontally toward target
        while (Mathf.Abs(transform.position.x - dashTarget.x) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, dashSpeed * Time.deltaTime);
            yield return null;
        }

        // After dash, hover back beside player
        float hoverSide = transform.position.x < player.transform.position.x ? -1f : 1f;
        Vector3 hoverTarget = new Vector3(player.transform.position.x + hoverOffset * hoverSide, player.transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, hoverTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverTarget, hoverSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dashCooldown);
        canAttack = true;
        state = FragmentState.Targeting;
    }
    void OnDrawGizmosSelected()
    {
        // Draw chase trigger distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);

        // Draw dash target range
        Gizmos.color = Color.red;
        Vector3 leftDash = transform.position + Vector3.left * dashDistance;
        Vector3 rightDash = transform.position + Vector3.right * dashDistance;
        Gizmos.DrawLine(transform.position, leftDash);
        Gizmos.DrawLine(transform.position, rightDash);

        // Draw hover offset positions relative to player (if assigned)
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(player.transform.position + Vector3.right * hoverOffset, 0.2f);
            Gizmos.DrawSphere(player.transform.position + Vector3.left * hoverOffset, 0.2f);
        }
    }
}
