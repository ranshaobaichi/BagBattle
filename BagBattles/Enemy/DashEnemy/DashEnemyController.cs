using UnityEngine;
using System.Collections;

public class DashEnemyController : EnemyController
{
    [Header("冲刺配置")]
    public float maxDashSpeed = 15f;       // 最大冲刺速度
    public float dashDuration = 0.5f;      // 冲刺持续时间
    public float dashCooldown = 3f;        // 冲刺冷却时间
    public float detectionRange = 10f;     // 检测范围
    public float preparationTime = 0.5f;   // 冲刺前的准备时间
    
    [Header("速度曲线")]
    public AnimationCurve dashSpeedCurve;  // 速度变化曲线
    
    private Vector2 dashDirection;          // 冲刺方向
    private bool canDash = true;            // 是否可以冲刺
    private bool isDashing = false;         // 是否正在冲刺
    private bool hitWall = false;
    
    protected override void Start()
    {
        base.Start();        
        // 确保敌人的碰撞设置正确
        if (rb != null)
        {
            // 如果使用动态刚体，确保它可以与墙壁正确碰撞
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // 设置冲刺敌人类型
        enemy_type = Enemy.EnemyType.DashEnemy;
    }
    
    protected override void Update()
    {
        // 冲刺时不执行普通的追踪行为
        // 不在冲刺状态时执行正常的敌人行为
        base.Update();
        if (isDashing)
            return;
            
        // 检测是否可以冲刺
        if (canDash && !isDashing && attack_flag)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= detectionRange)
            {
                StartCoroutine(DashWithCurve());
                return;
            }
        }
    }
    
    private IEnumerator DashWithCurve()
    {
        canDash = false;
        hitWall = false;
        
        // 1. 准备阶段
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // 变红警示玩家
        }
        
        // 记录冲刺方向 (朝向玩家)
        dashDirection = (player.transform.position - transform.position).normalized;
        
        // 冲刺前转向目标
        if (player.transform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
        
        // 等待准备时间
        yield return new WaitForSeconds(preparationTime);
        
        // 2. 开始冲刺
        isDashing = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // 恢复正常颜色
        }
        
        // 3. 执行冲刺
        float timer = 0;
        while (timer < dashDuration && !hitWall)
        {
            // 计算当前时间点的归一化值 (0-1)
            float normalizedTime = timer / dashDuration;
            
            // 从曲线获取当前时间点对应的速度系数 (0-1)
            float speedFactor = dashSpeedCurve.Evaluate(normalizedTime);
            
            // 使用刚体的velocity进行移动，这样会自动处理碰撞
            rb.velocity = dashDirection * maxDashSpeed * speedFactor;
            
            // 更新计时器
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 4. 结束冲刺
        rb.velocity = Vector2.zero;
        isDashing = false;
        
        // 5. 进入冷却阶段
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果撞墙了
        if (isDashing && collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            hitWall = true;
            
            // 可选：添加撞墙特效
            // Instantiate(wallHitEffect, collision.contacts[0].point, Quaternion.identity);
        }
    }
    
    // 覆盖移动方法，在冲刺时阻止正常移动
    protected override void find_way()
    {
        if (!isDashing)
        {
            base.find_way();
        }
    }
    
    // 在编辑器中可视化检测范围和冲刺路径
    protected virtual void OnDrawGizmosSelected()
    {        
        // 显示冲刺检测范围(青色)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}