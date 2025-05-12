using UnityEngine;

public class ExplosionEnemy : EnemyController
{
    [Header("爆炸属性")]
    public float explosion_radius;
    public float explosion_damage;
    public float explosion_time;
    [SerializeField] private GameObject explosionPrefab; // 爆炸区域预制体
    protected override void Start()
    {
        base.Start();
        // Initialize enemy-specific properties or behaviors here
        enemy_type = Enemy.EnemyType.ExplosionEnemy;
    }
    public override void OnDead()
    {
        base.OnDead();
        Explode();
    }
    private void Explode()
    {
        GameObject bomb = ObjectPool.Instance.GetObject(explosionPrefab);
        bomb.SetActive(true);
        bomb.transform.position = transform.position;
        ExplosionEnemyZone bombZone = bomb.GetComponent<ExplosionEnemyZone>();
        bombZone.Initialize(explosion_radius, explosion_damage, explosion_time);
    }
}