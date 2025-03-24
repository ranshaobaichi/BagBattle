
using System.Collections.Generic;
using Assets.BagBattles.Types;

public class FoodInventoryItem : InventoryItem
{
    public FoodType foodType = FoodType.None;
    public FoodInventoryItem() => itemType = Item.ItemType.FoodItem;
    public override object GetSpecificType() => foodType;
    public override bool Initialize(object foodType)
    {
        if (foodType is not FoodType type)
        {
            UnityEngine.Debug.LogError($"食物道具类型错误,无法获取食物道具属性");
            return false;
        }
        this.foodType = type;

        // 形状设置
        itemShape = ItemAttribute.Instance.GetItemShape(itemType, type);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, type));
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            UnityEngine.Debug.LogError($"食物道具初始化错误,无法获取食物形状");
            return false;
        }
        UnityEngine.Debug.Log($"食物道具种类：{type} 形状：{itemShape}");

        return true;
    }
}