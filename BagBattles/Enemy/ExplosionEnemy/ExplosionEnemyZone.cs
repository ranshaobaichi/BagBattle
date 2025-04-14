using System.Collections;
using UnityEngine;

public class ExplosionEnemyZone : MonoBehaviour
{
    private CircleCollider2D range;
    private float radius;
    private float damage;
    private float bombTime;
    
    [SerializeField] private Color gizmoColorActive = new Color(1f, 0.3f, 0.3f, 0.4f); // 半透明红色
    [SerializeField] private Color gizmoColorWire = new Color(1f, 0.1f, 0.1f, 1f);     // 实线红色

    void Awake()
    {
        range = GetComponent<CircleCollider2D>();
    }
    
    public void Initialize(float r, float d, float bomb_t)
    {
        radius = r;
        damage = d;
        range.radius = radius;
        bombTime = bomb_t;
        StartCoroutine(Bomb());
    }

    public IEnumerator Bomb()
    {
        yield return new WaitForSeconds(bombTime);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerController player = collider.GetComponent<PlayerController>();
                if (player != null && player.Live())
                {
                    player.TakeDamage(damage);
                }
            }
        }
        ObjectPool.Instance.PushObject(gameObject);
    }
    
    // 在Scene视图中绘制范围圆形，不管物体是否被选中都会显示
    private void OnDrawGizmos()
    {
        // 确保我们有有效的半径，如果物体还未初始化，则使用碰撞器半径
        float displayRadius = radius > 0 ? radius : 
                           (range != null ? range.radius : 1f);
        
        // 绘制半透明填充圆
        Gizmos.color = gizmoColorActive;
        Gizmos.DrawSphere(transform.position, displayRadius);
        
        // 绘制实线圆边框
        Gizmos.color = gizmoColorWire;
        Gizmos.DrawWireSphere(transform.position, displayRadius);
    }
}