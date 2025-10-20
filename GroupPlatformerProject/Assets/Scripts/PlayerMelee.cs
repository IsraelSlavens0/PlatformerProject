using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public int damage = 10;
    public float range = 0.5f;
    public float forwardOffset = 0.6f;

    private Vector2 moveDirection = Vector2.right; // Default facing right
    private bool facingRight = true;

    void Update()
    {
        // Get raw input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontal, vertical);

        if (input != Vector2.zero)
        {
            // Flip player only based on horizontal input
            if (horizontal != 0)
            {
                facingRight = horizontal > 0;
                Vector3 scale = transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            // Set moveDirection to normalized input vector (allows diagonal)
            moveDirection = input.normalized;
        }

        // Attack on right mouse click
        if (Input.GetMouseButtonDown(1))
        {
            Attack();
        }
    }

    void Attack()
    {
        Vector2 hitPos = (Vector2)transform.position + moveDirection * forwardOffset;

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
        Vector2 hitPos = (Vector2)transform.position + moveDirection * forwardOffset;
        Gizmos.DrawWireSphere(hitPos, range);
    }
}
