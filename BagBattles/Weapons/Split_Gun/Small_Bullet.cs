using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Small_Bullet : Bullet
{
    private GameObject ignore_enemy;

    public void SetBullet(BulletBasicAttribute attr, GameObject ignore)
    {
        this.bulletBasicAttribute = attr;
        ignore_enemy = ignore;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject == ignore_enemy)
            {
                Debug.Log("hit ignore_enemy");
                return;
            }
            CauseDamage(other.GetComponent<EnemyController>());
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
        if (other.CompareTag("Wall"))
            Del();
    }
}
