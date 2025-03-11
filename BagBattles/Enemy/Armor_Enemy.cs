using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor_Enemy : EnemyController
{
    [Header("减伤属性")]
    [Tooltip("减伤百分比")] public float armor;
    public override bool TakeDamage(float damage)
    {
        if (live == false || isActiveAndEnabled == false || currentHP <= 0f)
            return false;
        if (invincible_flag == false)
        {
            if (invincible_time != 0) invincible_flag = true;

            currentHP -= (int)(damage * (1 - armor));
            if (currentHP <= 0f)
            {
                live = false;
                rb.velocity = Vector2.zero;
                ObjectPool.Instance.PushObject(gameObject);
                return true;
            }
        }
        return false;
    }
}
