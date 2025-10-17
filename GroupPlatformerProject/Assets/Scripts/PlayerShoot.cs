using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject prefab;
    public float shootSpeed = 10f;
    public float bulletLifetime = 2f;
   
    void Start()
    {

    }

   
    void Update()
    {
     
        if (Input.GetButtonDown("Fire1"))
        {
        
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Debug.Log(mousePos);
            mousePos.z = 0;
      
            GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);

            Vector3 mouseDir = mousePos - transform.position;
            mouseDir.Normalize();
            bullet.GetComponent<Rigidbody2D>().velocity = mouseDir * shootSpeed;
            Destroy(bullet, bulletLifetime);
        }
    }
}
