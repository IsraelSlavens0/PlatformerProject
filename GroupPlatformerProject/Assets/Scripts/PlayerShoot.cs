using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    public GameObject prefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2f;

    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 10f;
    public float manaCostPerShot = 20f;

    [Header("UI")]
    public RectTransform manaBarFillRect;  // Drag ManaBarFill RectTransform here
    private float maxManaBarWidth;

    void Start()
    {
        currentMana = maxMana;
        if (manaBarFillRect != null)
        {
            maxManaBarWidth = manaBarFillRect.sizeDelta.x;  // Store the original width
        }
        UpdateManaUI();
    }

    void Update()
    {
        RegenerateMana();

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

        GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);

        Vector3 mouseDir = mousePos - transform.position;
        mouseDir.Normalize();

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = mouseDir * shootSpeed;
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

