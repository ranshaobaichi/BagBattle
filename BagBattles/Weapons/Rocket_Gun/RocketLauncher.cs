using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : Gun
{
    // public int rocketNum;
    // public float rocketAngle;

    // protected override void Fire()
    // {
    //     // yield return new WaitForSeconds(delay);
    //     int median = rocketNum / 2;
    //     for (int i = 0; i < rocketNum; i++)
    //     {
    //         GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
    //         bullet.transform.position = muzzlePos.position;
    //         SetBullet(bullet);

    //         if (rocketNum % 2 == 1)
    //         {
    //             bullet.transform.right = Quaternion.AngleAxis(rocketAngle * (i - median), Vector3.forward) * direction;
    //         }
    //         else
    //         {
    //             bullet.transform.right = Quaternion.AngleAxis(rocketAngle * (i - median) + rocketAngle / 2, Vector3.forward) * direction;
    //         }
    //         bullet.GetComponent<Rocket>().SetTarget(mousePos);
    //     }

    //     attack_timer = attack_speed;
    //     attack_flag = false;
    // }
}
