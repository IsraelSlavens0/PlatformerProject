using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject prefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2.0f;
    public float shootDelay = 0.5f;
    float timer = 0;
   
    public float shootTriggerDistance = 5;
    GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }


    void Update()
    {
        timer += Time.deltaTime;
      
        Vector3 shootDir = player.transform.position - transform.position;

        if (shootDir.magnitude < shootTriggerDistance && timer > shootDelay)
        {
        
            timer = 0;
            shootDir.Normalize();
            GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = shootDir * shootSpeed;
            Destroy(bullet, bulletLifetime);
        }
    }
}
