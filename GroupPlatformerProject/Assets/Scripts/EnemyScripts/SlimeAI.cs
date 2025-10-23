using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAI : MonoBehaviour
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

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float jumpInterval = 2f;

    Rigidbody2D rb;
    float jumpTimer;
    bool isGrounded = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        home = transform.position;
        rb = GetComponent<Rigidbody2D>();
        jumpTimer = jumpInterval;
    }

    void Update()
    {
        Vector3 chaseDir = player.transform.position - transform.position;

        if (chaseDir.magnitude < chaseTriggerDistance)
        {
            chaseDir.Normalize();
            rb.velocity = new Vector2(chaseDir.x * chaseSpeed, rb.velocity.y);
            isHome = false;
        }

        // Go home and if I'm close enough stop moving
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

        // Patrol if the player gets away
        else if (patrol)
        {
            Vector3 displacement = transform.position - home;
            if (displacement.magnitude > patrolDistance)
            {
                patrolDirection = -displacement;
            }
            patrolDirection.Normalize();
            rb.velocity = new Vector2(patrolDirection.x * chaseSpeed, rb.velocity.y);
            HandleJumping();
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // Jump time, and checking to see if I'm grounded
    void HandleJumping()
    {
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0 && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpTimer = jumpInterval;
        }
    }

    // Am I able to jump? Yes
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Am I able to jump? No
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // Show the chase trigger radius distance
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);
    }
}
