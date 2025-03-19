using UnityEngine;

public class BulletItem : Item
{
    [Header("子弹类道具")]
    public Item.BulletItemAttribute bulletAttribute;

    public BulletItem(BulletItemAttribute bulletItemAttribute)
    {
        bulletAttribute = bulletItemAttribute;
    }

    public override void UseItem()
    {
        Debug.Log("子弹道具使用");
        BulletSpawner.Instance.LoadBullet(bulletAttribute.bulletType, bulletAttribute.bulletCount);
    }
}
