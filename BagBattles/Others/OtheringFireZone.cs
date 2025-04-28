using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtheringFireZone : Othering
{
    [Header("燃烧区域伤害属性")]
    [Tooltip("燃烧伤害")] public float damage;
    [Tooltip("施加的燃烧层数")] public int fireLevel;
    [Tooltip("施加的减速层数")] public int iceLevel;
    [Tooltip("持续时间")] public float duration;
    [Tooltip("伤害时间间隔")] public float tickInterval;
    [Tooltip("伤害半径")] public float radius;

    public GameObject fireEffectPrefab; //火焰粒子预制体
    private GameObject fireEffectInstance; //火焰粒子实例

    private List<EnemyController> enemiesInZone = new List<EnemyController>();

    void OnEnable()
    {
        transform.position = PlayerController.Instance.transform.position;
        fireEffectInstance = ObjectPool.Instance.GetObject(fireEffectPrefab);
        fireEffectInstance.transform.position = transform.position;
        fireEffectInstance.transform.SetParent(transform);
        StartCoroutine(DamageOverTime());
    }

    void OnDisable()
    {
        ObjectPool.Instance.PushObject(fireEffectInstance);
        fireEffectInstance = null;
        StopAllCoroutines();
    }

    private void DetectEnemiesInZone()
    {
        enemiesInZone.Clear();
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                collider.TryGetComponent<EnemyController>(out EnemyController enemy);
                if (enemy != null)
                {
                    enemiesInZone.Add(enemy);
                }
            }
        }
    }

    private IEnumerator DamageOverTime()
    {
        float timer = 0f;
        while (timer < duration)
        {
            DetectEnemiesInZone();
            foreach (var enemy in enemiesInZone)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    enemy.SetFire(fireLevel);
                    enemy.SetIce(iceLevel);
                }
            }
            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }

        ObjectPool.Instance.PushObject(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}