using System.Collections;
using UnityEngine;

public class DivisionParentEnemy : EnemyController
{
    [Header("分裂配置")]
    public GameObject divisionChildPrefab; // 分裂预制体
    public int divisionCount; // 分裂数量
    public float spawnRadius; // 分裂半径
    public float minSpawnDelay; // 最小分裂间隔
    public float maxSpawnDelay; // 最大分裂间隔
    
    private bool isDividing = false;

    protected override void Start()
    {
        base.Start();
        
        // 检查预制体是否已分配
        if (divisionChildPrefab == null)
        {
            Debug.LogError($"分裂敌人 {gameObject.name} 的 divisionChildPrefab 未设置！请在Inspector中分配预制体。");
        }
    }

    // 重写TakeDamage方法，阻止在分裂过程中销毁
    public override bool TakeDamage(float damage)
    {
        if (live == false || isActiveAndEnabled == false || currentHP <= 0f)
            return false;
            
        if (invincible_flag == false && live)
        {
            Debug.Log("enemy take damage: " + damage);
            currentHP -= damage;
            
            // 设置无敌状态
            if (invincible_time > 0)
            {
                invincible_flag = true;
                invincible_timer = 0f;
            }

            // 生成伤害数字
            EnemyDamageNumberController.Instance.CreateDamageNumber(damage, transform.position + new Vector3(0, 1.5f, 0));

            // 当血量为0时，开始分裂流程，但不立即销毁
            if (currentHP <= 0f && !isDividing)
            {
                isDividing = true;
                StartCoroutine(DivideAndDestroy());
                return true;
            }
            
            // 启动闪烁效果
            if (!isFlashing && spriteRenderer != null)
            {
                StartCoroutine(FlashEffect());
            }
            return true;
        }
        return false;
    }
    
    // 分裂并延迟销毁
    private IEnumerator DivideAndDestroy()
    {
        // 禁用碰撞体，防止继续受到伤害
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
            
        // 停止移动
        rb.velocity = Vector2.zero;
        
        // 执行分裂过程
        yield return StartCoroutine(SpawnChildren());
        
        // 分裂完成后，手动调用基类的销毁逻辑
        live = false;
        ObjectPool.Instance.PushObject(gameObject);
    }

    // 生成子敌人的协程
    private IEnumerator SpawnChildren()
    {
        // 隐藏父敌人但不销毁
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
            
        // 检查预制体
        if (divisionChildPrefab == null)
        {
            Debug.LogError("分裂预制体未设置，无法生成子敌人");
            yield break;
        }
        
        // 添加延迟，分散生成时间
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
        // 生成子敌人
        for (int i = 0; i < divisionCount; i++)
        {
            try
            {
                // 随机位置偏移
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                // 从对象池获取对象
                GameObject child = ObjectPool.Instance.GetObject(divisionChildPrefab);
                if (child != null)
                {
                    child.transform.position = spawnPosition;
                    child.SetActive(true);
                    
                    // 初始化子敌人
                    DivisionChildEnemy childEnemy = child.GetComponent<DivisionChildEnemy>();
                    if (childEnemy != null)
                    {
                        childEnemy.Initialize();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成子敌人时发生错误: {e.Message}");
            }
        }
    }
}
