using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectricityBallSurrounding : Surrounding
{
    [Header("连锁属性")]
    [Tooltip("连锁伤害")] public float chainDamage = 10f;
    [Tooltip("连锁范围")] public float chainRadius = 3f;
    [Tooltip("连锁次数")] public int maxChainCount = 3;
    [Tooltip("闪电线持续时间")] private float lineDuration = 0.5f;
    
    [Header("粒子效果")]
    [Tooltip("闪电粒子预制体")] public GameObject lightningParticlePrefab;

    [Header("闪电特效设置")]
    [Tooltip("闪电材质")] public Material lightningMaterial;
    [Tooltip("闪电宽度")] public float lightningWidth = 0.05f;
    [Tooltip("闪电强度")] [Range(1, 10)] public float lightningIntensity = 2f;
    [Tooltip("扭曲速度")] [Range(0, 5)] public float distortionSpeed = 1f;


    private float lightningDamageInterval = 1f; // 闪电伤害间隔
    private float lightningDamageTimer = 0f; // 闪电伤害计时器
    private bool lightningDamageFlag = true;
    private GameObject lightningParent;
    private HashSet<EnemyController> affectedEnemies = new HashSet<EnemyController>();

    private void Start()
    {
        lightningParent = new GameObject("SurroundingLightningLines");
    }

    protected override void Update()
    {
        base.Update();
        // 处理闪电伤害计时器
        if (lightningDamageTimer > 0f)
        {
            lightningDamageTimer -= Time.deltaTime;
            if (lightningDamageTimer <= 0f)
            {
                lightningDamageTimer = lightningDamageInterval;
                lightningDamageFlag = true;
            }
        }
    }

    public override void ApplyDamage(EnemyController target)
    {
        base.ApplyDamage(target);
        // 重置受影响敌人列表
        affectedEnemies.Clear();
        if (lightningDamageFlag)
        {
            DealChainDamage(target, maxChainCount);
            lightningDamageFlag = false;
            lightningDamageTimer = lightningDamageInterval;
        }
    }

    private void DealChainDamage(EnemyController enemy, int remainingChains)
    {
        if (enemy == null || remainingChains <= 0)
            return;

        enemy.TakeDamage(chainDamage); // 对当前敌人造成伤害
        // 将当前敌人添加到已处理列表
        affectedEnemies.Add(enemy);
        // 查找附近的敌人并继续连锁伤害
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(enemy.transform.position, chainRadius);
        foreach (var collider in enemiesInRange)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController nearbyEnemy = collider.GetComponent<EnemyController>();
                if (nearbyEnemy != null && !affectedEnemies.Contains(nearbyEnemy))
                {
                    // 在当前敌人与附近敌人之间绘制闪电线
                    DrawLightningLine(enemy.transform.position, nearbyEnemy.transform.position);
                    // 递归连锁伤害
                    DealChainDamage(nearbyEnemy, remainingChains - 1);
                    break;
                }
            }
        }
    }

    // 绘制闪电线
    private void DrawLightningLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObject = new GameObject("LightningLine");
        lineObject.transform.parent = lightningParent.transform;

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // 应用闪电材质
        if (lightningMaterial != null)
        {
            // 为每条闪电线创建材质实例，以便单独控制
            Material instanceMaterial = new Material(lightningMaterial);
            lineRenderer.material = instanceMaterial;
            
            // 设置材质参数
            instanceMaterial.SetFloat("_Intensity", lightningIntensity);
            instanceMaterial.SetFloat("_NoiseSpeed", distortionSpeed);
        }
        
        // 设置LineRenderer属性
        lineRenderer.startWidth = lightningWidth;
        lineRenderer.endWidth = lightningWidth;
        lineRenderer.useWorldSpace = true;
        
        // 添加锯齿状闪电效果
        CreateJaggedLightning(lineRenderer, startPos, endPos);

        // 添加闪烁效果
        StartCoroutine(AnimateLightning(lineRenderer));
        
        // 在闪电路径上生成粒子
        if (lightningParticlePrefab != null)
        {
            SpawnParticlesAlongLightning(lineRenderer);
        }
    }
    
    // 在闪电路径上生成粒子
    private void SpawnParticlesAlongLightning(LineRenderer lineRenderer)
    {
        int particleCount = Mathf.CeilToInt(Vector2.Distance(
            lineRenderer.GetPosition(0), 
            lineRenderer.GetPosition(lineRenderer.positionCount - 1)) * 2); // 每单位2个粒子
            
        for (int i = 0; i < particleCount; i++)
        {
            // 在闪电路径上随机选择一点
            int segmentIndex = Random.Range(0, lineRenderer.positionCount - 1);
            float t = Random.value; // 0到1之间的随机值
            
            Vector3 pos = Vector3.Lerp(
                lineRenderer.GetPosition(segmentIndex),
                lineRenderer.GetPosition(segmentIndex + 1),
                t
            );
            
            // 生成粒子
            GameObject particle = Instantiate(lightningParticlePrefab, pos, Quaternion.identity);
            particle.transform.parent = lineRenderer.transform;
            
            // 调整粒子系统
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                // 随机持续时间，但不超过闪电持续时间
                main.startLifetime = Random.Range(0.1f, lineDuration * 0.8f);
                
                // 播放粒子
                ps.Play();
            }
            
            // 销毁粒子实例
            Destroy(particle, lineDuration);
        }
    }
    
    // 动画效果
    private IEnumerator AnimateLightning(LineRenderer lineRenderer)
    {
        float elapsedTime = 0;
        Material mat = lineRenderer.material;
        float initialIntensity = mat.GetFloat("_Intensity");
        
        // 保存原始位置
        Vector3[] originalPositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(originalPositions);
        
        while (elapsedTime < lineDuration)
        {
            // 闪烁强度变化
            float intensityMultiplier = Mathf.PingPong(Time.time * 5, 1.0f) + 0.5f;
            mat.SetFloat("_Intensity", initialIntensity * intensityMultiplier);
            
            // 轻微扭曲闪电路径
            if (elapsedTime > lineDuration * 0.2f) // 开始后稍微延迟再扭曲
            {
                for (int i = 1; i < lineRenderer.positionCount - 1; i++) // 保持首尾不变
                {
                    // 原始位置加上轻微的随机偏移
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-0.05f, 0.05f),
                        Random.Range(-0.05f, 0.05f),
                        0
                    );
                    lineRenderer.SetPosition(i, originalPositions[i] + randomOffset);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 结束时的消失动画
        float fadeTime = 0.1f;
        float fadeElapsed = 0;
        
        while (fadeElapsed < fadeTime)
        {
            // 逐渐降低宽度和强度
            float t = fadeElapsed / fadeTime;
            lineRenderer.startWidth = lightningWidth * (1 - t);
            lineRenderer.endWidth = lightningWidth * (1 - t);
            mat.SetFloat("_Intensity", initialIntensity * (1 - t));
            
            fadeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // 销毁对象
        Destroy(lineRenderer.gameObject);
    }

    // 创建锯齿状闪电效果
    private void CreateJaggedLightning(LineRenderer lineRenderer, Vector2 start, Vector2 end)
    {
        // 闪电段数，越多越锯齿
        int segments = 12;
        lineRenderer.positionCount = segments;
        
        // 设置锯齿偏移最大值
        float maxOffset = 0.3f;
        
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector2 straightPoint = Vector2.Lerp(start, end, t);
            
            // 为中间点添加随机偏移（首尾点不偏移）
            if (i != 0 && i != segments - 1)
            {
                // 垂直于直线方向的偏移
                Vector2 perpendicular = new Vector2(-(end.y - start.y), end.x - start.x).normalized;
                float offset = Random.Range(-maxOffset, maxOffset);
                straightPoint += perpendicular * offset;
            }
            
            lineRenderer.SetPosition(i, straightPoint);
        }
    }

    private void OnDestroy()
    {
        if (lightningParent != null)
            Destroy(lightningParent);
    }
}