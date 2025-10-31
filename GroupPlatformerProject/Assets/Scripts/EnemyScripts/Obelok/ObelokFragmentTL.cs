using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObelokFragmentTL : MonoBehaviour
{
    public ObelokAI parentBoss; // Reference to main boss
    public ObelokFragmentHealth health;
    public float hoverHeight = 4f;
    public float hoverSpeed = 4f;
    public float slamSpeed = 20f;
    public float slamChargeTime = 0.5f;
    public LayerMask groundLayer;

    private bool canAct = true;
    private GameObject player;

    void Start()
    {
        if (health == null) health = GetComponent<ObelokFragmentHealth>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Only act if parent boss is in Phase 2 and fragment is not subdued
        if (parentBoss == null || parentBoss.phase != ObelokAI.BossPhase.Phase2) return;
        if (health.isSubdued) return;

        if (canAct && player != null)
        {
            StartCoroutine(SlamAttack()); // TL = Slammer
        }
    }

    IEnumerator SlamAttack()
    {
        canAct = false;
        yield return new WaitForSeconds(slamChargeTime);

        float groundY = FindGroundBelow(transform.position);
        Vector3 slamTarget = new Vector3(transform.position.x, groundY + 0.5f, transform.position.z);

        while (transform.position.y > slamTarget.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, slamTarget, slamSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        Vector3 riseTarget = new Vector3(transform.position.x, player.transform.position.y + hoverHeight, transform.position.z);
        while (Vector3.Distance(transform.position, riseTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, riseTarget, hoverSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        canAct = true;
    }

    float FindGroundBelow(Vector3 fromPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(fromPos, Vector2.down, 100f, groundLayer);
        if (hit.collider != null) return hit.point.y;
        return fromPos.y - 5f;
    }
}
