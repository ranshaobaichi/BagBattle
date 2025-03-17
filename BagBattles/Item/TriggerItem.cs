using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItem : MonoBehaviour
{
    [Header("触发器类道具")]
    private Trigger.BaseTriggerAttribute triggerItemAttribute;
    private Trigger.TriggerType triggerType; // 触发器类型
    private HashSet<Item> items = new();    // 触发器可触发的物品
    public void Initialize(object attr, Dictionary<Item.ItemType, List<object>> triggerItem, Trigger.TriggerType triggerType)
    {
        this.triggerType = triggerType;
        foreach (var (itemKey, itemValue) in triggerItem)
            switch (itemKey)
            {
                case Item.ItemType.BulletItem:
                    foreach (var item in itemValue)
                    {
                        BulletItem tmp = new()
                        {
                            bulletAttribute = (Item.BulletItemAttribute)item
                        };
                        items.Add(tmp);
                    }
                    break;
                case Item.ItemType.FoodItem:
                    Debug.LogError("FoodItem触发器未实现");
                    break;
                default:
                    Debug.LogError($"触发器不支持的物品类型：{itemKey}");
                    throw new ArgumentOutOfRangeException();
            }
    }

    // BUG: 触发器的属性未能工作
    public void StartTrigger()
    {
        Debug.Log("触发器开始工作");
        // 触发器的使用逻辑
        switch (triggerItemAttribute.triggerType)
        {
            case Trigger.TriggerType.ByTime:
                if (triggerItemAttribute is Trigger.TimeTriggerAttribute timeAttr)
                {
                    InvokeRepeating(nameof(TriggerItems), timeAttr.triggerTime, timeAttr.triggerTime);
                }
                break;
            case Trigger.TriggerType.ByFireTimes:
                // 在这里实现基于开火次数的触发逻辑
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void StopTrigger()
    {
        // 触发器的停用逻辑
        switch (triggerItemAttribute.triggerType)
        {
            case Trigger.TriggerType.ByTime:
                CancelInvoke(nameof(TriggerItems));
                break;
            case Trigger.TriggerType.ByFireTimes:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void TriggerItems()
    {
        // 触发器的触发逻辑
        foreach (var item in items)
        {
            item.UseItem();
        }
    }

    // public override void UseItem()
    // {
    //     // 触发器的使用逻辑
    //     StartTrigger();
    // }
    // public override object GetItemAttribute() => triggerItemAttribute;
}
