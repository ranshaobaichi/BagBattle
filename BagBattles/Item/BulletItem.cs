using UnityEngine;
using Assets.BagBattles.Types;

public class BulletItem : Item
{
    [Header("子弹类道具")]
    public Item.BulletItemAttribute bulletAttribute;

    public BulletItem(BulletType bulletType)
    {
        if(ItemAttribute.Instance.GetAttribute(Item.ItemType.BulletItem, bulletType) is Item.BulletItemAttribute attr)
        {
            bulletAttribute = attr;
        }
        else
        {
            Debug.LogError("无法获取子弹道具属性");
        }
    }

    public override void UseItem()
    {
        Debug.Log("子弹道具使用");
        BulletSpawner.Instance.LoadBullet(bulletAttribute.bulletType, bulletAttribute.bulletCount);
    }
}
