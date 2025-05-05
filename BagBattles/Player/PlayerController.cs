using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class PlayerData
{
    [Serializable] public class PlayerBonusData
    {
        public float permanent_speed_bonus;
        public LinkedList<Food.Bonus> temporary_speed_bonus;
        public float temporary_speed_bonus_sum = 0f; // 临时加成总和
    }

    public float init_speed;
    public float bonus_speed;
    public float speed;
    public float maxSpeed;
    public float minSpeed;
    public float invincible_time;
    public PlayerBonusData playerBonusData; // 角色属性加成数据
    public List<string> triggerGuids; // 触发器数据
}

public class PlayerController : MonoBehaviour
{
    const string playerSaveDataPath = "playerData.json";
    public static PlayerController Instance = null;
    #region 组件属性
    [Header("基础属性")]
    [Tooltip("角色移动速度")] public float speed;
    private float init_speed;
    // [SerializeField] private float currentSpeed;
    // [Tooltip("速度变化平滑率(值越大变化越快)")] private float speedLerpRate = 5f; // 速度插值速率
    [Tooltip("角色最大移动速度")] public float maxSpeed;
    [Tooltip("角色最小移动速度")] public float minSpeed;

    [Tooltip("受击无敌时间")] public float invincible_time;

    [Header("属性标志位")]
    private static bool live;
    protected bool invincible_flag;

    [Header("计时器")]
    protected float invincible_timer;

    [Header("组件")]
    private static Rigidbody2D rb;
    private Animator animator;

    [Header("Trigger&Item")]
    public List<TriggerItem> triggerItems = new List<TriggerItem>(); // 角色拥有的触发器
    public GameObject triggerGameObject; //承载触发器子物体
    // private List<Item> items = new(); // 角色拥有的物品
    private int face;
    #endregion

    #region 属性加成
    private float bonus_speed; // 角色加成速度 --> 所有加成无视限制时的数值
    private float permanent_speed_bonus;
    private LinkedList<Food.Bonus> temporary_speed_bonus = new();
    private float temporary_speed_bonus_sum = 0f; // 临时加成总和

    private LinkedList<(Food.FoodBonusType, Food.Bonus)> temporary_time_bonus = new();
    #endregion

    #region 对外接口
    public void AddTriggerItem(TriggerInventoryItem item)
    {
        Trigger.TriggerType type = item.GetTriggerType();
        switch (type)
        {
            case Trigger.TriggerType.ByTime:
                TimeTriggerItem timeTriggerItem = triggerGameObject.AddComponent<TimeTriggerItem>();
                timeTriggerItem.Initialize(item.inventoryID, item.GetSpecificType(), item.triggerItems);
                triggerItems.Add(timeTriggerItem);
                break;
            case Trigger.TriggerType.ByFireTimes:
                FireTriggerItem fireTriggerItem = triggerGameObject.AddComponent<FireTriggerItem>();
                fireTriggerItem.Initialize(item.inventoryID, item.GetSpecificType(), item.triggerItems);
                triggerItems.Add(fireTriggerItem);
                break;
            case Trigger.TriggerType.ByOtherTrigger:
                ByOtherTriggerItem otherTriggerItem = triggerGameObject.AddComponent<ByOtherTriggerItem>();
                otherTriggerItem.Initialize(item.inventoryID, item.GetSpecificType(), item.triggerItems);
                triggerItems.Add(otherTriggerItem);
                break;
            default:
                Debug.LogError($"触发器类型{type}不支持");
                break;
        }
    }

    public void Dead()
    {
        live = false;
        rb.velocity = Vector2.zero;
        PlayerPrefs.SetInt(PlayerPrefsKeys.HAS_REMAINING_GAME_KEY, 0);
        GetComponent<SpriteRenderer>().enabled = false;
        TimeController.Instance.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(DeadScene());
    }
    private IEnumerator DeadScene()
    {
        yield return new WaitForSeconds(1.5f);
        // 结束游戏
        SceneManager.LoadScene("EndScene");
        gameObject.SetActive(false);
    }

    public void FinishRound()
    {
        live = false;
        rb.velocity = Vector2.zero;
        StopAllCoroutines();
        gameObject.SetActive(false);

        BulletSpawner.Instance.EndFire();
        // 结束并清除触发器
        DestroyAllTriggers();
        // 移除临时加成
        DecreaseTemporaryBonus();
        HealthController.Instance.DecreaseTemporaryBonus();
    }
    public bool Live() => live; // 角色是否存活
    public void SetActive(bool active) => gameObject.SetActive(active); // 设置角色是否激活
    #endregion

