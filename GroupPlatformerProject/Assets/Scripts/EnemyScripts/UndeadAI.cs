using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndeadAI : MonoBehaviour
{
    GameObject player;
    public float chaseSpeed = 5.0f;
    public float chaseTriggerDistance = 10f;
    public bool returnHome = true;
    Vector3 home;
    bool isHome = true;
    public bool patrol = true;
    public Vector3 patrolDirection = Vector3.right;  // default to right patrol
    public float patrolDistance = 3f;

    Rigidbody2D rb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 chaseDir = player.transform.position - transform.position;

        // --- Chase Player ---
        if (chaseDir.magnitude < chaseTriggerDistance)
        {
            chaseDir.Normalize();
            rb.velocity = new Vector2(chaseDir.x * chaseSpeed, rb.velocity.y);
            isHome = false;
        }

        // --- Return Home ---
        else if (returnHome && !isHome)
        {
            Vector3 homeDir = home - transform.position;
            if (homeDir.magnitude > 0.2f)
            {
                homeDir.Normalize();
                rb.velocity = new Vector2(homeDir.x * chaseSpeed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                isHome = true;
            }
        }

        // --- Patrol ---
        else if (patrol)
        {
            Vector3 displacement = transform.position - home;
            if (displacement.magnitude > patrolDistance)
            {
                patrolDirection = -displacement;
            }
            patrolDirection.Normalize();
            rb.velocity = new Vector2(patrolDirection.x * chaseSpeed, rb.velocity.y);
        }

        // --- Idle ---
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // --- Show the chase trigger radius distance ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);
    }
}