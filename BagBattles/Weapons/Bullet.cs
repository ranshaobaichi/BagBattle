using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
    [Serializable]
    public enum BulletType
    {
        None,
        Normal_Bullet,
        Spear_Bullet,
        Rocket_Bullet,
        Lightning_Bullet,
        Fire_Bullet,
        Ice_Bullet,
        Swallow_Bullet,
        Bomb_Bullet,
        Jump_Bullet,
        Split_Bullet
    };

    [Serializable] [Tooltip("子弹基础属性")]
    public struct BulletBasicAttribute
    {
        [Tooltip("弹速")] public float speed;
        [Tooltip("伤害")] public float damage;
        [Tooltip("穿透数")] public int bullet_pass_nums;
    }
    public GameObject explosionPrefab;
    public BulletType bulletType;
    [Tooltip("子弹基础属性")] public BulletBasicAttribute bulletBasicAttribute;
    new protected Rigidbody2D rigidbody;
    protected int current_pass_num;

    //FIXME:添加方便设置速度接口
    //fixme:添加局外更改伤害接口
    protected virtual void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void OnEnable()
    {
        current_pass_num = bulletBasicAttribute.bullet_pass_nums;
    }

    protected virtual void Update()
    {
        if(TimeController.Instance.TimeUp() || PlayerController.Instance.Live() == false)
        {
            rigidbody.velocity = Vector2.zero;
            StopAllCoroutines();
        }
    }

    public virtual void SetSpeed(Vector2 direction)
    {
        rigidbody.velocity = direction.normalized * bulletBasicAttribute.speed;
        // transform.rotation = Quaternion.LookRotation(direction);
    }

    public virtual void Del()
    {
        current_pass_num = bulletBasicAttribute.bullet_pass_nums;
        ObjectPool.Instance.PushObject(gameObject);
    }

    //FIXME:多个子弹碰撞同一敌人时，敌人伤害计算慢（未直接死亡），导致多个子弹同时销毁
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.GetComponent<EnemyController>().Live())
        {
            if (CauseDamage(other.gameObject.GetComponent<EnemyController>()))
            {
                current_pass_num--;
                if (current_pass_num < 0)
                    Del();
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Del();
        }
    }
    protected bool CauseDamage(EnemyController enemy)
    {
        float actual_damage = bulletBasicAttribute.damage;
        return enemy.TakeDamage(actual_damage);
    }
}
