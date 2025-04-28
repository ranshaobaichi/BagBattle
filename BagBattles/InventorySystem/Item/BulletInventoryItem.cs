using UnityEngine;
using Assets.BagBattles.Types;

public class BulletInventoryItem : InventoryItem
{
    [Header("子弹道具")]
    public BulletType bulletItemType;
    public BulletInventoryItem() => itemType = Item.ItemType.BulletItem;
    public override object GetSpecificType() => bulletItemType;
    public override bool Initialize(object bulletType)
    {
        if (bulletType is not BulletType type)
        {
            Debug.LogError($"子弹道具类型错误,无法获取子弹道具属性");
            return false;
        }
        this.bulletItemType = type;

        // 形状设置
        itemShape = ItemAttribute.Instance.GetItemShape(itemType, type);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, type));
        description = ItemAttribute.Instance.GetDescription(itemType, type);
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"子弹道具初始化错误,无法获取子弹形状");
            return false;
        }
        Debug.Log($"子弹道具种类：{type} 形状：{itemShape}");
        triggerDectectFlag = true;
        return true;
    }
}