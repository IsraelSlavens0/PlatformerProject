using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObelokFragmentBR : MonoBehaviour
{
    public enum FragmentState { Idle, Targeting, Teleporting }

    [Header("Aggro Settings")]
    public float chaseTriggerDistance = 10f;

    [Header("Drift Settings")]
    public float driftSpeed = 2f;       // speed of natural drifting
    public float driftAmount = 1.5f;    // max distance it drifts randomly per axis

    [Header("Teleport Settings")]
    public float teleportCooldown = 3f;
    public float teleportRadius = 25f;  // distance around the player to teleport
    private bool canTeleport = true;

    private FragmentState currentState = FragmentState.Idle;
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 driftTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        driftTarget = rb.position;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case FragmentState.Idle:
                if (distanceToPlayer <= chaseTriggerDistance)
                    currentState = FragmentState.Targeting;
                break;

            case FragmentState.Targeting:
                DriftNaturally();
                if (canTeleport)
                    StartCoroutine(TeleportAroundPlayer());
                break;
        }
    }

    void DriftNaturally()
    {
        // Pick a new small random drift target if reached
        if (Vector2.Distance(rb.position, driftTarget) < 0.1f)
        {
            driftTarget = rb.position + new Vector2(
                Random.Range(-driftAmount, driftAmount),
                Random.Range(-driftAmount, driftAmount)
            );
        }

        rb.position = Vector2.Lerp(rb.position, driftTarget, Time.deltaTime * driftSpeed);
    }

    IEnumerator TeleportAroundPlayer()
    {
        canTeleport = false;

        Vector2 newPos;
        int attempts = 0;

        // Keep generating a position until it is not below the player
        do
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float radius = Random.Range(0f, teleportRadius);
            newPos = (Vector2)player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            attempts++;
            if (attempts > 100) break;
        } while (newPos.y < player.position.y);

        rb.position = newPos;
        driftTarget = rb.position; // reset drift target after teleport

        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, teleportRadius);
        }
    }
#endif
}
