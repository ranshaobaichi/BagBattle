using System;
using System.Collections.Generic;
using Assets.BagBattles.Types;
using UnityEngine;
using UnityEngine.Events;

public abstract class TriggerItem : MonoBehaviour
{
    [Header("触发器类道具")]
    public Dictionary<Item.ItemType, List<Item>> items = new Dictionary<Item.ItemType, List<Item>>();    // 触发器可触发的物品
    public UnityEvent triggerEvent = new UnityEvent(); // 触发器触发事件
    #region 虚方法
    protected abstract void InitializeAttr(object specificType); // 初始化触发器属性
    public abstract void StartTrigger();
    public abstract void StopTrigger(); // 停用触发器
    public abstract Trigger.TriggerType GetTriggerType(); // 触发器触发物品
    public abstract object GetSpecificTriggerType();
    public Guid sourceTriggerInventoryItemGuid; // 触发器对应的仓库物品ID
    #endregion

    public void LaunchTrigger()
    {
        Debug.Log("触发器开始工作");
        StartTrigger();
    }
    public void Initialize(Guid guid, object specificType, Dictionary<Item.ItemType, List<(InventoryItem inventorySource, object specificType)>> triggerItem)
    {
        // 初始化触发器属性
        InitializeAttr(specificType);
        sourceTriggerInventoryItemGuid = guid;

        // 初始化触发器物品字典
        items[Item.ItemType.BulletItem] = new();
        items[Item.ItemType.FoodItem] = new();
        items[Item.ItemType.SurroundItem] = new();
        items[Item.ItemType.OtherItem] = new();

        // 初始化触发器物品
        foreach (var (itemKey, itemValue) in triggerItem)
        {
            switch (itemKey)
            {
                case Item.ItemType.BulletItem:
                    Debug.Log($"触发器绑定了{itemValue.Count}个子弹道具");
                    foreach (var (inventorySource, value) in itemValue)
                    {
                        if (value is not BulletType bulletType)
                        {
                            Debug.LogError("触发器绑定了无效的子弹类型" + value);
                            continue;
                        }
                        BulletItem tmp = new(bulletType);
                        tmp.SetSourceInventoryItem(inventorySource);
                        Debug.Log($"子弹道具属性：{tmp.bulletAttribute.bulletCount} {tmp.bulletAttribute.bulletType}");
                        items[Item.ItemType.BulletItem].Add(tmp);
                    }
                    break;
                case Item.ItemType.FoodItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个食物道具");
                    foreach (var (inventorySource, value) in itemValue)
                    {
                        if (value is not FoodType foodType)
                        {
                            Debug.LogError("触发器绑定了无效的子弹类型" + value);
                            continue;
                        }
                        FoodItem tmp = new(foodType);
                        tmp.SetSourceInventoryItem(inventorySource);
                        foreach (var food in tmp.foodItemAttributes.foodItemAttributes)
                        {
                            Debug.Log($"食物道具属性：{food.foodBonusType} {food.foodBonusValue} {food.foodDurationType} {food.timeLeft}");
                        }
                        items[Item.ItemType.FoodItem].Add(tmp);
                    }
                    break;
                case Item.ItemType.SurroundItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个环绕物道具");
                    foreach (var (inventorySource, value) in itemValue)
                    {
                        if (value is not SurroundType surroundType)
                        {
                            Debug.LogError("触发器绑定了无效的环绕物类型" + value);
                            continue;
                        }
                        SurroundItem tmp = new(surroundType);
                        tmp.SetSourceInventoryItem(inventorySource);
                        items[Item.ItemType.SurroundItem].Add(tmp);
                    }
                    break;
                case Item.ItemType.OtherItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个其他类别道具");
                    foreach (var (inventorySource, value) in itemValue)
                    {
                        if (value is not OtherType otherType)
                        {
                            Debug.LogError("触发器绑定了无效的其他类别类型" + value);
                            continue;
                        }
                        OtherItem tmp = new(otherType);
                        tmp.SetSourceInventoryItem(inventorySource);
                        items[Item.ItemType.OtherItem].Add(tmp);
                    }
                    break;
                default:
                    Debug.LogError($"触发器不支持的物品类型：{itemKey}");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public void TriggerItems()
    {
        Debug.Log("触发器触发物品");
        // 触发器的触发逻辑
        foreach (var itemList in items)
        {
            foreach (var item in itemList.Value)
            {
                if (item == null)
                {
                    Debug.LogError("触发器触发的物品为空");
                    continue;
                }
                item.UseItem();
            }
            if (itemList.Key == Item.ItemType.FoodItem)
            {
                for (int i = itemList.Value.Count - 1; i >= 0; i--)
                {
                    var item = itemList.Value[i];
                    if (item is FoodItem foodItem && foodItem.foodItemAttributes.destroyCount == 0)
                    {
                        items[Item.ItemType.FoodItem].Remove(item);
                        InventoryManager.Instance.RemoveFoodItem(item.sourceInventoryItem as FoodInventoryItem);
                        if (items[Item.ItemType.FoodItem].Count == 0)
                        {
                            items.Remove(Item.ItemType.FoodItem);
                            Debug.Log("触发器触发的所有食物道具已被销毁");
                        }
                    }
                }
            }
        }

        Debug.Log("触发器触发物品成功");
        triggerEvent?.Invoke();
    }

    public virtual void Destroy()
    {
        // 触发器销毁时的逻辑
        StopTrigger();

        // 清除环绕物
        foreach (var surround in items[Item.ItemType.SurroundItem])
        {
            if (surround == null || surround is not SurroundItem surroundItem)
            {
                Debug.LogError("触发器销毁的环绕物为空");
                continue;
            }
            surroundItem.DestroySurroundItem();
        }

        items.Clear();
        Debug.Log("触发器销毁");
    }
    // public override void UseItem()
    // {
    //     // 触发器的使用逻辑
    //     StartTrigger();
    // }
    // public override object GetItemAttribute() => triggerItemAttribute;
}
