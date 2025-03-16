using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BulletSpawner : MonoBehaviour
{
    public static BulletSpawner Instance { get; private set; } = null;
    #region 组件属性
    ///子弹预设体
    private Dictionary<Bullet.BulletType, GameObject> bulletPrefabs = new();
    //子弹发射器
    [Header("枪械设置")]
    [Tooltip("子弹攻击速度")] public float attackSpeed;
    [Tooltip("子弹装载速度")] public float loadSpeed;
    [Tooltip("子弹攻击范围")] public float attackRange;
    [Tooltip("初始发射普通子弹数量")]public int init_count;
    [SerializeField] private bool attackFlag;    //是否进行自动攻击
    private readonly LinkedList<EnemyController> enemyInRange = new();
    private readonly Queue<Bullet.BulletType> bullets = new();

    private bool isOnFire = false;  // 用来判断是否脚本被启动
    private bool isFiring = false; // 用来判断Fire协程是否正在运行
    public UnityEvent fireEvent;    //发射事件
    #endregion

    // 添加锁对象
    private readonly object bulletQueueLock = new object();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public void StartFire()
    {
        attackFlag = true;
        isOnFire = true;
        StartCoroutine(DectEnemyInRange());
        StartCoroutine(LoadBullet());
    }

    public void EndFire()
    {
        attackFlag = false;
        isOnFire = false;
        enemyInRange.Clear();
        bullets.Clear();
        StopAllCoroutines();
    }

    void FixedUpdate()
    {
        if (PlayerController.Instance.Live() == false || TimeController.Instance.TimeUp())
        {
            attackFlag = false;
            enemyInRange.Clear();
            bullets.Clear();
            return;
        }
        if (attackFlag && enemyInRange.Count != 0 && bullets.Count != 0 && isOnFire && !isFiring)
        {
            StartCoroutine(Fire());
            isFiring = false; // 设置为false，避免重复调用
        }
    }

    // 修改 LoadBullet 协程
    public IEnumerator LoadBullet()
    {
        while (isOnFire)
        {
            yield return new WaitForSeconds(loadSpeed);
            lock (bulletQueueLock)
            {
                for (int i = 0; i < init_count; i++)
                {
                    bullets.Enqueue(Bullet.BulletType.Normal_Bullet);
                }
            }
        }
    }
    public void LoadBullet(Bullet.BulletType bulletType, int count)
    {
        if (count == -1)
            count = init_count;
        for (int i = 0; i < count; i++)
        {
            bullets.Enqueue(bulletType);
        }
    }
    //将发射标志设置为false，延迟time后再设置为true
    public IEnumerator SetFireFlagFalse(float time)
    {
        if (!isOnFire)
            yield break;
        yield return new WaitForSeconds(time);
        attackFlag = true;
        isFiring = false; // 设置为false，允许下一次发射
    }

    //将bullets队列中的子弹发射出去
    // 修改 Fire 协程
    public IEnumerator Fire()
    {
        if (!attackFlag || enemyInRange.Count == 0 || !isOnFire)
            yield break;
            
        Queue<Bullet.BulletType> copy_bullets;
        
        // 原子操作：检查并复制子弹队列
        lock (bulletQueueLock)
        {
            if (bullets.Count == 0)
                yield break;
                
            copy_bullets = new Queue<Bullet.BulletType>(bullets);
            bullets.Clear();
        }
        
        int cnt = 0; //发射数量

        //存储有效敌人
        LinkedList<Transform> enemies = new();
        var temp_node = enemyInRange.First;
        while (temp_node != null)
        {
            if (temp_node.Value != null && temp_node.Value.Live())
                enemies.AddLast(temp_node.Value.transform);
            temp_node = temp_node.Next;
        }

        var node = enemies.First; // 获取第一个敌人
        while (copy_bullets.Count != 0)
        {
            if (node == null)
            {
                if(enemies.Count == 0)
                    break; // 如果没有敌人了，退出循环
                node = enemies.First; // 如果node为空，则重新获取第一个敌人
            }
            Transform target = node.Value;

            if(target == null)
            {
                enemies.Remove(node);
                node = node.Next;
                continue;
            }
            Bullet.BulletType bulletType = copy_bullets.Dequeue();
            GameObject bulletPrefab = LoadBulletPrefab(bulletType);
            if (bulletPrefab == null)
            {
                Debug.LogError("子弹预设体" + bulletType + "加载失败");
                yield break;
            }
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position;
            bullet.GetComponent<Bullet>().SetSpeed((target.transform.position - transform.position).normalized);

            node = node.Next; // 获取下一个敌人
            cnt++;
            attackFlag = false;
            yield return new WaitForSeconds(0.05f); // 等待0.05秒
        }

        Debug.Log("发射数量：" + cnt);
        StartCoroutine(SetFireFlagFalse(attackSpeed));
        fireEvent.Invoke();
    }
    
    private IEnumerator DectEnemyInRange()
    {
        while (PlayerController.Instance == null || TimeController.Instance == null)
            yield return null;
        while (PlayerController.Instance.Live() && !TimeController.Instance.TimeUp() && isOnFire)
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
