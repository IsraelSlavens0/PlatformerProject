using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObelokFragmentTR : MonoBehaviour
{
    public ObelokAI parentBoss;
    public ObelokFragmentHealth health;
    public GameObject projectilePrefab;
    public float hoverHeight = 4f;

    private bool canAct = true;
    private GameObject player;

    void Start()
    {
        if (health == null) health = GetComponent<ObelokFragmentHealth>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (parentBoss == null || parentBoss.phase != ObelokAI.BossPhase.Phase2) return;
        if (health.isSubdued) return;

        if (canAct && player != null)
        {
            StartCoroutine(FireballAttack()); // TR = Fireball
        }
    }

    IEnumerator FireballAttack()
    {
        canAct = false;
        if (projectilePrefab != null)
            Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);
        canAct = true;
    }
}
