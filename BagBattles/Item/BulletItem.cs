using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletItem : Item
{
    [Header("子弹类道具")]
    public BulletItemAttribute bulletAttribute;
    public override void UseItem()
    {
        BulletSpawner.Instance.LoadBullet(bulletAttribute.bulletType, bulletAttribute.bulletCount);
    }
    public override object GetItemAttribute() => bulletAttribute;
}
