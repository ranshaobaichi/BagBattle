using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [Tooltip("初始发射子弹数量")] public int init_count;
    [Tooltip("初始发射子弹类型")] public Bullet.BulletType init_bulletType;
    [SerializeField] [Tooltip("是否进行自动攻击")] private bool attackFlag;
    private readonly LinkedList<EnemyController> enemyInRange = new(); // 用来存储范围内的敌人
    private readonly Queue<Bullet.BulletType> bullets = new();  // 用来存储子弹的队列

    private bool isOnFire = false;  // 用来判断是否脚本被启动
    private bool isFiring = false; // 用来判断Fire协程是否正在运行
    public UnityEvent fireEvent;    //发射事件

    #endregion

    #region 属性加成

    [Header("攻击伤害加成")]
    // 伤害计算：（基础+临时加+永久加）* （1 + 临时乘 + 永久乘）
    private float permanent_damage_add_bonus;   // 永久伤害加成
    private LinkedList<Food.Bonus> temporary_damage_add_bonus = new();   // 临时伤害加成，波次结束后消失
    private float permanent_damage_percentage_bonus;   // 永久伤害加成
    private LinkedList<Food.Bonus> temporary_damage_percentage_bonus = new();   // 临时伤害加成，波次结束后消失

    [Header("攻击速度加成")]
    private float permanent_attack_speed_bonus;   // 永久攻击速度加成
    private LinkedList<Food.Bonus> temporary_attack_speed_bonus = new();   // 临时攻击速度加成，波次结束后消失
    
    [Header("装载速度加成")]
    private float permanent_load_speed_bonus;   // 永久装载速度加成
    private LinkedList<Food.Bonus> temporary_load_speed_bonus = new();   // 临时装载速度加成，波次结束后消失

    #endregion


    // 添加锁对象
    private readonly object bulletQueueLock = new object();

    #region 核心逻辑控制
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public void StartFire()
    {
        Debug.Log("开始发射");
        attackFlag = true;
        isOnFire = true;

        // 获得临时加成
        attackSpeed += temporary_attack_speed_bonus.Sum(b => b.bonusValue);
        loadSpeed += temporary_load_speed_bonus.Sum(b => b.bonusValue);

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
                // Debug.Log("加载初始子弹");
                for (int i = 0; i < init_count; i++)
                {
                    bullets.Enqueue(init_bulletType);
                }
            }
        }
    }
    public void LoadBullet(Bullet.BulletType bulletType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Debug.Log("LoadBullet: " + bulletType);
            lock (bulletQueueLock)
            {
                bullets.Enqueue(bulletType);
            }
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
            // 错误检测
            if (node == null)
            {
                if (enemies.Count == 0)
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

            // 获得子弹类型及预制体
            Bullet.BulletType bulletType = copy_bullets.Dequeue();
            GameObject bulletPrefab = LoadBulletPrefab(bulletType);
            if (bulletPrefab == null)
            {
                Debug.LogError("子弹预设体" + bulletType + "加载失败");
                yield break;
            }

            // 发射子弹--火箭弹单独处理
            if (bulletType != Bullet.BulletType.Rocket_Bullet)
            {
                GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
                bullet.transform.position = transform.position;
                Bullet bulletComponent = bullet.GetComponent<Bullet>();
                bulletComponent.SetSpeed((target.transform.position - transform.position).normalized);
                DealWithBonus(bulletComponent);
            }
            else
            {
                FireRocket(bulletPrefab, bulletPrefab.GetComponent<Rocket_Bullet>().num, target.transform.position);
            }

            // 发射后处理
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

    private void FireRocket(GameObject bulletPrefab, int rocketNum, Vector2 targetPos)
    {
        // yield return new WaitForSeconds(delay);
        int median = rocketNum / 2;
        int rocketAngle = 15;
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

        for (int i = 0; i < rocketNum; i++)
        {
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position;

            if (rocketNum % 2 == 1)
            {
                bullet.transform.right = Quaternion.AngleAxis(rocketAngle * (i - median), Vector3.forward) * direction;
            }
            else
            {
                bullet.transform.right = Quaternion.AngleAxis(rocketAngle * (i - median) + rocketAngle / 2, Vector3.forward) * direction;
            }
            bullet.GetComponent<Rocket_Bullet>().SetTarget(targetPos);
            DealWithBonus(bullet.GetComponent<Bullet>());
        }
    }
    #endregion

    #region 加成处理及接口
    // 永久属性加成：获得时直接加到属性中；
    // 临时属性加成：获得时加入到链表中，回合结束时减少回合数，回合数为0时移除，每次回合开始时/开枪时获得
    // 伤害加成
    public void AddTemporaryAddDamage(float _damage, int _round = 1) => temporary_damage_add_bonus.AddLast(new Food.Bonus(_damage, _round));
    public void AddPermanentAddDamage(float _damage) => permanent_damage_add_bonus += _damage;
    public void AddTemporaryPercentageDamage(float _damage, int _round = 1) => temporary_damage_percentage_bonus.AddLast(new Food.Bonus(_damage, _round));
    public void AddPermanentPercentageDamage(float _damage) => permanent_damage_percentage_bonus += _damage;
    // 攻击速度加成
    public void AddTemporaryAttackSpeed(float _attackSpeed, int _round = 1) => temporary_attack_speed_bonus.AddLast(new Food.Bonus(_attackSpeed, _round));
    public void AddPermanentAttackSpeed(float _attackSpeed)
    {
        permanent_attack_speed_bonus += attackSpeed;
        attackSpeed += _attackSpeed;
    }
    // 装载速度加成
    public void AddTemporaryLoadSpeed(float _loadSpeed, int _round = 1) => temporary_load_speed_bonus.AddLast(new Food.Bonus(_loadSpeed, _round));
    public void AddPermanentLoadSpeed(float _loadSpeed)
    {
        permanent_load_speed_bonus += loadSpeed;
        loadSpeed += _loadSpeed;
    }

    private void DealWithBonus(Bullet bullet)
    {
        if (bullet == null)
        {
            Debug.LogError("DealWithBonus: bullet is null");
            return;
        }
        float dmg = bullet.bulletBasicAttribute.damage;
        float tempDamageAdd = temporary_damage_add_bonus.Sum(b => b.bonusValue);
        float tempDamageMul = temporary_damage_percentage_bonus.Sum(b => b.bonusValue);
        bullet.bulletBasicAttribute.damage = (dmg + permanent_damage_add_bonus + tempDamageAdd) * (1 + permanent_damage_percentage_bonus + tempDamageMul);
    }

    public void ClearTemporaryBonus()
    {
        // 清除临时加成
        attackSpeed -= temporary_attack_speed_bonus.Sum(b => b.bonusValue);
        loadSpeed -= temporary_load_speed_bonus.Sum(b => b.bonusValue);

        // 处理过期的临时加成
        temporary_damage_add_bonus.DecreaseRounds();
        temporary_damage_percentage_bonus.DecreaseRounds();
        temporary_attack_speed_bonus.DecreaseRounds();
        temporary_load_speed_bonus.DecreaseRounds();
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
