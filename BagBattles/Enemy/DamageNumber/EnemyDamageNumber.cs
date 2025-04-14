using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyDamageNumber : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float upSpeed; // 上升速度
    public float destroyTime; // 销毁时间
    
    // 额外的视觉效果参数
    public float startScale = 0.5f;
    public float targetScale = 1.0f;

    private void Awake()
    {
        // 确保初始时文本为空，防止显示默认的"T"
        if (textMeshPro != null)
            textMeshPro.text = "";
    }

    public void Initialize(float damage, Vector3 position)
    {
        // 设置文本内容，并确保转换为字符串
        textMeshPro.text = damage.ToString("0"); // 使用格式化字符串移除小数部分
        
        // 设置初始位置
        transform.position = position;
        
        // 设置初始缩放
        transform.localScale = new Vector3(startScale, startScale, startScale);
        
        // 启动移动和销毁协程
        StartCoroutine(MoveAndDestroy());
    }

    private IEnumerator MoveAndDestroy()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        
        // 记录初始颜色
        Color originalColor = textMeshPro.color;
        
        while (elapsedTime < destroyTime)
        {
            // 计算当前时间进度比例
            float progress = elapsedTime / destroyTime;
            
            // 更新位置 - 匀速上升效果
            transform.position = startPosition + new Vector3(0, upSpeed * elapsedTime, 0);
            
            // 更新缩放 - 从小到大的效果（仅在前半程）
            if (progress < 0.3f)
            {
                float scaleProgress = progress / 0.3f;
                float currentScale = Mathf.Lerp(startScale, targetScale, scaleProgress);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            
            // 更新透明度 - 从初始值逐渐降低到0（后半程）
            if (progress > 0.5f)
            {
                float alphaProgress = (progress - 0.5f) / 0.5f;
                Color newColor = textMeshPro.color;
                newColor.a = Mathf.Lerp(originalColor.a, 0f, alphaProgress);
                textMeshPro.color = newColor;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 重置颜色以便下次使用
        Color resetColor = textMeshPro.color;
        resetColor.a = originalColor.a;
        textMeshPro.color = resetColor;
        
        // 将对象放回对象池
        ObjectPool.Instance.PushObject(gameObject);
    }
}
