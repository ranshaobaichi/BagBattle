using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombZone : MonoBehaviour
{
    private CircleCollider2D range;
    private LineRenderer lineRenderer;
    private float radius;
    private float damage;
    private bool showRange;
    private int circleSegments = 50; // 圆圈的平滑度


    void Awake()
    {
        range = GetComponent<CircleCollider2D>();
    }
    public void Initialize(float r, float d, bool show)
    {
        showRange = show;
        radius = r;
        damage = d;
        range.radius = radius;
        DrawCircle();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    EnemyController enemy = collider.GetComponent<EnemyController>();
                    if (enemy != null && enemy.Live())
                    {
                        enemy.TakeDamage(damage);
                    }
                }
            }
            Destroy(gameObject);
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

}
