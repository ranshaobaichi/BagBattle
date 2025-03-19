using UnityEngine;

public class BulletInventoryItem : InventoryItem
{
    [Header("子弹道具")]
    public Item.BulletItemAttribute bulletAttribute;
    public BulletInventoryItem() => itemType = Item.ItemType.BulletItem;
    public Bullet.BulletType GetBulletType() => bulletAttribute.bulletType;
    public override object GetAttribute() => bulletAttribute;
}