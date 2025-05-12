using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BulletSpawnerData
{
    // 基础属性
    public float attackSpeed;
    public float attackRange;
    public float maxAttackSpeed;
    public float minAttackSpeed;
    public float maxLoadSpeed;
    public float minLoadSpeed;
    public float initAttackSpeed;

    // 伤害加成
    public float permanent_damage_add_bonus = 0;
    public float temporary_damage_add_bonus_sum = 0;
    public List<Food.Bonus> temporary_damage_add_bonus_list = new List<Food.Bonus>();

    public float permanent_damage_percentage_bonus = 0;
    public float temporary_damage_percentage_bonus_sum = 0;
    public List<Food.Bonus> temporary_damage_percentage_bonus_list = new List<Food.Bonus>();

    // 攻击速度加成
    public float bonus_attack_speed = 0;
    public float permanent_attack_speed_bonus = 0;
    public float temporary_attack_speed_bonus_sum = 0;
    public List<Food.Bonus> temporary_attack_speed_bonus_list = new List<Food.Bonus>();

    // 攻击范围加成
    public float permanent_attack_range_bonus = 0;
    public float temporary_attack_range_bonus_sum = 0;
    public List<Food.Bonus> temporary_attack_range_bonus_list = new List<Food.Bonus>();
}

public class BulletSpawner : MonoBehaviour
{
    private const string bulletSpawnerSaveDataPath = "bulletSpawnerData.json";
    public static BulletSpawner Instance { get; set; } = null;
    private readonly object bulletQueueLock = new object();
    private readonly object enemyInRangeLock = new object(); // 用来锁定敌人列表
    #region 组件属性
    ///子弹预设体
    private Dictionary<Bullet.SingleBulletType, GameObject> bulletPrefabs = new();
    //子弹发射器
    [Header("枪械设置")]
    [Tooltip("子弹攻击速度")] public float attackSpeed;
    // [Tooltip("子弹装载速度")] public float loadSpeed;
    [Tooltip("子弹攻击范围")] public float attackRange;
    // [Tooltip("初始发射子弹数量")] public int init_count;
    // [Tooltip("初始发射子弹类型")] public Bullet.SingleBulletType init_bulletType;
    [SerializeField][Tooltip("是否进行自动攻击")] private bool attackFlag;
    private readonly LinkedList<EnemyController> enemyInRange = new(); // 用来存储范围内的敌人
    private readonly Queue<Bullet.SingleBulletType> bullets = new();  // 用来存储子弹的队列

    private bool isOnFire = false;  // 用来判断是否脚本被启动
    private bool isFiring = false; // 用来判断Fire协程是否正在运行
    public UnityEvent fireEvent;    //发射事件
    public TextMeshProUGUI bulletCountText; // 用于显示子弹数量的文本
    #endregion

    #region 属性加成
    [Header("极限属性")]
    [Tooltip("最快攻击速度")] public float maxAttackSpeed;
    [Tooltip("最慢攻击速度")] public float minAttackSpeed;
    [Tooltip("初始子弹最大装载速度")] public float maxLoadSpeed;
    [Tooltip("初始子弹最小装载速度")] public float minLoadSpeed;
    [Header("初始属性")]
    private float initAttackSpeed;
    // private float initLoadSpeed;


    [Header("攻击伤害加成")]
    // 伤害计算：（基础+临时加+永久加）* （1 + 临时乘 + 永久乘）
    private float permanent_damage_add_bonus = 0;   // 永久伤害加成
    private float temporary_damage_add_bonus_sum = 0;   // 临时伤害加成
    [SerializeField] private LinkedList<Food.Bonus> temporary_damage_add_bonus = new();   // 临时伤害加成，波次结束后消失
    private float permanent_damage_percentage_bonus = 0;   // 永久伤害加成
    private float temporary_damage_percentage_bonus_sum = 0;   // 临时伤害加成
    [SerializeField] private LinkedList<Food.Bonus> temporary_damage_percentage_bonus = new();   // 临时伤害加成，波次结束后消失
    private float next_shoot_damage_add_bonus = 0;   // 下次发射伤害加成
    private float next_shoot_damage_percentage_bonus = 0;   // 下次发射伤害加成

