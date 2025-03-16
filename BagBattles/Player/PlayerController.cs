using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
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
    private HashSet<TriggerItem> triggerItems = new(); // 角色拥有的触发器
    private HashSet<Item> items = new(); // 角色拥有的物品
    private int face;
    #endregion

    #region 对外接口
    public void AddTriggerItem(InventoryTriggerItem item)
    {
        TriggerItem tmpTriggerItem = new();
        tmpTriggerItem.Initialize(item.triggerItemAttribute, item.triggerItems);
        triggerItems.Add(tmpTriggerItem);
    }
    public void Dead() { live = false; rb.velocity = Vector2.zero; gameObject.SetActive(false); }
    public bool Live() { return live; }
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (active == true)
        {
            live = true;
            rb.velocity = Vector2.zero;
        }
    }
    #endregion

    #region 角色控制
    private void OnEnable()
    {
        transform.position = new Vector3(0, 0, 0);
        rb.velocity = Vector2.zero;
        live = true;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;
        foreach (var item in triggerItems)
        {
            item.StartTrigger();
        }
        bulletSpawner.StartFire();
    }

    private void OnDisable()
    {
        foreach (var item in triggerItems)
        {
            item.StopTrigger();
        }
        rb.velocity = Vector2.zero;
        live = false;
        invincible_flag = false;
        invincible_timer = 0.0f;
        face = 0;
        bulletSpawner.EndFire();
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
            flip.y = rb.velocity.x < 0 ? 180 : 0;
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

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (invincible_flag == true)
            return;
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        //     if (enemy.CanAttack() == true)
        //     {
        //         //Debug.Log("damage");
        //         HeartController.TakeDamage((int)collision.gameObject.GetComponent<EnemyController>().attack_damage);
        //         invincible_flag = true;
        //     }
        // }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (invincible_flag == true)
            return;
        // if (collision.gameObject.CompareTag("Enemy_Bullet"))
        // {
        //     invincible_flag = true;
        //     HeartController.TakeDamage((int)collision.gameObject.GetComponent<Bullet>().GetDamage());
        // }
    }
    #endregion
}
