using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletItem : Item
{
    [Header("子弹类道具")]
    [Tooltip("子弹类型")] public Bullet.BulletType bulletType;
    [Tooltip("每次激活向弹夹中加入子弹数量")] public int bulletCount;
    public override void UseItem()
    {
        BulletSpawner.Instance.LoadBullet(bulletType, bulletCount);
    }
}
