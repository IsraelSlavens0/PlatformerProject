using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 30;

    private ThiefAI thiefAI;

    //Added: public prefab to drop when enemy dies
    public GameObject dropPrefab;

    //Added: optional drop position offset
    public Vector3 dropOffset = Vector3.zero;

    void Start()
    {
        thiefAI = GetComponent<ThiefAI>();
    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When hit by a player bullet
        if (collision.gameObject.tag == "PlayerBullet")
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // If this enemy is a thief, drop stolen coins
        if (thiefAI != null)
        {
            thiefAI.DropStolenCoins();
        }

        //Added: drop prefab if assigned
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position + dropOffset, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
