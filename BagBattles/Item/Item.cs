using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.BagBattles.Types;

public abstract class Item
{
    [Serializable]
    public enum ItemType
    {
        None,
        TriggerItem,
        BulletItem,
        FoodItem,
        SurroundItem,
        // Add other item types here
    }
    
    protected InventoryItem sourceInventoryItem; // 对应仓库中物品
    public abstract void UseItem(); // 使用道具
    public abstract object GetSpecificItemType(); // 获取具体的道具类型
    public ItemType itemType; // 道具类型
    public Guid sourceInventoryItemGuid;
    public ItemType GetItemType()
    {
        if (itemType == ItemType.None)
        {
            throw new ArgumentOutOfRangeException(nameof(itemType), "道具类型未设置");
        }
        return itemType;
    }
    public void SetSourceInventoryItem(InventoryItem item)
    {
        sourceInventoryItem = item;
        sourceInventoryItemGuid = item.inventoryID;
    }
}
