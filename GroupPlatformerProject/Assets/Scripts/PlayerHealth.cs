using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    //store the players health
    public float health = 10;
    public float maxHealth;
    public Image healthBar;
    //if we collide with something tagged as enemy, take damage
    //if health gets too low, we die (reload the level)
    //if we collide with something tagged as health pack, increase health
    private void OnCollisionEnter2D(Collision2D collision)
    {
        

        if (collision.gameObject.tag == "Enemy" && !GetComponent<Powerups>().isInvincible)
        {
            health--;
            healthBar.fillAmount = health / maxHealth;
            if (health <= 0)
            {
                //if health is too low, reload the level
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            }
        }
        //if we collide with the health pack collectable
        if (collision.gameObject.tag == "HealthPack")
        {
            //increase the health value
            health++;
            healthBar.fillAmount = health / maxHealth;
            Destroy(collision.gameObject);
            //if our health is trying to exceed our max health
            if (health > maxHealth)
            {
                //cap our health at max health
                health = maxHealth;
            }
        }
    }
   private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EnemyBullet")
        {
            health--;
            healthBar.fillAmount = health / maxHealth;
            if (health <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }
}