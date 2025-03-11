using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_BulletSpawner : Gun
{
    protected new void Start()
    {
        //animator = GetComponent<Animator>();
        muzzlePos = transform;
        //shellPos = transform.Find("BulletShell");
        flipY = transform.localScale.y;
        random_angel = false;
        attack_flag = false;
    }
    
    private new void Update()
    {
        if (attack_flag == false)
            attack_timer -= Time.deltaTime;

        if (attack_timer <= 0)
        {
            attack_flag = true;
            attack_timer = attack_speed;
        }
    }
}
