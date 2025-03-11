using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class Jump_Bullet : Bullet
{
    private Vector2 x_direct, y_direct;
    private float v_x, v_y;

    private float timer = 0;

    public void SetBullet(float x, float y)
    {
        v_x = x;
        v_y = y;
    }

    public override void SetSpeed(Vector2 direction)
    {
        x_direct = direction.normalized;
        y_direct = new Vector2(-x_direct.y, x_direct.x).normalized;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            CauseDamage(other.gameObject.GetComponent<EnemyController>());
            current_pass_num--;
            if(current_pass_num < 0)
                Del();
        }
        if (other.CompareTag("Wall"))
            Del();
    }

    protected override void Update()
    {
        base.Update();
        timer += Time.deltaTime;

        Vector2 speed_y = (float)Math.Cos(v_x * timer) * y_direct;
        rigidbody.velocity = speed_y * v_y + x_direct * v_x;
    }
}