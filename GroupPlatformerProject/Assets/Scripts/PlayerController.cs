using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // -----------------------
    // Movement Variables
    // -----------------------
    Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    bool grounded = false;

    AudioSource audioSource;
    public AudioClip jumpSound;

    // -----------------------
    // Melee Attack Settings
    // -----------------------
    [Header("Melee Attack Settings")]
    [Tooltip("Damage dealt by melee attacks")]
    public int meleeDamage = 10;

    [Tooltip("Width of the melee hitbox")]
    public float meleeHitboxWidth = 1.0f;

    [Tooltip("Height of the melee hitbox")]
    public float meleeHitboxHeight = 1.0f;

    [Tooltip("Distance from player center to FRONT EDGE of melee hitbox")]
    public float meleeForwardOffset = 0.6f;

    private bool facingRight = true;
    private Vector2 meleeMoveDirection = Vector2.right;

    // -----------------------
    // Ranged Attack Settings
    // -----------------------
    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2f;
    private float nextShootTime = 0f;
    public float shootDelay = 0.5f;


    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 10f;
    public float manaCostPerShot = 20f;

    [Header("UI")]
    public RectTransform manaBarFillRect;
    private float maxManaBarWidth;

    // -----------------------
    // Initialization
    // -----------------------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = Camera.main.GetComponent<AudioSource>();

        currentMana = maxMana;
        if (manaBarFillRect != null)
        {
            maxManaBarWidth = manaBarFillRect.sizeDelta.x;
        }
        UpdateManaUI();
    }

    void Update()
    {
        HandleMovement();
        HandleMeleeInput();
        HandleRangedInput();
        RegenerateMana();
    }

    // -----------------------
    // Movement Logic
    // -----------------------
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");

        Vector3 velocity = rb.velocity;
        velocity.x = moveX * moveSpeed;
        rb.velocity = velocity;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
            rb.AddForce(new Vector2(0, 100 * jumpSpeed));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    // -----------------------
    // Melee Attack Logic
    // -----------------------
    void HandleMeleeInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontal, vertical);

        if (input != Vector2.zero)
        {
            // Flip player sprite horizontally only
            if (horizontal != 0)
            {
                facingRight = horizontal > 0;
                Vector3 scale = transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            meleeMoveDirection = input.normalized;
        }

        if (Input.GetMouseButtonDown(1))
        {
            MeleeAttack();
        }
    }

    void MeleeAttack()
    {
        Vector2 direction = meleeMoveDirection.normalized;
        if (direction == Vector2.zero)
            direction = facingRight ? Vector2.right : Vector2.left;

        float halfWidth = meleeHitboxWidth / 2f;
        float clampedOffset = Mathf.Max(meleeForwardOffset, halfWidth);

        // Position the box so front edge is meleeForwardOffset in front of player
        Vector2 hitboxCenter = (Vector2)transform.position + direction * clampedOffset;

        // Calculate rotation angle in degrees for the box (so it faces the direction)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, new Vector2(meleeHitboxWidth, meleeHitboxHeight), angle);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeDamage);
            }
            KnightHealth KnightBoss = hit.GetComponent<KnightHealth>();
            if (KnightBoss != null)
            {
                KnightBoss.TakeDamage(meleeDamage);
            }
        }
    }

    // -----------------------
    // Ranged Attack Logic
    // -----------------------
    void HandleRangedInput()
    {
        // Only shoot if enough time has passed since last shot
        if (Input.GetButtonDown("Fire1") && Time.time >= nextShootTime)
        {
            if (currentMana >= manaCostPerShot)
            {
                Shoot();
                SpendMana(manaCostPerShot);
                UpdateManaUI();

                // set the next allowed shoot time
                nextShootTime = Time.time + shootDelay;
            }
            else
            {
                Debug.Log("Not enough mana to shoot!");
            }
        }
    }

    void Shoot()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log(mousePos);  // Added from PlayerShoot
        mousePos.z = 0;

        GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Vector3 mouseDir = mousePos - transform.position;
        mouseDir.Normalize();

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.velocity = mouseDir * shootSpeed;
        }

        Destroy(bullet, bulletLifetime);
    }


    // -----------------------
    // Mana System
    // -----------------------
    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            UpdateManaUI();
        }
    }

    public void SpendMana(float amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
    }

    public void UpdateManaUI()
    {
        if (manaBarFillRect != null)
        {
            float newWidth = (currentMana / maxMana) * maxManaBarWidth;
            manaBarFillRect.sizeDelta = new Vector2(newWidth, manaBarFillRect.sizeDelta.y);
        }
    }

    // -----------------------------------------------------------
    // ADDITION: Infinite Mana if Powerups.isInvincible == true
    // -----------------------------------------------------------
    void LateUpdate()
    {
        Powerups powerupScript = FindObjectOfType<Powerups>();
        if (powerupScript != null && powerupScript.isActiveAndEnabled && powerupScript.isInvincible)
        {
            currentMana = maxMana;
            UpdateManaUI();
        }
    }
}
