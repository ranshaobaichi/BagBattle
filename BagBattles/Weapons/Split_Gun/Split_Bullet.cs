using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Split_Bullet : Bullet
{
    public GameObject smallBulletPrefab;
    private int splitNum;
    private float splitAngle;

    public BulletBasicAttribute small_bullet_attr;

    private Vector2 dir;

    public void SetBullet(int splitNum, float splitAngle, BulletBasicAttribute small_attr, Vector2 dir)
    {
        this.splitNum = splitNum;
        this.splitAngle = splitAngle;
        small_bullet_attr = small_attr;
        this.dir = dir;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            Split(other.gameObject);
            Del();
        }
        if (other.CompareTag("Wall"))
            Del();
    }

    //TODO: change the pass_num to spilt num
    //FIXME: This method is too long
    private void Split(GameObject hit_enemy)
    {
        //分裂
        int median = splitNum / 2;
        for (int i = 0; i < splitNum; i++)
        {
            GameObject bullet = ObjectPool.Instance.GetObject(smallBulletPrefab);
            bullet.transform.position = transform.position;
            bullet.GetComponent<Small_Bullet>().SetBullet(small_bullet_attr, hit_enemy);
            Rigidbody2D rigidbody = bullet.GetComponent<Rigidbody2D>();
            if (splitNum % 2 == 1)
            {
                rigidbody.velocity = Quaternion.Euler(0, 0, splitAngle * (i - median)) * dir * small_bullet_attr.speed;
            }
            else
            {
                rigidbody.velocity = Quaternion.Euler(0, 0, splitAngle * (i - median) + splitAngle / 2) * dir * small_bullet_attr.speed;
            }
        }
    }
}
    
