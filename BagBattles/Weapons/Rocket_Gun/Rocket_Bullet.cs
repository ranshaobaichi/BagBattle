using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket_Bullet : Bullet
{
    [Header("火箭弹设置")]
    [Tooltip("一次发射火箭弹数量")] public int num;
    [Tooltip("偏转速度")] public float lerp;
    private Vector3 targetPos;
    private Vector3 direction;
    private bool arrived;

    public void SetTarget(Vector2 _target)
    {
        arrived = false;
        targetPos = _target;
    }

    private void FixedUpdate()
    {
        direction = (targetPos - transform.position).normalized;

        if (!arrived)
        {
            transform.right = Vector3.Slerp(transform.right, direction, lerp / Vector2.Distance(transform.position, targetPos));
            rigidbody.velocity = transform.right * bulletBasicAttribute.speed;
        }
        if (Vector2.Distance(transform.position, targetPos) < .5f && !arrived)
        {
            arrived = true;
        }

        // float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            current_pass_num--;
            if (current_pass_num < 0)
                Del();
        }
        if (other.CompareTag("Wall"))
            Del();
    }

    // IEnumerator Push(GameObject _object, float time)
    // {
    //     yield return new WaitForSeconds(time);
    //     ObjectPool.Instance.PushObject(_object);
    // }
}
