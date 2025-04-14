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

    public override bool TakeDamage(float damage)
    {
        if (live == false || isActiveAndEnabled == false || currentHP <= 0f)
            return false;
        if (invincible_flag == false && live)
        {
            if (invincible_time != 0) invincible_flag = true;
            float actual_damage = damage * (1 - armor);
            Debug.Log("enemy take damage: " + actual_damage);

            currentHP -= actual_damage;

            // 设置无敌状态
            if (invincible_time > 0)
            {
                invincible_flag = true;
                invincible_timer = 0f;
            }
            
            // 生成伤害数字
            EnemyDamageNumberController.Instance.CreateDamageNumber(damage, transform.position + new Vector3(0, 1.5f, 0));
            if (currentHP <= 0f)
            {
                live = false;
                rb.velocity = Vector2.zero;
                ObjectPool.Instance.PushObject(gameObject);
                return true;
            }
            // 启动闪烁效果
            if (!isFlashing && spriteRenderer != null && live)
            {
                StartCoroutine(FlashEffect());
            }
            return true;
        }
        return false;
    }
}
