using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice_Bullet : Bullet
{   
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
            Del();
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyController>().SetIce();
            CauseDamage(other.GetComponent<EnemyController>());
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
    }
}
