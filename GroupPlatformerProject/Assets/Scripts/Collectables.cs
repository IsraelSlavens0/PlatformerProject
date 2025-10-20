using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    //store the number of collected items in a variable
    public int coins = 0;
    public int XP = 0;
    //whenever we collide with a new collectable, add to my variable
    //destroy the collected item so we can't spam collect 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        //check to see if we hit a coin specifically
        if (collision.gameObject.tag == "Coin")
        {
            coins++;
            //Destroy the coin gameobject that we hit
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "ExperiencePoint")
        {
            XP++;
            //destroy the experience point that we collect
            Destroy(collision.gameObject);
        }
    }
}
