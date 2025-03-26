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

    // 添加碰撞目标列表
    private List<EnemyController> collidedEnemies = new List<EnemyController>();
    private bool processingCollisions = false;

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

        // 每帧结束时处理碰撞
        if (collidedEnemies.Count > 0 && !processingCollisions)
        {
            StartCoroutine(ProcessCollisionsNextFrame());
        }
    }

    // 用于延迟处理碰撞的协程
    private IEnumerator ProcessCollisionsNextFrame()
    {
        processingCollisions = true;
        yield return null; // 等待下一帧

        // 处理所有收集到的碰撞
        foreach (var enemy in collidedEnemies)
        {
            if (enemy != null && enemy.Live())
            {
                if (CauseDamage(enemy))
                {
                    current_pass_num--;
                    if (current_pass_num < 0)
                    {
                        collidedEnemies.Clear();
                        processingCollisions = false;
                        Del();
                        yield break; // 提前退出
                    }
                }
            }
        }

        collidedEnemies.Clear();
        processingCollisions = false;
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
            EnemyController enemy = other.GetComponent<EnemyController>();
            // 只添加到列表中，不立即处理
            if (!collidedEnemies.Contains(enemy))
            {
                collidedEnemies.Add(enemy);
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
