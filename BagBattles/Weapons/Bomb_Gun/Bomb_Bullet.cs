using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb_Bullet : Bullet
{

    public GameObject bombPrefab; //爆炸区域预制体
    private float bomb_radius;
    private float bomb_damage;
    private bool show_range;

    public void SetBullet(float r,float b_damage, bool show)
    {
        bomb_radius = r;
        bomb_damage = b_damage;
        show_range = show;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            Explode();
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
        if (other.CompareTag("Wall"))
        {
            Explode();
            Del();
        }
    }

    private void Explode()
    {
        GameObject bomb = ObjectPool.Instance.GetObject(bombPrefab);
        bomb.transform.position = transform.position;
        bomb.GetComponent<BombZone>().Initialize(bomb_radius, bomb_damage, show_range);
    }
}
