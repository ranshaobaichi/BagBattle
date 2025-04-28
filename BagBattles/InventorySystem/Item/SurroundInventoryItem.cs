using UnityEngine;
using Assets.BagBattles.Types;

public class SurroundInventoryItem : InventoryItem
{
    [Header("环绕物道具")]
    public SurroundType surroundItemType;
    public SurroundInventoryItem() => itemType = Item.ItemType.SurroundItem;
    public override object GetSpecificType() => surroundItemType;
    public override bool Initialize(object SurroundType)
    {
        if (SurroundType is not SurroundType type)
        {
            Debug.LogError($"环绕物道具类型错误,无法获取环绕物道具属性");
            return false;
        }
        this.surroundItemType = type;

        // 形状设置
        itemShape = ItemAttribute.Instance.GetItemShape(itemType, type);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, type));
        description = ItemAttribute.Instance.GetDescription(itemType, type);
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"环绕物道具初始化错误,无法获取环绕物形状");
            return false;
        }
        Debug.Log($"环绕物道具种类：{type} 形状：{itemShape}");
        triggerDectectFlag = true;
        return true;
    }
}