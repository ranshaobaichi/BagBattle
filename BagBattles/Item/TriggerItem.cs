using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItem : MonoBehaviour
{
    [Header("触发器类道具")]
    private Trigger.BaseTriggerAttribute triggerItemAttribute;
    private HashSet<Item> items = new();    // 触发器可触发的物品
    public void Initialize(Trigger.BaseTriggerAttribute type, Dictionary<Item.ItemType, List<object>> triggerItem)
    {
        triggerItemAttribute = type;
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
                    throw new ArgumentOutOfRangeException();
            }
    }

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
                    
                    // 如果设置了持续时间，安排停止触发
                    if (timeAttr.duration > 0)
                    {
                        Invoke(nameof(StopTrigger), timeAttr.duration);
                    }
                }
                else if (triggerItemAttribute is Trigger.TriggerItemAttribute legacyAttr)
                {
                    // 兼容旧代码
                    InvokeRepeating(nameof(TriggerItems), legacyAttr.triggerTime, legacyAttr.triggerTime);
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
}