    [Header("攻击速度加成")]
    private float permanent_attack_speed_bonus = 0;   // 记录永久攻击速度加成
    private float temporary_attack_speed_bonus_sum = 0;   // 记录临时攻击速度加成
    [SerializeField] private LinkedList<Food.Bonus> temporary_attack_speed_bonus = new();   // 临时攻击速度加成，波次结束后消失
    private float bonus_attack_speed;

    [Header("攻击范围加成")]
    public float permanent_attack_range_bonus = 0;
    public float temporary_attack_range_bonus_sum = 0;
    public LinkedList<Food.Bonus> temporary_attack_range_bonus = new();

    [Header("添加基于时间的临时加成")]
    private LinkedList<(Food.FoodBonusType, Food.Bonus)> temporary_time_bonus = new();

    // [Header("装载速度加成")]
    // private float permanent_load_speed_bonus = 0;   // 记录永久装载速度加成
    // private float temporary_load_speed_bonus_sum = 0;   // 记录临时装载速度加成
    // [SerializeField] private LinkedList<Food.Bonus> temporary_load_speed_bonus = new();   // 临时装载速度加成，波次结束后消失

    #endregion


    // 添加锁对象
    #region 核心逻辑控制
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        initAttackSpeed = attackSpeed;
        // initLoadSpeed = loadSpeed;

