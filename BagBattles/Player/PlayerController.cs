using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance = null;
    #region 组件属性
    [Header("基础属性")]
    [Tooltip("角色移动速度")] public float speed;
    [Tooltip("受击无敌时间")] public float invincible_time;

    [Header("属性标志位")]
    private static bool live;
    protected bool invincible_flag;

    [Header("计时器")]
    protected float invincible_timer;

    [Header("组件")]
    private static Rigidbody2D rb;
    private BulletSpawner bulletSpawner;

    [Header("Trigger&Item")]
    [SerializeField] private List<TriggerItem> triggerItems = new List<TriggerItem>(); // 角色拥有的触发器
    public GameObject triggerGameObject; //承载触发器子物体
    // private List<Item> items = new(); // 角色拥有的物品
    private int face;
    #endregion

    #region 属性加成
    private float permanent_speed_bonus;
    private LinkedList<Food.Bonus> temporary_speed_bonus = new();
    #endregion

    #region 对外接口
    public void AddTriggerItem(TriggerInventoryItem item)
    {
        Trigger.TriggerType type = item.GetTriggerType();
        switch (type)
        {
            case Trigger.TriggerType.ByTime:
                TimeTriggerItem timeTriggerItem = triggerGameObject.AddComponent<TimeTriggerItem>();
                timeTriggerItem.Initialize(item.GetSpecificType(), item.triggerItems);
                triggerItems.Add(timeTriggerItem);
                break;
            case Trigger.TriggerType.ByFireTimes:
                FireTriggerItem fireTriggerItem = triggerGameObject.AddComponent<FireTriggerItem>();
                fireTriggerItem.Initialize(item.GetSpecificType(), item.triggerItems);
                triggerItems.Add(fireTriggerItem);
                break;
            default:
                Debug.LogError($"触发器类型{type}不支持");
                break;
        }
    }
    public void Dead()
    {
        live = false;
        gameObject.SetActive(false);
        StopAllCoroutines();
    }
    public void FinishRound()
    {
        live = false;
        gameObject.SetActive(false);
        StopAllCoroutines();
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

        // 启动触发器
        foreach (var item in triggerItems)
        {
            Debug.Log("player trigger item: " + item.GetType());
            item.LaunchTrigger();
        }

        // 启动枪械模组
        bulletSpawner.StartFire();

        // 角色获得临时加成
        speed += temporary_speed_bonus.Sum(x => x.bonusValue);
    }

    private void OnDisable()
    {
        // 禁用角色
        rb.velocity = Vector2.zero;
        live = false;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;

        // 结束枪械模组
        bulletSpawner.EndFire();
        bulletSpawner.ClearTemporaryBonus();

        // 结束并清除触发器
        DestroyAllTriggers();
        Component[] triggerComponents = triggerGameObject.GetComponents<TriggerItem>();
        foreach (var item in triggerComponents)
        {
            Destroy(item);
        }

        // 移除临时加成
        ClearTemporaryBonus();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        live = true;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;
        rb = GetComponent<Rigidbody2D>();
        rb.mass = 1000f;
        bulletSpawner = transform.GetComponent<BulletSpawner>();
    }

    private void Update()
    {
        if (live == false) return;
        //move
        Vector3 move = new(0f, 0f, 0f)
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };

        Quaternion flip = transform.rotation;
        if (move.x != 0)
        {
            flip.y = rb.velocity.x > 0 ? 180 : 0;
            face = (int)flip.y;
        }
        else
            flip.y = face;
        transform.rotation = flip;
        Vector2 v = (Vector2)move.normalized;
        rb.velocity = v * speed;
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
            HeartController.TakeDamage((int)damage);
        }
    }

    public void DestroyAllTriggers()
    {
        foreach (var item in triggerItems)
            item.Destroy();
        triggerItems.Clear();
    }
    #endregion

    #region 属性接口
    // 速度属性
    public void AddPermanentSpeed(float bonus)
    {
        permanent_speed_bonus += bonus;
        speed += bonus;
    }
    public void AddTemporarySpeed(float bonus, int round) => temporary_speed_bonus.AddLast(new Food.Bonus(bonus, round)); // 添加临时加成
    
    private void ClearTemporaryBonus()
    {
        // 清除临时加成
        speed -= temporary_speed_bonus.Sum(x => x.bonusValue);

        // 处理过期的临时加成
        temporary_speed_bonus.DecreaseRounds();
    }
    #endregion
}
