using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalFarCombatEnemy_Controller : EnemyController
{
    [Header("射程")]
    public float attack_range;
    private float tolerant;
    private NormalFarCombatEnemy_BulletSpawner gun;
    private GameObject p;
    private new void Start()
    {
        base.Start();
        tolerant = 0.1f;
        gun = GetComponent<NormalFarCombatEnemy_BulletSpawner>();
        p = GameObject.FindWithTag("Player");

        enemy_type = Enemy.EnemyType.NormalFarCombatEnemy;
    }

    protected override void find_way()
    {
        if (live == false && knockback_flag == true) return;
        Vector3 p_pos = p.transform.position;
        float dist = Vector3.Distance(transform.position, p_pos) - attack_range;
        if (Math.Abs(dist) > tolerant)
        {
            Vector2 dir = new(player.transform.position.x - rb.position.x, player.transform.position.y - rb.position.y);
            dir = dir.normalized;
            rb.velocity = dist > 0 ? dir * speed : -dir * speed;
        }
        else
        {
            rb.velocity = Vector2.zero;
            gun.Fire((p_pos - transform.position).normalized);
        }
    }
}