        // bulletCountText = GameObject.FindWithTag("BulletCountText").GetComponent<TextMeshPro>();
    }

    public void StartFire()
    {
        Debug.Log("开始发射");
        attackFlag = true;
        isOnFire = true;

        StartCoroutine(DectEnemyInRange());
        StartCoroutine(ScanTemporyBonusList());
        // StartCoroutine(LoadBullet());
    }

    public void EndFire()
    {
        attackFlag = false;
        isOnFire = false;
        enemyInRange.Clear();
        bullets.Clear();
        StopAllCoroutines();

        DecreaseTemporaryBonus();
    }

    void Update()
    {
        if (PlayerController.Instance.Live() == false || TimeController.Instance.TimeUp())
        {
            attackFlag = false;
            enemyInRange.Clear();
            bullets.Clear();
            Debug.Log("Player is dead or time is up, stop firing.");
            return;
        }

        bool hasEnemies;
        lock (enemyInRangeLock)
        {
            hasEnemies = enemyInRange.Count != 0;
        }

        // 更新子弹数量文本
        if (bulletCountText != null)
        {
            bulletCountText.text = bullets.Count.ToString();
        }
        else
        {
            // Debug.LogError("BulletCountText is null");
            bulletCountText = GameObject.FindWithTag("BulletCountText").GetComponent<TextMeshProUGUI>();
        }

        if (attackFlag && hasEnemies && bullets.Count != 0 && isOnFire && !isFiring)
        {
            StartCoroutine(Fire());
            Debug.Log("start fire coroutine");
            isFiring = false; // 设置为false，避免重复调用
        }
    }

    private void OnDestroy()
    {
        // 清理单例引用
        if (Instance == this)
            Instance = null;
            
        // 停止所有协程
        StopAllCoroutines();
        
        // 清空集合
        bullets.Clear();
        enemyInRange.Clear();
        temporary_time_bonus.Clear();
        temporary_damage_add_bonus.Clear();
        temporary_damage_percentage_bonus.Clear();
        temporary_attack_speed_bonus.Clear();
        
        // 清空字典
        bulletPrefabs.Clear();
    }

    // 修改 LoadBullet 协程
    // public IEnumerator LoadBullet()
    // {
    //     while (isOnFire)
    //     {
    //         yield return new WaitForSeconds(loadSpeed);
    //         lock (bulletQueueLock)
    //         {
    //             // Debug.Log("加载初始子弹");
    //             for (int i = 0; i < init_count; i++)
    //             {
    //                 bullets.Enqueue(init_bulletType);
    //             }
    //         }
    //     }
    // }
    public void LoadBullet(Bullet.SingleBulletType bulletType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Debug.Log("LoadBullet: " + bulletType);
            lock (bulletQueueLock)
            {
                bullets.Enqueue(bulletType);
            }
        }
        // Debug.Log("has " + bullets.Count + " bullets in the queue.");
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

        Queue<Bullet.SingleBulletType> copy_bullets;

        // 原子操作：检查并复制子弹队列
        lock (bulletQueueLock)
        {
            if (bullets.Count == 0)
                yield break;

            copy_bullets = new Queue<Bullet.SingleBulletType>(bullets);
            bullets.Clear();
        }

        int cnt = 0; //发射数量

        //存储有效敌人
        LinkedList<Transform> enemies = new();
        lock (enemyInRangeLock)
        {
            var temp_node = enemyInRange.First;
            while (temp_node != null)
            {
                if (temp_node.Value != null && temp_node.Value.Live())
                    enemies.AddLast(temp_node.Value.transform);
                temp_node = temp_node.Next;
            }
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
            if (target == null)
            {
                enemies.Remove(node);
                node = node.Next;
                continue;
            }

            // 获得子弹类型及预制体
            Bullet.SingleBulletType bulletType = copy_bullets.Dequeue();
            GameObject bulletPrefab = LoadBulletPrefab(bulletType);
            if (bulletPrefab == null)
            {
                Debug.LogError("子弹预设体" + bulletType + "加载失败");
                yield break;
            }

            // 发射子弹--火箭弹单独处理
            if (bulletType != Bullet.SingleBulletType.Rocket_Bullet)
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
        next_shoot_damage_add_bonus = 0; // 重置下次发射伤害加成
        next_shoot_damage_percentage_bonus = 0; // 重置下次发射伤害加成
        StartCoroutine(SetFireFlagFalse(attackSpeed));
        fireEvent.Invoke();
    }

    private IEnumerator DectEnemyInRange()
    {
        while (PlayerController.Instance == null || TimeController.Instance == null)
            yield return null;
        while (isOnFire)
        {
            if (PlayerController.Instance.Live() && !TimeController.Instance.TimeUp())
            {
                // Wait for a short time before checking again
                yield return new WaitForSeconds(.5f);
                lock (enemyInRangeLock)
                {
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
                        
                    // Debug.Log("检测敌人数量：" + enemyInRange.Count);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    public GameObject LoadBulletPrefab(Bullet.SingleBulletType bulletType)
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
    // 临时属性加成：获得时加入到链表中，并在属性中加成；回合结束时减少回合数，回合数为0时移除，并从属性中减去加成值；
    //              并在每回合结束时清除过期的加成    

    public void NextShootDamageUpAdd(float val) => next_shoot_damage_add_bonus += val;
    public void NextShootDamageUpPercent(float val) => next_shoot_damage_percentage_bonus += val;
    // 通用加成方法
    public void AddBonus(Food.FoodBonusType type, float value, Food.FoodDurationType foodDurationType, float rounds = 1f)
    {
        switch (type)
        {
            case Food.FoodBonusType.AttackDamageByValue:
                if (foodDurationType == Food.FoodDurationType.Permanent)
                    permanent_damage_add_bonus += value;
                else if (foodDurationType == Food.FoodDurationType.TemporaryRounds)
                {
                    temporary_damage_add_bonus.AddLast(new Food.Bonus(value, rounds));
                    temporary_damage_add_bonus_sum += value;
                }
                else if (foodDurationType == Food.FoodDurationType.TemporaryTime)
                {
                    temporary_time_bonus.AddLast((type, new Food.Bonus(value, rounds)));
                    temporary_damage_add_bonus_sum += value;
                }
                break;
            case Food.FoodBonusType.AttackDamageByPercent:
                if (foodDurationType == Food.FoodDurationType.Permanent)
                    permanent_damage_percentage_bonus += value;
                else if (foodDurationType == Food.FoodDurationType.TemporaryRounds)
                {
                    temporary_damage_percentage_bonus.AddLast(new Food.Bonus(value, rounds));
                    temporary_damage_percentage_bonus_sum += value;
                }
                else if (foodDurationType == Food.FoodDurationType.TemporaryTime)
                {
                    temporary_time_bonus.AddLast((type, new Food.Bonus(value, rounds)));
                    temporary_damage_percentage_bonus_sum += value;
                }
                break;
            case Food.FoodBonusType.AttackSpeed:
                switch (foodDurationType)
                {
                    case Food.FoodDurationType.Permanent:
                        permanent_attack_speed_bonus += value;
                        break;
                    case Food.FoodDurationType.TemporaryRounds:
                        temporary_attack_speed_bonus.AddLast(new Food.Bonus(value, rounds));
                        temporary_attack_speed_bonus_sum += value;
                        break;
                    case Food.FoodDurationType.TemporaryTime:
                        temporary_time_bonus.AddLast((type, new Food.Bonus(value, rounds)));
                        break;
                    default:
                        Debug.LogError("AddBonus: Invalid DurationType");
                        break;
                }
                bonus_attack_speed += value;
                attackSpeed = Mathf.Clamp(bonus_attack_speed, minAttackSpeed, maxAttackSpeed);
                break;
            case Food.FoodBonusType.AttackRange:
                attackRange += value;
                switch (foodDurationType)
                {
                    case Food.FoodDurationType.Permanent:
                        permanent_attack_range_bonus += value;
                        break;
                    case Food.FoodDurationType.TemporaryRounds:
                        temporary_attack_range_bonus.AddLast(new Food.Bonus(value, rounds));
                        temporary_attack_range_bonus_sum += value;
                        break;
                    case Food.FoodDurationType.TemporaryTime:
                        temporary_time_bonus.AddLast((type, new Food.Bonus(value, rounds)));
                        break;
                    default:
                        Debug.LogError("AddBonus: Invalid DurationType");
                        break;
                }
                break;
            default:
                Debug.LogError("AddBonus: Invalid BonusType");
                break;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void DealWithBonus(Bullet bullet)
    {
        if (bullet == null)
        {
            Debug.LogError("DealWithBonus: bullet is null");
            return;
        }
        float dmg = bullet.bulletBasicAttribute.damage;
        bullet.bulletBasicAttribute.damage = (dmg + permanent_damage_add_bonus + temporary_damage_add_bonus_sum + next_shoot_damage_add_bonus) * (1 + permanent_damage_percentage_bonus + temporary_damage_percentage_bonus_sum + next_shoot_damage_percentage_bonus);
    }

    private IEnumerator ScanTemporyBonusList()
    {
        yield return new WaitForSeconds(.5f);
        var currentNode = temporary_time_bonus.First;
        while (currentNode != null)
        {
            var timeLeft = currentNode.Value.Item2.timeLeft;
            timeLeft -= 0.5f;
            if (timeLeft <= 0)
            {
                temporary_time_bonus.Remove(currentNode);
                switch (currentNode.Value.Item1)
                {
                    case Food.FoodBonusType.AttackSpeed:
                        bonus_attack_speed -= currentNode.Value.Item2.bonusValue;
                        attackSpeed = Mathf.Clamp(bonus_attack_speed, minAttackSpeed, maxAttackSpeed);
                        break;
                    case Food.FoodBonusType.AttackDamageByValue:
                        temporary_damage_add_bonus_sum -= currentNode.Value.Item2.bonusValue;
                        break;
                    case Food.FoodBonusType.AttackDamageByPercent:
                        temporary_damage_percentage_bonus_sum -= currentNode.Value.Item2.bonusValue;
                        break;
                    case Food.FoodBonusType.AttackRange:
                        attackRange -= currentNode.Value.Item2.bonusValue;
                        break;
                    default:
                        Debug.LogError($"加成类型{currentNode.Value.Item1}在角色处不支持");
                        break;
                }
            }
            currentNode = currentNode.Next;
        }
    }

    public void DecreaseTemporaryBonus()
    {
        // 处理过期的临时加成
        var temporary_damage_add_bonus_decrease = temporary_damage_add_bonus.DecreaseRounds();
        var temporary_damage_percentage_bonus_decrease = temporary_damage_percentage_bonus.DecreaseRounds();
        var temporary_attack_speed_bonus_decrease = temporary_attack_speed_bonus.DecreaseRounds();
        var temporary_attack_range_bonus_decrease = temporary_attack_range_bonus.DecreaseRounds();
        // temporary_load_speed_bonus_sum -= temporary_load_speed_bonus.DecreaseRounds();

        // 清除本回合限时加成
        var currentNode = temporary_time_bonus.First;
        while (currentNode != null)
        {
            var nextNode = currentNode.Next;
            switch (currentNode.Value.Item1)
            {
                case Food.FoodBonusType.AttackDamageByValue:
                    temporary_damage_add_bonus_decrease += currentNode.Value.Item2.bonusValue;
                    break;
                case Food.FoodBonusType.AttackDamageByPercent:
                    temporary_damage_percentage_bonus_decrease += currentNode.Value.Item2.bonusValue;
                    break;
                case Food.FoodBonusType.AttackSpeed:
                    bonus_attack_speed -= currentNode.Value.Item2.bonusValue;
                    break;
                case Food.FoodBonusType.AttackRange:
                    temporary_attack_range_bonus_decrease += currentNode.Value.Item2.bonusValue;
                    break;
                default:
                    Debug.LogError($"加成类型{currentNode.Value.Item1}在BulletSpawner中不支持");
                    break;
            }
            currentNode = nextNode;
        }
        temporary_time_bonus.Clear();

        temporary_damage_add_bonus_sum -= temporary_damage_add_bonus_decrease;
        temporary_damage_percentage_bonus_sum -= temporary_damage_percentage_bonus_decrease;
        temporary_attack_speed_bonus_sum -= temporary_attack_speed_bonus_decrease;
        bonus_attack_speed -= temporary_attack_speed_bonus_decrease;
        // 恢复属性值
        attackSpeed = Mathf.Clamp(bonus_attack_speed, minAttackSpeed, maxAttackSpeed);
        attackRange -= temporary_attack_range_bonus_decrease;
        // loadSpeed = Mathf.Clamp(initLoadSpeed + permanent_load_speed_bonus + temporary_load_speed_bonus_sum, minLoadSpeed, maxLoadSpeed);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
    #endif
    }
    // 添加清除所有临时加成的方法，供编辑器使用
    public void ClearTemporaryBonus()
    {
        // 恢复原始属性值
        attackSpeed = Mathf.Min(attackSpeed + temporary_attack_speed_bonus_sum, initAttackSpeed);
        // loadSpeed = Mathf.Min(loadSpeed + temporary_load_speed_bonus_sum, initLoadSpeed);

        // 清空所有临时加成
        temporary_damage_add_bonus.Clear();
        temporary_damage_add_bonus_sum = 0;

        temporary_damage_percentage_bonus.Clear();
        temporary_damage_percentage_bonus_sum = 0;

        temporary_attack_speed_bonus.Clear();
        temporary_attack_speed_bonus_sum = 0;

        // temporary_load_speed_bonus.Clear();
        // temporary_load_speed_bonus_sum = 0;

        temporary_attack_range_bonus.Clear();
        temporary_attack_range_bonus_sum = 0;

        temporary_time_bonus.Clear();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    #endregion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #region 存档系统
    /// <summary>
    /// 保存子弹发射器数据到文件
    /// </summary>
    public void StoreBulletData()
    {
        BulletSpawnerData bulletSpawnerData = new BulletSpawnerData
        {
            // 基础属性
            attackSpeed = attackSpeed,
            attackRange = attackRange,
            maxAttackSpeed = maxAttackSpeed,
            minAttackSpeed = minAttackSpeed,
            maxLoadSpeed = maxLoadSpeed,
            minLoadSpeed = minLoadSpeed,
            initAttackSpeed = initAttackSpeed,
            
            // 伤害加成
            permanent_damage_add_bonus = permanent_damage_add_bonus,
            temporary_damage_add_bonus_sum = temporary_damage_add_bonus_sum,
            permanent_damage_percentage_bonus = permanent_damage_percentage_bonus,
            temporary_damage_percentage_bonus_sum = temporary_damage_percentage_bonus_sum,

            // 攻击速度加成
            bonus_attack_speed = bonus_attack_speed,
            permanent_attack_speed_bonus = permanent_attack_speed_bonus,
            temporary_attack_speed_bonus_sum = temporary_attack_speed_bonus_sum
        };
        
        // 将LinkedList转换为List以便序列化
        bulletSpawnerData.temporary_damage_add_bonus_list = new List<Food.Bonus>(temporary_damage_add_bonus);
        bulletSpawnerData.temporary_damage_percentage_bonus_list = new List<Food.Bonus>(temporary_damage_percentage_bonus);
        bulletSpawnerData.temporary_attack_speed_bonus_list = new List<Food.Bonus>(temporary_attack_speed_bonus);
        bulletSpawnerData.temporary_attack_range_bonus_list = new List<Food.Bonus>(temporary_attack_range_bonus);

        // 将数据转换为JSON
        string jsonData = JsonUtility.ToJson(bulletSpawnerData, true);
        
        // 保存到文件
        string filePath = Path.Combine(Application.persistentDataPath, bulletSpawnerSaveDataPath);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("子弹系统数据已保存到: " + filePath);
    }
    
    /// <summary>
    /// 从文件加载子弹发射器数据
    /// </summary>
    public void LoadBulletData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, bulletSpawnerSaveDataPath);
        
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            BulletSpawnerData loadedData = JsonUtility.FromJson<BulletSpawnerData>(jsonData);
            
            // 应用基础属性
            attackSpeed = loadedData.attackSpeed;
            attackRange = loadedData.attackRange;
            maxAttackSpeed = loadedData.maxAttackSpeed;
            minAttackSpeed = loadedData.minAttackSpeed;
            maxLoadSpeed = loadedData.maxLoadSpeed;
            minLoadSpeed = loadedData.minLoadSpeed;
            initAttackSpeed = loadedData.initAttackSpeed;
            
            // 应用伤害加成
            permanent_damage_add_bonus = loadedData.permanent_damage_add_bonus;
            temporary_damage_add_bonus_sum = loadedData.temporary_damage_add_bonus_sum;
            permanent_damage_percentage_bonus = loadedData.permanent_damage_percentage_bonus;
            temporary_damage_percentage_bonus_sum = loadedData.temporary_damage_percentage_bonus_sum;

            // 应用攻击速度加成
            bonus_attack_speed = loadedData.bonus_attack_speed;
            permanent_attack_speed_bonus = loadedData.permanent_attack_speed_bonus;
            temporary_attack_speed_bonus_sum = loadedData.temporary_attack_speed_bonus_sum;

            // 应用攻击范围加成
            permanent_attack_range_bonus = loadedData.permanent_attack_range_bonus;
            temporary_attack_range_bonus_sum = loadedData.temporary_attack_range_bonus_sum;

            // 清空并重建临时加成链表
            temporary_damage_add_bonus.Clear();
            foreach (var bonus in loadedData.temporary_damage_add_bonus_list)
            {
                temporary_damage_add_bonus.AddLast(bonus);
            }
            
            temporary_damage_percentage_bonus.Clear();
            foreach (var bonus in loadedData.temporary_damage_percentage_bonus_list)
            {
                temporary_damage_percentage_bonus.AddLast(bonus);
            }
            
            temporary_attack_speed_bonus.Clear();
            foreach (var bonus in loadedData.temporary_attack_speed_bonus_list)
            {
                temporary_attack_speed_bonus.AddLast(bonus);
            }
            
            temporary_attack_range_bonus.Clear();
            foreach (var bonus in loadedData.temporary_attack_range_bonus_list)
            {
                temporary_attack_range_bonus.AddLast(bonus);
            }

            Debug.Log("子弹系统数据已从以下位置加载: " + filePath);
        }
        else
        {
            Debug.LogWarning("未找到子弹系统数据文件: " + filePath);
        }
    }
    #endregion
}
