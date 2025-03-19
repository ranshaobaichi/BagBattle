using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    #region 属性
    [Tooltip("同种敌人生成时移速差距")] public float speed_gap;
    protected PlayerController player;
    protected float[] ice_effect;


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
    [Tooltip("火属性伤害数值")] protected float fire_effect;
    [Tooltip("火属性伤害持续时间")] protected float fire_time;


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

    #endregion

    #region 基础控制
    //对外接口
    public float GetHP() { return currentHP; }
    public bool CanAttack() { return attack_flag; }
    public bool Live() { return live; }

    public void Initialize()
    {
        live = true;
        attack_flag = true;
        invincible_flag = false;
        knockback_flag = false;
        ice_flag = false;

        attack_timer = 0f;
        currentHP = maxHP;
        current_speed += UnityEngine.Random.Range(-speed_gap, speed_gap);
        ice_timer = 0.0f;
        fire_timer = 0.0f;
        current_speed = speed;
        ice_level = 0;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Initialize();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        ice_effect = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (PlayerController.Instance.Live() == false || live == false)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        find_way();
        Update_status();
    }

    protected virtual void find_way()
    {
        if (live == true && knockback_flag == false)
        {
            Vector2 dir = new(player.transform.position.x - rb.position.x, player.transform.position.y - rb.position.y);
            dir = dir.normalized;
            rb.velocity = dir * current_speed;
        }
    }

    public virtual bool TakeDamage(float damage)
    {
        if (live == false || isActiveAndEnabled == false || currentHP <= 0f)
            return false;
        if (invincible_flag == false && live)
        {
            // if (invincible_time != 0) invincible_flag = true;
            Debug.Log("enemy take damage: " + damage);
            currentHP -= damage;
            if (currentHP <= 0f)
            {
                live = false;
                rb.velocity = Vector2.zero;
                ObjectPool.Instance.PushObject(gameObject);
            }
            return true;
        }
        return false;
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

        //invincible timer
        // if (invincible_flag == true)
        // {
        //     invincible_timer += deltaTime;
        //     if (invincible_timer > invincible_time)
        //     {
        //         invincible_flag = false;
        //         invincible_timer = 0f;
        //     }
        // }

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
            }
            else
            {
                TakeDamage(fire_effect);
            }
        }
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
    public void SetFire()
    {
        fire_flag = true;
        fire_timer = 0.0f;
    }
    public bool OnFire() { return fire_flag; }
    //冰
    public void SetIce()
    {
        ice_flag = true; ice_timer = 0.0f;
        if (ice_level < 9)
            ice_level++;
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

    private IEnumerator HandleKnockback()
    {
        knockback_flag = true;
        // 在震退期间，可以选择禁用其它移动逻辑
        yield return new WaitForSeconds(.2f);
        knockback_flag = false;
    }

    #endregion
}
