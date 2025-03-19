using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.EditorTools;
using UnityEngine;

public class Cigarette_Bullet : Bullet
{
    public GameObject firePrefab; // 火焰区域的预制体
    [Tooltip("子弹旋转速度（度/秒）")] public float rotationSpeed;
    [Header("火焰区域参数")]
    [Tooltip("火焰影响半径")] public float explosionRadius;
    [Tooltip("火焰持续时间")] public float fireDuration;
    [Tooltip("每次伤害的间隔时间")] public float fireTickInterval;
    [Tooltip("火焰的伤害值")] public float fireDamage;
    [Tooltip("是否显示火焰范围")] public bool showRange;

    private Vector2 movePos;

    // public void SetBullet(float ro, float ex, float du, float ti, float f_da, bool sh)
    // {
    //     rotationSpeed = ro;
    //     explosionRadius = ex;
    //     fireDuration = du;
    //     fireTickInterval = ti;
    //     fireDamage = f_da;
    //     showRange = sh;
    // }
    protected override void Update()
    {
        base.Update();
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        rigidbody.velocity = movePos * bulletBasicAttribute.speed;
    }

    public override void SetSpeed(Vector2 direction)
    {
        rigidbody.velocity = direction * bulletBasicAttribute.speed;
        movePos = direction;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            Explode();
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
        if(other.CompareTag("Wall"))
            Del();
    }

    private void Explode()
    {
        GameObject fire = ObjectPool.Instance.GetObject(firePrefab);
        fire.SetActive(true);
        fire.transform.position = transform.position;
        fire.GetComponent<FireZone>().Initialize(explosionRadius, fireDuration, fireTickInterval, fireDamage, showRange);
    }
    
}