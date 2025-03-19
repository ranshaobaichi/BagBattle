using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerItem : MonoBehaviour
{
    [Header("触发器类道具")]
    public Dictionary<Item.ItemType, List<Item>> items = new Dictionary<Item.ItemType, List<Item>>();    // 触发器可触发的物品

    #region 虚方法
    protected abstract void InitializeAttr(object attr); // 初始化触发器属性
    public abstract void StartTrigger();
    public abstract void StopTrigger(); // 停用触发器
    public abstract Trigger.TriggerType GetTriggerType(); // 触发器触发物品
    #endregion

    public void Initialize(object attr, Dictionary<Item.ItemType, List<object>> triggerItem)
    {
        // 初始化触发器属性
        InitializeAttr(attr);

        // 初始化触发器物品字典
        items[Item.ItemType.BulletItem] = new();
        items[Item.ItemType.FoodItem] = new();


        // 初始化触发器物品
        foreach (var (itemKey, itemValue) in triggerItem)
        {
            switch (itemKey)
            {
                case Item.ItemType.BulletItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个子弹道具");
                    foreach (var value in itemValue)
                    {
                        try
                        {
                            BulletItem tmp = new((BulletItem.BulletItemAttribute)value);
                            Debug.Log($"子弹道具属性：{tmp.bulletAttribute.bulletCount} {tmp.bulletAttribute.bulletType}");
                            items[Item.ItemType.BulletItem].Add(tmp);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"创建子弹道具失败: {ex.Message}");
                            Debug.LogException(ex);
                        }
                    }
                    break;
                case Item.ItemType.FoodItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个食物道具");
                    foreach (var value in itemValue)
                    {
                        try
                        {
                            FoodItem tmp = new((FoodItem.FoodItemAttribute)value);
                            foreach (var foodItemAttribute in ((FoodItem.FoodItemAttribute)value).foodItemAttributes)
                            {
                                Debug.Log($"食物道具加成种类：{foodItemAttribute.foodBonusType} " +
                                          $"食物道具加成数值：{foodItemAttribute.foodBonusValue} " +
                                          $"食物道具持续时间类型：{foodItemAttribute.foodDurationType} " +
                                          $"食物道具加成持续时间（回合数）：{foodItemAttribute.roundLeft}");
                            }
                            items[Item.ItemType.FoodItem].Add(tmp);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"创建食物道具失败: {ex.Message}");
                            Debug.LogException(ex);
                        }
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
        }

        // 食物道具触发后清空
        items[Item.ItemType.FoodItem].Clear();
    }

    public virtual void Destroy()
    {
        // 触发器销毁时的逻辑
        StopTrigger();
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
