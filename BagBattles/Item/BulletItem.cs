using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletItem : Item
{
    [Header("子弹类道具")]
    public BulletItemAttribute bulletAttribute;
    public override void UseItem()
    {
        Debug.Log("子弹道具使用");
        BulletSpawner.Instance.LoadBullet(bulletAttribute.bulletType, bulletAttribute.bulletCount);
    }
    public override object GetAttribute() => bulletAttribute;
}
