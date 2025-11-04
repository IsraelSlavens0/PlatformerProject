using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnDebuff : MonoBehaviour
{
    private Coroutine burnRoutine;

    public void ApplyBurn(float duration, float tickDamage)
    {
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(Burn(duration, tickDamage));
    }

    private IEnumerator Burn(float duration, float tickDamage)
    {
        float elapsed = 0f;
        PlayerHealth ph = GetComponent<PlayerHealth>();

        while (elapsed < duration)
        {
            if (ph != null)
            {
                ph.TakeDamage(tickDamage);
                Debug.Log($"{name} takes {tickDamage} burn damage!");
            }

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        burnRoutine = null;
    }
}
