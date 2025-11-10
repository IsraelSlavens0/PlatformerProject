using UnityEngine;

public class TrollChaseAndSmash : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float chaseRange = 6f;          // How far the troll can detect the player
    public float moveSpeed = 2f;           // Movement speed
    public float stoppingDistance = 1.5f;  // Distance at which the troll stops to attack

    [Header("Attack Settings")]
    public float attackRange = 2f;         // Radius of AOE around attack point
    public float attackDamage = 25f;       // Damage amount
    public float attackCooldown = 3f;      // Time between attacks
    public float attackWindupTime = 1f;    // Time before damage hits

    [Header("References")]
    public Transform player;               // Target player transform
    public Transform attackPoint;          // Point from which AOE originates
    public LayerMask targetLayer;          // Player or damageable layers
    public Animator animator;              // Troll’s animator component

    [Header("Animation Settings")]
    public string attackAnimationTrigger = "Smash"; // Trigger for attack animation
    public string walkAnimationBool = "IsWalking";  // Bool for walking animation

    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private Vector3 originalScale; // store the troll's base scale

    void Start()
    {
        // Save the original scale at start so flipping won't mess it up
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If the player is within detection range
        if (distanceToPlayer <= chaseRange && !isAttacking)
        {
            // Close enough to attack
            if (distanceToPlayer <= stoppingDistance && Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformSmash());
            }
            else
            {
                // Move towards the player
                ChasePlayer();
            }
        }
        else
        {
            // Idle animation if not chasing
            if (animator != null && !string.IsNullOrEmpty(walkAnimationBool))
                animator.SetBool(walkAnimationBool, false);
        }
    }

    void ChasePlayer()
    {
        // Move towards the player
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        // Flip to face the player but preserve the original scale
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(direction.x) * Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }

        // Walking animation
        if (animator != null && !string.IsNullOrEmpty(walkAnimationBool))
            animator.SetBool(walkAnimationBool, true);
    }

    private System.Collections.IEnumerator PerformSmash()
    {
        isAttacking = true;

        // Stop walking animation
        if (animator != null && !string.IsNullOrEmpty(walkAnimationBool))
            animator.SetBool(walkAnimationBool, false);

        // Trigger the attack animation
        if (animator != null && !string.IsNullOrEmpty(attackAnimationTrigger))
            animator.SetTrigger(attackAnimationTrigger);

        // Wait for windup (sync this to the animation's impact frame)
        yield return new WaitForSeconds(attackWindupTime);

        // Apply damage in a radius around the attack point
        Vector2 attackPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPos, attackRange, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        // Reset for next attack
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection and attack range in editor
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (attackPoint != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Gizmos.DrawSphere(attackPoint.position, attackRange);
        }
    }
}
