using UnityEngine;

public class Bomb_Bullet : Bullet
{

    [Header("爆炸参数")]
    [Tooltip("爆炸范围")] public float bomb_radius;
    [Tooltip("爆炸伤害")] public float bomb_damage;
    [Tooltip("爆炸时间")] public float bomb_time;
    [Tooltip("显示范围")] public bool show_range;

    [Space(10)]
    public GameObject bombPrefab; //爆炸区域预制体
    // public void SetBullet(float r,float b_damage, bool show)
    // {
    //     bomb_radius = r;
    //     bomb_damage = b_damage;
    //     show_range = show;
    // }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            Explode();
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
        if (other.CompareTag("Wall"))
        {
            Explode();
            Del();
        }
    }

    private void Explode()
    {
        GameObject bomb = ObjectPool.Instance.GetObject(bombPrefab);
        bomb.SetActive(true);
        bomb.transform.position = transform.position;
        BombZone bombZone = bomb.GetComponent<BombZone>();
        bombZone.Initialize(bomb_radius, bomb_damage, show_range, bomb_time);
    }
}
