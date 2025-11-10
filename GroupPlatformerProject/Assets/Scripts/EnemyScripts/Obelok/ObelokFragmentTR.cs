using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObelokFragmentTR : MonoBehaviour
{
    public enum FragmentState { Idle, Flying }

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float patrolDistance = 5f;       // Horizontal patrol distance
    public float hoverHeight = 3f;          // Fixed vertical height
    private Vector3 startPosition;
    private int moveDirection = 1;           // 1 = right, -1 = left

    [Header("Aggro Settings")]
    public float chaseTriggerDistance = 8f;  // Distance to start moving/shooting

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2f;
    public float shootDelay = 0.5f;
    private float shootTimer = 0f;

    private GameObject player;
    private FragmentState currentState = FragmentState.Idle;

    void Start()
    {
        startPosition = transform.position;
        
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // --- State Management ---
        if (distanceToPlayer <= chaseTriggerDistance)
        {
            currentState = FragmentState.Flying;
        }
        else
        {
            currentState = FragmentState.Idle;
        }

        // --- Movement ---
        if (currentState == FragmentState.Flying)
        {
            // Keep hovering at fixed height
            transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);

            // Fly back and forth horizontally
            transform.Translate(Vector2.right * moveSpeed * moveDirection * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
            {
                moveDirection *= -1; // Change horizontal direction
            }

            // --- Shooting ---
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootDelay)
            {
                ShootAtPlayer(player.transform.position - transform.position);
                shootTimer = 0f;
            }
        }
    }

    void ShootAtPlayer(Vector3 direction)
    {
        direction.Normalize();
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * shootSpeed;
        Destroy(bullet, bulletLifetime);
    }
    void OnDrawGizmosSelected()
    {
        // Draw chase trigger distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseTriggerDistance);

        // Draw patrol range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.left * patrolDistance, transform.position + Vector3.right * patrolDistance);

        // Draw hover height line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + Vector3.up * hoverHeight, transform.position + Vector3.up * hoverHeight + Vector3.right * 0.1f);
    }

}
