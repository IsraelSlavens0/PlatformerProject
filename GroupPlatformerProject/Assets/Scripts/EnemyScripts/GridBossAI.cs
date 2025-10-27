using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GridBossAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float chaseTriggerDistance = 10f;

    [Header("Return Home Settings")]
    public bool returnHome = true;
    private Vector3 home;
    private bool isHome = true;

    [Header("Patrol Settings")]
    public bool patrol = true;
    public Vector3 patrolDirection = Vector3.right;
    public float patrolDistance = 3f;

    [Header("Grid Settings")]
    public float gridSize = 1f;

    private Rigidbody2D rb;
    private GameObject player;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        home = SnapToGrid(transform.position);
        transform.position = home;
    }

    void Update()
    {
        if (!isMoving)
        {
            Vector3 target = GetTargetPosition();
            if (target != transform.position)
                StartCoroutine(MoveToGridCell(target));
        }
    }

    /// <summary>
    /// Determines the next grid cell the boss should move toward,
    /// while avoiding obstacles.
    /// </summary>
    Vector3 GetTargetPosition()
    {
        Vector3 target = transform.position;

        if (player == null) return target;

        Vector3 chaseDir = player.transform.position - transform.position;

        // --- Chase logic ---
        if (chaseDir.magnitude < chaseTriggerDistance)
        {
            chaseDir.Normalize();
            Vector3 potential = SnapToGrid(transform.position + RoundToGridStep(chaseDir));
            if (!IsBlocked(potential))
            {
                target = potential;
                isHome = false;
            }
        }
        // --- Return home logic ---
        else if (returnHome && !isHome)
        {
            Vector3 homeDir = home - transform.position;
            if (homeDir.magnitude > 0.1f)
            {
                homeDir.Normalize();
                Vector3 potential = SnapToGrid(transform.position + RoundToGridStep(homeDir));
                if (!IsBlocked(potential))
                    target = potential;
            }
            else
            {
                isHome = true;
            }
        }
        // --- Patrol logic ---
        else if (patrol)
        {
            Vector3 displacement = transform.position - home;
            if (displacement.magnitude >= patrolDistance)
                patrolDirection = -patrolDirection;

            Vector3 potential = SnapToGrid(transform.position + RoundToGridStep(patrolDirection.normalized));
            if (!IsBlocked(potential))
            {
                target = potential;
            }
            else
            {
                // Reverse patrol if blocked
                patrolDirection = -patrolDirection;
            }
        }

        return target;
    }

    /// <summary>
    /// Checks if moving toward the target grid cell would hit Ground or Wall.
    /// </summary>
    bool IsBlocked(Vector3 target)
    {
        Vector2 direction = (target - transform.position).normalized;
        float distance = gridSize * 0.9f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Wall"))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Smoothly moves the boss to the target grid cell.
    /// </summary>
    IEnumerator MoveToGridCell(Vector3 target)
    {
        isMoving = true;

        while ((target - transform.position).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                chaseSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    /// <summary>
    /// Snaps a position to the nearest grid cell.
    /// </summary>
    Vector3 SnapToGrid(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
        pos.z = 0f;
        return pos;
    }

    /// <summary>
    /// Converts a direction vector into a single grid step.
    /// </summary>
    Vector3 RoundToGridStep(Vector3 dir)
    {
        Vector3 step = Vector3.zero;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            step.x = Mathf.Sign(dir.x) * gridSize;
        else
            step.y = Mathf.Sign(dir.y) * gridSize;
        return step;
    }

    /// <summary>
    /// Draws debug rays to visualize obstacle checks.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Gizmos.DrawRay(transform.position, patrolDirection.normalized * gridSize);
        }
    }
}
