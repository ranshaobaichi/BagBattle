using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_BulletScript : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // explosion
        // GameObject exp = ObjectPool.Instance.GetObject(explosionPrefab);
        // exp.transform.position = transform.position;
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case "Player":
                collision.gameObject.GetComponent<PlayerController>().TakeDamage(bulletBasicAttribute.damage);
                Del();
                break;
            case "Wall":
                Del();
                break;
        }
    }
}
