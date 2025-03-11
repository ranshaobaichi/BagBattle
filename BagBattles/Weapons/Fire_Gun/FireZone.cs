using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireZone : MonoBehaviour
{
    private float duration;
    private float tickInterval;
    private float damage;
    private HashSet<EnemyController> enemiesInZone = new HashSet<EnemyController>();

    public GameObject fireEffectPrefab; //火焰粒子预制体
    private GameObject fireEffectInstance; //火焰粒子实例
    private LineRenderer lineRenderer; // 🔥 画圆用的 LineRenderer
    private int circleSegments = 50; // 圆圈的平滑度
    private bool showRange;
    private float radius;

    public void Initialize(float radius, float duration, float tickInterval, float damage, bool showRange)
    {
        this.duration = duration;
        this.tickInterval = tickInterval;
        this.damage = damage;
        this.showRange = showRange;
        this.radius = radius;

        // 生成火焰粒子效果
        fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
        fireEffectInstance.transform.SetParent(transform); // 让粒子跟随火焰区域

        DrawCircle();
        DetectEnemiesInZone();
        StartCoroutine(DamageOverTime());
    }

    private void DetectEnemiesInZone()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null && !enemiesInZone.Contains(enemy))
                {
                    enemiesInZone.Add(enemy); // 🔥 手动锁定范围内的敌人
                }
            }
        }
    }


    private void DrawCircle()
    {
        if (!showRange) return;
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = circleSegments + 1; // 多一个点来闭合圆圈
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = false; // true 可能导致坐标错误
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 使用默认材质
        lineRenderer.startColor = Color.red; // 圆圈颜色
        lineRenderer.endColor = Color.red;

        float angleStep = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    private IEnumerator DamageOverTime()
    {
        float timer = 0f;
        while (timer < duration)
        {
            // 🔥 每次伤害时，重新检测范围内的敌人，减少计算量
            RefreshEnemiesInZone();

            foreach (var enemy in enemiesInZone)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }

            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }

        Destroy(fireEffectInstance);
        ObjectPool.Instance.PushObject(gameObject);
    }

    private void RefreshEnemiesInZone()
    {
        enemiesInZone.Clear(); // 🔥 直接清空列表，重新获取敌人

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemiesInZone.Add(enemy); // 只保留仍在范围内的敌人
                }
            }
        }
    }


    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Enemy"))
    //     {
    //         EnemyController enemy = other.GetComponent<EnemyController>();
    //         if (enemy != null)
    //             enemiesInZone.Add(enemy);
    //     }
    // }

    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag("Enemy"))
    //     {
    //         EnemyController enemy = other.GetComponent<EnemyController>();
    //         if (enemy != null)
    //             enemiesInZone.Remove(enemy);
    //     }
    // }
}
