using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning_Bullet : Bullet
{
    [Tooltip("连锁范围")] private float chainRadius;
    [Tooltip("连锁次数")] private int maxChainCount; // 最大连锁次数
    [Tooltip("闪电线持续时间")] private float lineDuration; // 闪电线的持续时间
    public GameObject lightningParent;
    private bool isDestoried;

    HashSet<EnemyController> enemies = new HashSet<EnemyController>();
    public void SetBullet(float ch, int ma, float li)
    {
        chainRadius = ch;
        maxChainCount = ma;
        lineDuration = li;
    }

    private void Start()
    {
        isDestoried = false;
        lightningParent = new GameObject("LightningLines"); // 创建一个父物体
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
            Destroy(gameObject);
        if (isDestoried == true)
            return;
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyController firstEnemy = other.gameObject.GetComponent<EnemyController>();
            DealDamage(firstEnemy, maxChainCount);
            current_pass_num--;
            if(current_pass_num < 0)
            {
                isDestoried = true;
                GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    private void DealDamage(EnemyController enemy, int remainingChains)
    {
        enemies.Add(enemy);
        if (enemy == null || remainingChains <= 0)
        {
            if(current_pass_num < 0)
                Destroy(gameObject);
            return;
        }

        // 对当前敌人造成伤害
        CauseDamage(enemy);

        // 查找附近的敌人并继续连锁伤害
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(enemy.transform.position, chainRadius);
        foreach (var collider in enemiesInRange)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController nearbyEnemy = collider.GetComponent<EnemyController>();
                if (nearbyEnemy != null && nearbyEnemy != enemy && !enemies.Contains(nearbyEnemy)) // 避免连锁到自己
                {
                    enemies.Add(nearbyEnemy);
                    // 在当前敌人与附近敌人之间绘制闪电线
                    DrawLightningLine(enemy.transform.position, nearbyEnemy.transform.position);

                    // 递归连锁伤害
                    DealDamage(nearbyEnemy, remainingChains - 1);
                    break;
                }
            }
        }
    }

    // 绘制闪电线
    private void DrawLightningLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObject = new GameObject("LightningLine");
        lineObject.transform.parent = lightningParent.transform; // 将闪电线添加到父物体下

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        // 设置LineRenderer的属性
        lineRenderer.startWidth = 0.05f; // 设置线宽
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.yellow; // 设置颜色
        lineRenderer.endColor = Color.yellow;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 设置材质
        lineRenderer.positionCount = 2; // 线段数量
        lineRenderer.SetPosition(0, startPos); // 设置起点
        lineRenderer.SetPosition(1, endPos); // 设置终点

        // 添加闪烁效果
        StartCoroutine(FlashLine(lineRenderer));
    }

    // 闪电线闪烁效果
    private IEnumerator FlashLine(LineRenderer lineRenderer)
    {
        float elapsedTime = 0f;
        while (elapsedTime < lineDuration)
        {
            // 闪烁效果
            float alpha = Mathf.PingPong(Time.time * 5f, 1f);
            Color color = new Color(1f, 1f, 0f, alpha); // 黄色并闪烁
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 等待闪烁完成后销毁
        Destroy(lineRenderer.gameObject);
    }

    // 在需要销毁时销毁父物体（所有闪电线将被销毁）
    private void DestroyAllLightningLines()
    {
        if (lightningParent != null)
            Destroy(lightningParent); //销毁父物体后所有子物体（闪电线）都会被销毁
    }

    private void OnDestroy()
    {
        DestroyAllLightningLines();
    }
}
