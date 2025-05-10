using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    #region 属性
    [Tooltip("同种敌人生成时移速差距")] public float speed_gap;
    protected PlayerController player;
    protected float[] ice_effect;
    protected float[] fire_effect;
    protected Enemy.EnemyType enemy_type;

    [Header("基础属性")]
    public float speed;
    protected float current_speed;
    public float maxHP;
    protected float currentHP;
    [Tooltip("受击无敌时间")] public float invincible_time;
    [Tooltip("接触伤害")] public float attack_damage;
    [Tooltip("接触伤害攻速")] public float attack_speed;
    [Tooltip("冰属性减速层数")] protected int ice_level;
    [Tooltip("冰属性减速持续时间")] public float ice_time;
    [Tooltip("火属性伤害层数")] protected int fire_level;
    [Tooltip("火属性伤害持续时间")] protected float fire_time;

    [Header("受击效果")]
    protected Color originalColor;
    protected Color flashColor;
    [Tooltip("受击闪烁次数")] public int hurtFlashCount = 1;
    [Tooltip("受击闪烁速度")] public float hurtFlashSpeed = 0.1f;
    [Tooltip("受击闪烁透明度")] [Range(0, 1)] public float hurtFlashAlpha = 0.3f;
    protected SpriteRenderer spriteRenderer;
    protected bool isFlashing;

    [Header("属性标志位")]
    protected bool live = true;
    protected bool invincible_flag = false;
    protected bool attack_flag;
    protected bool fire_flag;
    protected bool ice_flag;
    protected bool knockback_flag;

    [Header("计时器")]
    protected float attack_timer;
    protected float invincible_timer;
    protected float fire_timer;
    protected float ice_timer;

    [Header("组件")]
    public Text health_text;
    public Rigidbody2D rb;

    [Header("敌人图像")]
    [Tooltip("序列帧切换时间")] public float enemySpriteChangeTime = 0.1f;
    [Tooltip("敌人不同序列帧")] public List<Sprite> enemySprites = new List<Sprite>();
    protected int currentSpriteIdx;
    #endregion

    #region 基础控制
    //对外接口
    public float GetHP() { return currentHP; }
    public bool CanAttack() { return attack_flag; }
    public bool Live() { return live; }

    public virtual void Initialize()
    {
        live = true;
        attack_flag = true;
        invincible_flag = false;
        knockback_flag = false;
        ice_flag = false;

        attack_timer = 0f;
        ice_timer = 0.0f;
        fire_timer = 0.0f;
        invincible_timer = 0f;
        invincible_time = 0.1f;
        ice_level = 0;
        currentHP = maxHP;
        current_speed += UnityEngine.Random.Range(-speed_gap, speed_gap);
        current_speed = speed;

        spriteRenderer.color = originalColor;
        isFlashing = false;

        currentSpriteIdx = 0;
        InvokeRepeating(nameof(UpdateSprite), enemySpriteChangeTime, enemySpriteChangeTime);
    }

    public void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, hurtFlashAlpha);
        }
    }

    protected virtual void Start()
    {
        if (TimeController.Instance.TimeUp() == true)
        {
            live = false;
            rb.velocity = Vector2.zero;
            ObjectPool.Instance.PushObject(gameObject);
            return;
        }
        ice_effect = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        fire_effect = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        Initialize();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (TimeController.Instance.TimeUp() == true || PlayerController.Instance.Live() == false)
        {
            live = false;
            rb.velocity = Vector2.zero;
            ObjectPool.Instance.PushObject(gameObject);
            return;
        }
        find_way();
        Update_status();
    }

    protected virtual void OnDisable()
    {
        CancelInvoke(nameof(UpdateSprite));
        rb.velocity = Vector2.zero;
        live = false;
        attack_flag = false;
        invincible_flag = false;
    }

    protected virtual void find_way()
    {
        if (live == true && knockback_flag == false)
        {
            Vector2 dir = new(player.transform.position.x - rb.position.x, player.transform.position.y - rb.position.y);
            if (dir.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (dir.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            dir = dir.normalized;
            rb.velocity = dir * current_speed;
        }
    }

    // 敌人死亡时调用接口
    public virtual void OnDead()
    {
        StatisticsScript.Instance.AddTotalEnemiesKilled();
    }
    
    // 敌人被击中时调用接口
    public virtual bool TakeDamage(float damage)
    {
        if (live == false || isActiveAndEnabled == false || currentHP <= 0f)
            return false;
        if (invincible_flag == false && live)
        {
            Debug.Log("enemy take damage: " + damage);

            // 计算实际伤害
            float actual_damage = CalculateActualDamage(damage);
            currentHP -= actual_damage;
            StatisticsScript.Instance.AddTotalDamageCaused(actual_damage);

            // 设置无敌状态
            if (invincible_time > 0)
            {
                invincible_flag = true;
                invincible_timer = 0f;
            }

            // 生成伤害数字
            EnemyDamageNumberController.Instance.CreateDamageNumber(damage, transform.position + new Vector3(0, 1.5f, 0));

            // 死亡处理
            if (currentHP <= 0f)
            {
                OnDead();
                live = false;
                rb.velocity = Vector2.zero;
                ObjectPool.Instance.PushObject(gameObject);
            }
            
            // 启动闪烁效果
            if (!isFlashing && live)
            {
                StartCoroutine(FlashEffect());
            }
            return true;
        }
        return false;
    }

    protected virtual float CalculateActualDamage(float damage)
    {
        return Mathf.Clamp(damage, 0, currentHP);
    }

    protected IEnumerator FlashEffect()
    {
        isFlashing = true;
        for (int i = 0; i < hurtFlashCount; i++)
        {
            // 闪烁到透明状态
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(hurtFlashSpeed);

            // 恢复原始颜色
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(hurtFlashSpeed);
        }

        // 确保最后恢复原始颜色
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    protected void Update_status()
    {
        if (!live) return;

        float deltaTime = Time.deltaTime;
        //attack timer
        if (attack_flag == false)
        {
            attack_timer += deltaTime;
            if (attack_timer > attack_speed)
            {
                attack_flag = true;
                attack_timer = 0f;
            }
        }

        // invincible timer
        if (invincible_flag == true)
        {
            invincible_timer += deltaTime;
            if (invincible_timer > invincible_time)
            {
                invincible_flag = false;
                invincible_timer = 0f;
            }
        }

        //ice
        if (ice_flag == true)
        {
            ice_timer += deltaTime;
            current_speed = speed * ice_effect[ice_level];
            if (ice_timer > ice_time)
            {
                ice_flag = false;
                ice_timer = 0.0f;
                current_speed = speed;
                ice_level = 0;
            }
        }

        //fire
        if (fire_flag == true)
        {
            fire_timer += deltaTime;
            if (fire_timer > fire_time)
            {
                fire_flag = false;
                fire_timer = 0.0f;
                StopCoroutine(TakeFireDamage());
            }
        }
    }

    protected virtual void UpdateSprite()
    {
        if (enemySprites.Count == 0) return;
        currentSpriteIdx = (currentSpriteIdx + 1) % enemySprites.Count;
        spriteRenderer.sprite = enemySprites[currentSpriteIdx];
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (PlayerController.Instance.Live() == false)
            return;
        if (attack_flag == false)
            return;

        if (collision.CompareTag("Player"))
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(attack_damage);
    }

    #endregion

    #region 道具效果
    //火
    public void SetFire(int level = 1)
    {
        if (level == 0)
        {
            fire_timer = 0.0f;
            return;
        }
        StopCoroutine(TakeFireDamage());
        fire_flag = true;
        fire_timer = 0.0f;
        fire_level = Math.Min(fire_level + level, 9);
        StartCoroutine(TakeFireDamage());
    }
    public bool OnFire() { return fire_flag; }
    protected IEnumerator TakeFireDamage()
    {
        TakeDamage(fire_effect[fire_level]);
        yield return new WaitForSeconds(1f);
    }
    //冰
    public void SetIce(int level = 1)
    {
        if (level == 0)
        {
            ice_timer = 0.0f;
            return;
        }
        ice_flag = true;
        ice_timer = 0.0f;
        ice_level = Math.Min(ice_level + level, 9);
    }
    public bool OnIce() { return ice_flag; }
    
    //击退
    // 在敌人被震退时调用该方法
    public void OnKnockback(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
            StartCoroutine(HandleKnockback());
        }
    }
    public void OnKnockback(float force, Vector2 position)
    {
        if (rb != null)
        {
            Vector2 direction = (rb.position - position).normalized;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            StartCoroutine(HandleKnockback());
        }
    }

    private IEnumerator HandleKnockback()
    {
        knockback_flag = true;
        // 在震退期间，可以选择禁用其它移动逻辑
        yield return new WaitForSeconds(.2f);
        knockback_flag = false;
    }

    #endregion
}
