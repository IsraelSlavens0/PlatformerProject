using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObelokFragmentBR : MonoBehaviour
{
    public ObelokAI parentBoss;
    public ObelokFragmentHealth health;
    public GameObject stonePrefab;

    private bool canAct = true;

    void Start()
    {
        if (health == null) health = GetComponent<ObelokFragmentHealth>();
    }

    void Update()
    {
        if (parentBoss == null || parentBoss.phase != ObelokAI.BossPhase.Phase2) return;
        if (health.isSubdued) return;

        if (canAct)
        {
            StartCoroutine(SummonStone()); // BR = Stone Summoner
        }
    }

    IEnumerator SummonStone()
    {
        canAct = false;
        if (stonePrefab != null)
            Instantiate(stonePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(2f);
        canAct = true;
    }
}
