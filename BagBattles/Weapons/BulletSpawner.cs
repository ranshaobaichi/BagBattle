using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BulletSpawner : MonoBehaviour
{
    #region 组件属性
    ///子弹预设体
    private Dictionary<Bullet.BulletType, GameObject> bulletPrefabs = new();

    //子弹发射器
    [Header("枪械设置")]
    [Tooltip("子弹攻击速度")] public float attackSpeed;
    [Tooltip("子弹攻击范围")] public float attackRange;
    [Tooltip("初始发射数量")] public int attackCount;
    private float attackTimer;
    private bool attackFlag;
    private readonly LinkedList<EnemyController> enemyInRange = new();

    public UnityEvent fireEvent;    //发射事件
    #endregion
    void Start()
    {
        attackTimer = attackSpeed;
        attackFlag = true;
        StartCoroutine(DectEnemyInRange());
    }

    void Update()
    {
        //如果角色死亡或时间到，直接返回
        if (PlayerController.Instance.Live() == false || TimeController.Instance.TimeUp())
            return;

        //如果敌人不在范围内，或未到攻击时间，直接返回
        if (attackFlag)
        {
            //发射子弹
            if (Fire(Bullet.BulletType.Normal_Bullet, attackCount))
            {
                fireEvent.Invoke(); //发射事件
                attackFlag = false;
                attackTimer = attackSpeed;
            }
        }
            else
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                    attackFlag = true;
            }

    }

    public bool Fire(Bullet.BulletType bulletType, int count = 1)
    {
        var node = enemyInRange.First;
        int cnt = 0; //发射数量
        
        while (cnt < count && node != null)  // 添加了node != null检查，防止无限循环
        {
            EnemyController target = null;
            while (node != null)
            {
                target = node.Value;
                var temp = node;
                node = node.Next;
                if (target == null || !target.Live())
                {
                    enemyInRange.Remove(temp);
                    target = null;
                }
                else
                    break;
            }

            if (target != null)
            {
                GameObject bulletPrefab = LoadBulletPrefab(bulletType);
                if (bulletPrefab == null)
                {
                    Debug.LogError("子弹预设体" + bulletType + "加载失败");
                    return false;
                }
                GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
                bullet.transform.position = transform.position;
                bullet.GetComponent<Bullet>().SetSpeed((target.transform.position - transform.position).normalized);
                cnt++;
            }
            else
            {
                // 没有更多有效目标，退出循环
                break;
            }
        }

        return cnt != 0;
    }

    private IEnumerator DectEnemyInRange()
    {
        while (PlayerController.Instance == null || TimeController.Instance == null)
            yield return null;
        while (PlayerController.Instance.Live() && !TimeController.Instance.TimeUp())
        {
            // Wait for a short time before checking again
            yield return new WaitForSeconds(.5f);
            enemyInRange.Clear();
            Collider2D[] colliders = new Collider2D[30];
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, colliders);
            List<EnemyController> enemyList = new List<EnemyController>();

            for (int i = 0; i < count; i++)
            {
                Collider2D collider = colliders[i];
                if (collider.CompareTag("Enemy"))
                {
                    EnemyController enemy = collider.GetComponent<EnemyController>();
                    if (enemy != null && enemy.Live())
                        enemyList.Add(enemy);
                }
            }

            // Sort enemies by descending distance (largest distance first)
            enemyList.Sort((a, b) =>
                Vector2.Distance(transform.position, b.transform.position)
                .CompareTo(Vector2.Distance(transform.position, a.transform.position))
            );

            foreach (var enemy in enemyList)
                enemyInRange.AddLast(enemy);
        }
    }

    public GameObject LoadBulletPrefab(Bullet.BulletType bulletType)
    {
        if (bulletPrefabs.ContainsKey(bulletType))
            return bulletPrefabs[bulletType];
        else
        {
            GameObject bullet = Resources.Load<GameObject>("Bullets/" + bulletType.ToString());
            if (bullet != null)
            {
                bulletPrefabs.Add(bulletType, bullet);
                return bullet;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
