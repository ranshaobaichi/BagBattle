using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swallow_Bullet : Bullet
{
    private Vector2 movePos;
    private int rotationSpeed;
    private Vector3 init_scale;
    private HashSet<Bullet> enemyBullets = new HashSet<Bullet>();
    private float current_damage;
    [Header("吞噬系数")]
    [Tooltip("每次吞噬变大百分比")]public float larger_param;
    [Tooltip("每次吞噬增加伤害")]public float damageUp;
    [Tooltip("体积最大增加倍数")]public float max_scale;

    void Start()
    {
        init_scale = transform.localScale;
        max_scale += 1;
    }
    private new void Update()
    {
        base.Update();
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        rigidbody.velocity = movePos * bulletBasicAttribute.speed;
    }

    public void SetBullet(int rotationSpeed, float larger_param, float damageUp, float max)
    {
        enemyBullets.Clear();
        max_scale = max;
        current_damage = bulletBasicAttribute.damage;
        this.rotationSpeed = rotationSpeed;
        this.larger_param = larger_param;
        transform.localScale = init_scale;
        this.damageUp = damageUp;
    }

    public override void SetSpeed(Vector2 direction)
    {
        rigidbody.velocity = direction * bulletBasicAttribute.speed;
        movePos = direction;
    }

    // 重写 Del 方法，使用对象池回收子弹
    public override void Del()
    {
        transform.localScale = init_scale;
        enemyBullets.Clear();
        base.Del();
    }

    IEnumerator DelayDel()
    {
        yield return new WaitForSeconds(1f);
        Del();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object has been destroyed or is invalid
        if (other == null || other.gameObject == null)
        {
            return;
        }

        string tag = other.tag;

        switch (tag)
        {
            case "Enemy":
                if (other.gameObject != null) // Ensure enemy is valid
                {
                    other.gameObject.GetComponent<EnemyController>()?.TakeDamage(current_damage);
                    current_pass_num--;
                    if (current_pass_num < 0) Del();
                }
                break;
            //TODO:体积逐渐变大
            case "Enemy_Bullet":
                if(enemyBullets.Contains(other.GetComponent<Bullet>()))
                {
                    return;
                }
                if (other.gameObject != null) // Ensure enemy bullet is valid
                {
                    transform.localScale = transform.localScale * (1 + larger_param);
                    if (transform.localScale.x > max_scale) transform.localScale = max_scale * init_scale;
                    current_pass_num++;
                    current_damage += damageUp;

                    Bullet enemyBullet = other.GetComponent<Bullet>();
                    enemyBullets.Add(enemyBullet);

                    if (enemyBullet != null)
                    {
                        enemyBullet.Del(); // Safely destroy the enemy bullet
                    }
                }
                break;

            case "Wall":
                if (current_pass_num >= 0)
                {
                    current_pass_num--;
                    StartCoroutine(DelayDel());
                }
                else
                    Del();
                break;
        }
    }

}
