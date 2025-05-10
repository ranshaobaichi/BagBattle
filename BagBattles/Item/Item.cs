using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.BagBattles.Types;
using System.Collections;

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
        OtherItem,
    }

    public InventoryItem sourceInventoryItem; // 对应仓库中物品
    public ItemType itemType; // 道具类型
    public Guid sourceInventoryItemGuid;
    protected bool canBeTriggered = true; // 是否可以触发

    protected abstract void UseItem(); // 使用道具
    public abstract object GetSpecificItemType(); // 获取具体的道具类型
    public abstract float GetTriggerInterval(); // 获取触发间隔时间

    public virtual void Use()
    {
        if (!canBeTriggered)
        {
            Debug.Log($"道具{itemType}: {GetSpecificItemType()}触发间隔时间未到，无法使用道具");
            return;
        }
        canBeTriggered = false;
        UseItem();
    }

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
    
    public IEnumerator TriggerTimer()
    {
        if (GetTriggerInterval() <= 0)
        {
            canBeTriggered = true;
            yield break;
        }
        yield return new WaitForSeconds(GetTriggerInterval());
        canBeTriggered = true;
    }
}
