using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    
    GameObject player;
 
    public float chaseSpeed = 5.0f;
  
    public float chaseTriggerDistance = 10f;
   
    public bool returnHome = true;

    Vector3 home;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
     
        home = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
     
        Vector3 chaseDir = player.transform.position - transform.position;
   
        if (chaseDir.magnitude < chaseTriggerDistance)
        {
       
            chaseDir.Normalize();
            GetComponent<Rigidbody2D>().velocity = chaseDir * chaseSpeed;
        }
    
        else if (returnHome)
        {
      
            Vector3 homeDir = home - transform.position;
           
            if (homeDir.magnitude > 0.2f)
            {
                homeDir.Normalize();
                GetComponent<Rigidbody2D>().velocity = homeDir * chaseSpeed;
            }
       
            else
            {
         
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            }
        }

        else
        {
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }
}