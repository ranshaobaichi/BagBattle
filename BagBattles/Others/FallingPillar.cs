using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPillar : Othering
{
    [Header("柱子设置")]
    [Tooltip("生成后多长时间倒下")]
    public float delayBeforeFall = 0.5f;
    
    [Tooltip("柱子倒下的时间(秒)")]
    public float fallingDuration = 1.5f;
    
    [Tooltip("完全倒下后存在时间")]
    public float existenceDuration = 2.0f;
    
    [Tooltip("淡出动画占总存在时间的比例 (0-1)")]
    [Range(0, 1)]
    public float fadeOutRatio = 0.7f;
    
    [Tooltip("造成的伤害值")]
    public int damage = 10;
    
    [Tooltip("倒下方向 (-1为左, 1为右)")]
    protected int fallDirection;

    [Header("动画控制")]
    [Tooltip("倒下的速度曲线：X轴为时间(0-1)，Y轴为插值进度(0-1)")]
    public AnimationCurve fallingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("淡出的速度曲线：X轴为时间(0-1)，Y轴为不透明度(1-0)")]
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private bool isAnimationComplete = false;
    private float initialRotation = 0f; // 初始为直立状态
    private HashSet<EnemyController> enemies = new();
    private float targetAngle;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    void OnEnable()
    {
        // 获取所有子物体的SpriteRenderer组件
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        GetComponent<BoxCollider2D>().enabled = true; 
        originalColors = new Color[spriteRenderers.Length];
        
        // 保存原始颜色
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
            // 确保开始时完全不透明
            Color color = spriteRenderers[i].color;
            color.a = 1f;
            spriteRenderers[i].color = color;
        }
        
        // 重置为直立状态
        transform.rotation = Quaternion.Euler(0, 0, 0);
        initialRotation = 0f;
        enemies = new();
        enemies.Clear();
        isAnimationComplete = false;
        // 根据倒下方向计算目标角度 (正90度或负90度)
        targetAngle = 90f * fallDirection;
        StartCoroutine(FallDown());
    }

    private IEnumerator FallDown()
    {
        yield return new WaitForSeconds(delayBeforeFall);
        fallDirection = PlayerController.Instance.transform.position.x < transform.position.x ? 1 : -1;
        targetAngle = 90f * fallDirection; // 更新目标角度
        float elapsedTime = 0f;

        // 逐渐倒下的效果
        while (elapsedTime < fallingDuration)
        {
            // 通过曲线计算插值因子，而不是简单的线性插值
            float t = elapsedTime / fallingDuration;
            float curveValue = fallingCurve.Evaluate(t);

            // 使用曲线值进行角度插值
            float currentAngle = Mathf.Lerp(initialRotation, targetAngle, curveValue);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
            if (elapsedTime >= 0.7f * fallingDuration) // 70%时间后停止检测伤害
            {
                isAnimationComplete = true;
            }
        }

        // 确保最终旋转到位
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

        yield return new WaitForSeconds(.75f);
        isAnimationComplete = false;
        
        // 计算淡出开始的时间点
        float solidTime = existenceDuration * (1 - fadeOutRatio);
        yield return new WaitForSeconds(solidTime);
        
        // 启动淡出效果
        StartCoroutine(FadeOut(existenceDuration * fadeOutRatio));
        
        // 等待淡出完成后回收
        yield return new WaitForSeconds(existenceDuration * fadeOutRatio);
        ObjectPool.Instance.PushObject(gameObject); // 返回对象池
    }

    private IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0f;
        
        // 逐渐淡出所有SpriteRenderer
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float alpha = fadeOutCurve.Evaluate(t);
            
            // 更新所有子物体的透明度
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Color color = originalColors[i];
                color.a = alpha;
                spriteRenderers[i].color = color;
            }
            
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 0.7f * duration)
                GetComponent<BoxCollider2D>().enabled = false; // 70%时间后停止检测伤害
            yield return null;
        }
        
        // 确保完全透明
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = originalColors[i];
            color.a = 0f;
            spriteRenderers[i].color = color;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAnimationComplete) return;
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                enemy.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        // 确定当前角度和目标角度
        float currentAngle = transform.rotation.eulerAngles.z;
        float previewFallDirection = Application.isPlaying ? fallDirection : 1;
        float previewTargetAngle = 90f * previewFallDirection;
        
        // 显示当前旋转
        Vector3 currentDirection = new Vector3(0, 1, 0);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, currentAngle) * currentDirection * 2f);
        
        // 显示目标旋转
        Vector3 targetDirection = new Vector3(0, 1, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, previewTargetAngle) * targetDirection * 2f);
    }
}