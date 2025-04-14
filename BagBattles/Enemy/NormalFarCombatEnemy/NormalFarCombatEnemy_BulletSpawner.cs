using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalFarCombatEnemy_BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    [SerializeField]
    [Header("攻击属性")]
    [Tooltip("攻速")] public float attack_speed;
    [Tooltip("是否发射角度随机偏移")] public bool random_angel;
    private float attack_timer;
    [Header("标志位")]
    private bool attack_flag;
    private void Start()
    {
        attack_flag = true;
        attack_timer = 0.0f;
    }

    private void Update()
    {
        if (attack_flag == false)
            attack_timer -= Time.deltaTime;

        if (attack_timer <= 0)
        {
            attack_flag = true;
            attack_timer = attack_speed;
        }
    }
    
    public void Fire(Vector3 pos)
    {
        if (attack_flag == true)
        {
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = transform.position;

            // random angel
            if (random_angel == true)
            {
                float angel = Random.Range(-5f, 5f);
                bullet.GetComponent<Bullet>().SetSpeed(Quaternion.AngleAxis(angel, Vector3.forward) * pos);
            }
            else
                bullet.GetComponent<Bullet>().SetSpeed(pos);

            attack_timer = attack_speed;
            attack_flag = false;
        }
    }
}
