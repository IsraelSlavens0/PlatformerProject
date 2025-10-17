using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public int damage = 10;
    public float range = 0.5f;          // size of hit area
    public float forwardOffset = 0.6f;  // how far in front of player
    void Start()
    {

    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // right-click
        {
            Attack();
        }
    }

    void Attack()
    {
        // Position in front of the player
        Vector2 hitPos = (Vector2)transform.position + (Vector2)transform.right * forwardOffset;

        // Check all colliders nearby
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, range);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitPos = (Vector2)transform.position + (Vector2)transform.right * forwardOffset;
        Gizmos.DrawWireSphere(hitPos, range);
    }
}
