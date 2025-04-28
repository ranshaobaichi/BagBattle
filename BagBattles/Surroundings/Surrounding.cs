using System;
using UnityEngine;

public abstract class Surrounding : MonoBehaviour
{
    [Serializable]
    public enum SingleSurroundingType
    {
        None,
        FireBall,
        ElectricityBall,
    }

    [Serializable]
    [Tooltip("环绕物基础属性")]
    public struct SurroundingBasicAttribute
    {
        [Tooltip("伤害")] public float damage;
        [Tooltip("每秒旋转角度")] public float rotateSpeed;
        // [Tooltip("持续时间")] public float duration;
        [Tooltip("与人物间距离")] public float range;
        [Tooltip("击退效果")] public float knockBackForce;
    }

    [Header("环绕物基础属性")]
    public SurroundingBasicAttribute surroundingBasicAttribute;
    protected new Rigidbody2D rigidbody;
    protected float initialAngle; // 存储初始随机角度
    protected float currentAngle; // 当前角度

    protected float initSpeed;
    protected float speedUpTimer;
    public void DestroySurrounding() => ObjectPool.Instance.PushObject(gameObject); // 归还对象池

    protected void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        initialAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentAngle = initialAngle; // 初始化当前角度为初始角度

        // 立即设置初始位置
        if (PlayerController.Instance != null)
        {
            Vector2 playerPosition = PlayerController.Instance.transform.position;
            float x = Mathf.Cos(initialAngle) * surroundingBasicAttribute.range;
            float y = Mathf.Sin(initialAngle) * surroundingBasicAttribute.range;
            transform.position = new Vector3(playerPosition.x + x, playerPosition.y + y, transform.position.z);
        }

        // 转换为每秒旋转的角度
        surroundingBasicAttribute.rotateSpeed *= Mathf.Deg2Rad;
        initSpeed = surroundingBasicAttribute.rotateSpeed;
        speedUpTimer = 0f;
    }

    protected virtual void Update()
    {
        if (TimeController.Instance.TimeUp() || PlayerController.Instance.Live() == false)
        {
            rigidbody.velocity = Vector2.zero;
            StopAllCoroutines();
        }

        if (speedUpTimer > 0f)
        {
            speedUpTimer -= Time.deltaTime;
            if (speedUpTimer < 0f)
            {
                speedUpTimer = 0f;
                surroundingBasicAttribute.rotateSpeed = initSpeed;
            }
        }

        Rotate();
    }

    public void SpeedUp(float scale, float time)
    {
        if (speedUpTimer <= 0f)
        {
            surroundingBasicAttribute.rotateSpeed = initSpeed * (1 + scale);
        }
        speedUpTimer = Mathf.Max(speedUpTimer, time);
    }

    protected virtual void Rotate()
    {
        if (PlayerController.Instance == null) return;

        // 每帧递增角度而不是重新计算
        currentAngle += Time.deltaTime * surroundingBasicAttribute.rotateSpeed;

        Vector2 offset = new Vector2(
            Mathf.Cos(currentAngle) * surroundingBasicAttribute.range,
            Mathf.Sin(currentAngle) * surroundingBasicAttribute.range
        );

        transform.position = (Vector2)PlayerController.Instance.transform.position + offset;
    }
    protected virtual void ApplyKnockBack(EnemyController target)
    {
        if (target != null)
        {
            target.OnKnockback(surroundingBasicAttribute.knockBackForce, transform.position);
        }
    }

    public virtual void ApplyDamage(EnemyController target)  // 应用伤害
    {
        target.TakeDamage(surroundingBasicAttribute.damage);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null && enemy.Live())
            {
                ApplyDamage(enemy);
                ApplyKnockBack(enemy);
            }
        }
    }
}