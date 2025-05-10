using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorEnemy : EnemyController
{
    [Header("减伤属性")]
    [Tooltip("减伤百分比")] public float armor;

    protected override void Start()
    {
        base.Start();
        // Initialize enemy-specific properties or behaviors here
        enemy_type = Enemy.EnemyType.ArmorEnemy;
    }

    protected override float CalculateActualDamage(float damage)
    {
        return Mathf.Clamp(damage * (1 - armor), 0, currentHP);
    }
}
