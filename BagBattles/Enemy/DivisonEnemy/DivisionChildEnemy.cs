using System.Collections;
using UnityEngine;

public class DivisionChildEnemy : EnemyController
{
    public override void Initialize()
    {
        base.Initialize();
        StartCoroutine(InvincibleCoroutine(1.0f)); // Start invincible coroutine for 2 seconds
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        invincible_flag = true;
        yield return new WaitForSeconds(duration);
        invincible_flag = false;
    }
}