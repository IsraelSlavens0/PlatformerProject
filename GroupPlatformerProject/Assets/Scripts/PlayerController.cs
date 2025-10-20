using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    bool grounded = false;

    AudioSource audioSource;
    public AudioClip jumpSound;

    // Melee attack variables
    public int meleeDamage = 10;
    public float meleeRange = 0.5f;
    public float meleeForwardOffset = 0.6f;
    private Vector2 meleeMoveDirection = Vector2.right; // Default facing right
    private bool facingRight = true;

    // Ranged attack variables
    public GameObject projectilePrefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2f;

    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 10f;
    public float manaCostPerShot = 20f;

    [Header("UI")]
    public RectTransform manaBarFillRect;
    private float maxManaBarWidth;

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

    // Movement logic
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

    // Melee attack logic
    void HandleMeleeInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontal, vertical);

        if (input != Vector2.zero)
        {
            if (horizontal != 0)
            {
                facingRight = horizontal > 0;
                Vector3 scale = transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            meleeMoveDirection = input.normalized;
        }

        // Attack on right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            MeleeAttack();
        }
    }

    void MeleeAttack()
    {
        Vector2 hitPos = (Vector2)transform.position + meleeMoveDirection * meleeForwardOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, meleeRange);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitPos = (Vector2)transform.position + meleeMoveDirection * meleeForwardOffset;
        Gizmos.DrawWireSphere(hitPos, meleeRange);
    }

    // Ranged attack logic
    void HandleRangedInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (currentMana >= manaCostPerShot)
            {
                Shoot();
                SpendMana(manaCostPerShot);
                UpdateManaUI();
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
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
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

    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            UpdateManaUI();
        }
    }

    void SpendMana(float amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
    }

    void UpdateManaUI()
    {
        if (manaBarFillRect != null)
        {
            float newWidth = (currentMana / maxMana) * maxManaBarWidth;
            manaBarFillRect.sizeDelta = new Vector2(newWidth, manaBarFillRect.sizeDelta.y);
        }
    }
}
