using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformerAnimScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = GetComponent<Rigidbody2D>().velocity.y;
        GetComponent<Animator>().SetFloat("x", moveX);
        GetComponent<Animator>().SetFloat("y", moveY);
        //flip the player if our input is negative
        if (moveX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            GetComponent<Animator>().SetBool("Flip", true);
        }
        else if (moveX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            GetComponent<Animator>().SetBool("Flip", false);
        }
    }
}