    #region 角色控制
    private void OnEnable()
    {
        // 角色位置恢复
        transform.position = new Vector3(0, 0, 0);
        rb.velocity = Vector2.zero;
        live = true;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;
        // currentSpeed = 0;

        // 启动触发器
        foreach (var item in triggerItems)
        {
            Debug.Log("player trigger item: " + item.GetType());
            item.LaunchTrigger();
        }

        Debug.Log("player trigger item count: " + triggerItems.Count);
        // 启动枪械模组
        BulletSpawner.Instance.StartFire();
        Debug.Log("player bullet spawner: " + BulletSpawner.Instance.gameObject.name);
        StartCoroutine(ScanTemporyBonusList());
    }

    private void OnDisable()
    {
        // 禁用角色
        rb.velocity = Vector2.zero;
        live = false;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 1)
        {
            init_speed = speed;
            bonus_speed = init_speed;
        }
        rb.mass = 50f;
    }

    private void Update()
    {
        if (live == false) return;
        // 移动输入
        Vector3 move = new(0f, 0f, 0f)
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };
        bool isMoving = move.x != 0 || move.y != 0;

        // 角色朝向
        Quaternion flip = transform.rotation;
        if (isMoving)
        {
            flip.y = rb.velocity.x > 0 ? 180 : 0;
            face = (int)flip.y;
        }
        else
            flip.y = face;
        transform.rotation = flip;

        // 角色移动
        // 平滑过渡到目标速度
        // if (isMoving)
        //     currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * speedLerpRate);
        // else
        //     currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * speedLerpRate);
        // Vector2 v = (Vector2)move.normalized;
        // rb.velocity = v * currentSpeed;
        // if (currentSpeed > speed * 2f / 3f)
        //     animator.SetBool("IsWalking", true);
        // else
        //     animator.SetBool("IsWalking", false);

        rb.velocity = move.normalized * speed;
        if (isMoving)
            animator.SetBool("IsWalking", true);
        else
            animator.SetBool("IsWalking", false);
        // 更新状态
        Update_status();
    }

    protected void Update_status()
    {
        if (live == true)
        {
            //invincible timer
            if (invincible_flag == true)
            {
                invincible_timer += Time.deltaTime;
                if (invincible_timer > invincible_time)
                {
                    invincible_flag = false;
                    invincible_timer = 0f;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (invincible_flag == false)
        {
            invincible_flag = true;
            HealthController.Instance.TakeDamage((int)damage);
        }
    }

    public void DestroyAllTriggers()
    {
        foreach (var item in triggerItems)
            item.Destroy();
        triggerItems.Clear();
        Destroy(triggerGameObject);
        triggerGameObject = new GameObject("TriggerGameObject");
        triggerGameObject.transform.SetParent(transform);
    }
    #endregion

    #region 属性接口
    // 速度属性
    public void AddBonus(FoodItemAttribute.BasicFoodAttribute foodItemAttribute)
    {
        Food.FoodBonusType type = foodItemAttribute.foodBonusType;
        float value = foodItemAttribute.foodBonusValue;
        Food.FoodDurationType foodDurationType = foodItemAttribute.foodDurationType;
        float timeLeft = foodItemAttribute.timeLeft;

        switch (type)
        {
            // 角色加成
            case Food.FoodBonusType.Speed:
                switch (foodDurationType)
                {
                    case Food.FoodDurationType.Permanent:
                        permanent_speed_bonus += value;
                        break;
                    case Food.FoodDurationType.TemporaryRounds:
                        temporary_speed_bonus.AddLast(new Food.Bonus(value, timeLeft));
                        temporary_speed_bonus_sum += value;
                        break;
                    case Food.FoodDurationType.TemporaryTime:
                        temporary_time_bonus.AddLast((type, new Food.Bonus(value, timeLeft)));
                        Debug.Log($"加成类型{type}剩余时间: {timeLeft}");
                        break;
                }
                bonus_speed += value;
                speed = Mathf.Clamp(bonus_speed, minSpeed, maxSpeed);
                break;

            // 枪械子弹加成
            case Food.FoodBonusType.AttackDamageByValue:
            case Food.FoodBonusType.AttackDamageByPercent:
            case Food.FoodBonusType.AttackSpeed:
            case Food.FoodBonusType.LoadSpeed:
            case Food.FoodBonusType.AttackRange:
                BulletSpawner.Instance.AddBonus(type, value, foodDurationType, timeLeft);
                break;

            // 角色血量、护甲加成
            case Food.FoodBonusType.HealthUp:
            case Food.FoodBonusType.HealthRecover:
            case Food.FoodBonusType.HealthDown:
            case Food.FoodBonusType.ArmorUp:
                HealthController.Instance.AddBonus(type, value, foodDurationType, timeLeft);
                break;
            default:
                Debug.LogError($"加成类型{type}不支持");
                break;
        }
    }

    private IEnumerator ScanTemporyBonusList()
    {
        yield return new WaitForSeconds(.5f);
        var currentNode = temporary_time_bonus.First;
        while (currentNode != null)
        {
            var currentValue = currentNode.Value;
            var timeLeft = currentValue.Item2.timeLeft - 0.5f;
            var newBonus = new Food.Bonus(currentValue.Item2.bonusValue, timeLeft);
            currentNode.Value = (currentValue.Item1, newBonus);
            Debug.Log($"加成类型{currentNode.Value.Item1}剩余时间: {timeLeft}");
            if (timeLeft <= 0)
            {
                temporary_time_bonus.Remove(currentNode);
                switch (currentNode.Value.Item1)
                {
                    case Food.FoodBonusType.Speed:
                        bonus_speed -= currentNode.Value.Item2.bonusValue;
                        speed = Mathf.Clamp(bonus_speed, minSpeed, maxSpeed);
                        break;
                    default:
                        Debug.LogError($"加成类型{currentNode.Value.Item1}在角色处不支持");
                        break;
                }
            }
            currentNode = currentNode.Next;
        }
        StartCoroutine(ScanTemporyBonusList());
    }

    private void DecreaseTemporaryBonus()
    {
        // 清除临时回合加成
        float decrease_speed = temporary_speed_bonus.DecreaseRounds();
        temporary_speed_bonus_sum -= decrease_speed;
        bonus_speed -= decrease_speed;

        // 清除本回合限时加成
        var currentNode = temporary_time_bonus.First;
        while (currentNode != null)
        {
            switch (currentNode.Value.Item1)
            {
                case Food.FoodBonusType.Speed:
                    bonus_speed -= currentNode.Value.Item2.bonusValue;
                    break;
                default:
                    Debug.LogError($"加成类型{currentNode.Value.Item1}在角色处不支持");
                    break;
            }
            currentNode = currentNode.Next;
        }
        temporary_time_bonus.Clear();

        // 计算实际速度
        speed = Mathf.Clamp(bonus_speed, minSpeed, maxSpeed);
    }
    #endregion

    public void StorePlayerData()
    {
        PlayerData playerData = new()
        {
            init_speed = init_speed,
            speed = speed,
            bonus_speed = bonus_speed,
            maxSpeed = maxSpeed,
            minSpeed = minSpeed,
            invincible_time = invincible_time,
            playerBonusData = new PlayerData.PlayerBonusData()
            {
                permanent_speed_bonus = permanent_speed_bonus,
                temporary_speed_bonus = temporary_speed_bonus,
                temporary_speed_bonus_sum = temporary_speed_bonus_sum
            },
            triggerGuids = new List<string>()
        };

        foreach (var item in triggerItems)
        {
            Guid triggerGuid = item.sourceTriggerInventoryItemGuid;
            // TODO: 存储杂项物品
            playerData.triggerGuids.Add(triggerGuid.ToString());
        }

        // 将数据转换为JSON
        string jsonData = JsonUtility.ToJson(playerData, true);
        // 保存到文件
        string filePath = Path.Combine(Application.persistentDataPath, playerSaveDataPath);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("Player data saved to: " + filePath);
    }


    public void LoadPlayerData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, playerSaveDataPath);
        
        if (File.Exists(filePath))
        {
            string jsonData = System.IO.File.ReadAllText(filePath);
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(jsonData);

            // 应用加载的数据
            init_speed = loadedData.init_speed;
            speed = loadedData.speed;
            bonus_speed = loadedData.bonus_speed;
            maxSpeed = loadedData.maxSpeed;
            minSpeed = loadedData.minSpeed;
            invincible_time = loadedData.invincible_time;
            
            // 加载角色加成数据
            permanent_speed_bonus = loadedData.playerBonusData.permanent_speed_bonus;
            temporary_speed_bonus = loadedData.playerBonusData.temporary_speed_bonus;
            temporary_speed_bonus_sum = loadedData.playerBonusData.temporary_speed_bonus_sum;
            Debug.Log("Player data loaded from: " + filePath);
        }
        else
        {
            Debug.LogWarning("No player data file found at: " + filePath);
        }
    }
}
