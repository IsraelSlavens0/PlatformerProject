using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObelokFragmentBL : MonoBehaviour
{
    public ObelokAI parentBoss;
    public ObelokFragmentHealth health;
    public float dashSpeed = 15f;
    public float dashCooldown = 3f;

    private bool canAct = true;
    private GameObject player;
    private Vector3 dashTarget;

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
            StartCoroutine(HorizontalDash()); // BL = Horizontal Dash
        }
    }

    IEnumerator HorizontalDash()
    {
        canAct = false;

        float targetY = player.transform.position.y;
        dashTarget = new Vector3(player.transform.position.x + 10f * Mathf.Sign(transform.position.x - player.transform.position.x), targetY, transform.position.z);

        while (Vector3.Distance(transform.position, dashTarget) > 0.1f)
        {
            Vector3 move = Vector3.MoveTowards(transform.position, new Vector3(dashTarget.x, transform.position.y, transform.position.z), dashSpeed * Time.deltaTime);
            transform.position = move;
            yield return null;
        }

        yield return new WaitForSeconds(dashCooldown);
        canAct = true;
    }
}
