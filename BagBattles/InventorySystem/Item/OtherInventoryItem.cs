using UnityEngine;
using Assets.BagBattles.Types;

public class OtherInventoryItem : InventoryItem
{
    [Header("其他类型道具")]
    public OtherType otherItemType;
    public OtherInventoryItem() => itemType = Item.ItemType.OtherItem;
    public override object GetSpecificType() => otherItemType;
    
    public override bool Initialize(object otherType)
    {
        if (otherType is not OtherType type)
        {
            Debug.LogError($"其他类型道具类型错误,无法获取其他类型道具属性");
            return false;
        }
        this.otherItemType = type;

        // 形状设置
        itemShape = ItemAttribute.Instance.GetItemShape(itemType, type);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, type));
        description = ItemAttribute.Instance.GetDescription(itemType, type);
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"其他类型道具初始化错误,无法获取其他类型形状");
            return false;
        }
        Debug.Log($"其他类型道具种类：{type} 形状：{itemShape}");
        triggerDectectFlag = true;
        return true;
    }
}