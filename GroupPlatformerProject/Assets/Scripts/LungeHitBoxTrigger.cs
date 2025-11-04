using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LungeHitboxTrigger : MonoBehaviour
{
    private LungeSkill playerLungeSkill;

    private void Start()
    {
        playerLungeSkill = GetComponentInParent<LungeSkill>();
        if (playerLungeSkill == null)
        {
            Debug.LogError("LungeHitboxTrigger couldn't find LungeSkill in parent!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerLungeSkill == null) return;


        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            playerLungeSkill.ApplyLungeDamage(collision);
            return;
        }


        KnightHealth knight = collision.GetComponent<KnightHealth>();
        if (knight != null)
        {
            playerLungeSkill.ApplyLungeDamage(collision);
            return;
        }
    }
}
