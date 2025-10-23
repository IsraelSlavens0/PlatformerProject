using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LungeSkill : MonoBehaviour
{
    [Header("Lunge Settings")]
    public float lungeDistance = 5f;
    public float lungeSpeed = 20f;
    public float lungeDamage = 30f;
    public float lungeManaCost = 30f;
    public float lungeCooldown = 4f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;  // Adjust jump height here

    [Header("References")]
    public GameObject lungeHitbox;  // Assign in Inspector

    private bool isLunging = false;
    private float lungeCooldownTimer = 0f;
    private Vector2 lungeTargetPosition;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
            Debug.LogError("LungeSkill requires PlayerController component on the same GameObject.");

        if (lungeHitbox != null)
            lungeHitbox.SetActive(false);
        else
            Debug.LogWarning("LungeHitbox reference not set in LungeSkill.");
    }

    void Update()
    {
        if (lungeCooldownTimer > 0)
            lungeCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isLunging && lungeCooldownTimer <= 0)
            TryStartLunge();
    }

    void TryStartLunge()
    {
        if (playerController == null) return;

        if (playerController.currentMana >= lungeManaCost)
        {
            playerController.SpendMana(lungeManaCost);
            playerController.UpdateManaUI();

            StartLunge();
        }
        else
        {
            Debug.Log("Not enough mana to lunge!");
        }
    }

    void StartLunge()
    {
        isLunging = true;
        lungeCooldownTimer = lungeCooldown;
        facingRight = playerController.transform.localScale.x >= 0;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        lungeTargetPosition = (Vector2)transform.position + direction * lungeDistance;

        if (lungeHitbox != null)
            lungeHitbox.SetActive(true);
    }

    void FixedUpdate()
    {
        if (isLunging)
            PerformLunge();
    }

    void PerformLunge()
    {
        Vector2 currentPosition = rb.position;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, lungeTargetPosition, lungeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, lungeTargetPosition) < 0.1f)
        {
            isLunging = false;

            if (lungeHitbox != null)
                lungeHitbox.SetActive(false);

            // Small jump after lunge finishes
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // Called by the hitbox trigger to apply damage
    public void ApplyLungeDamage(EnemyHealth enemy)
    {
        if (enemy != null)
        {
            enemy.TakeDamage((int)lungeDamage);
            Debug.Log($"LungeSkill applied {lungeDamage} damage to {enemy.name}");
        }
    }
}
