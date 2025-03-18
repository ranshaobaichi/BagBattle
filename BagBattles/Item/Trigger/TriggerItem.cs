using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerItem : MonoBehaviour
{
    [Header("触发器类道具")]
    public List<Item> items = new();    // 触发器可触发的物品

    #region 虚方法
    protected abstract void InitializeAttr(object attr); // 初始化触发器属性
    public abstract void StartTrigger();
    public abstract void StopTrigger(); // 停用触发器
    public abstract new Trigger.TriggerType GetType(); // 触发器触发物品
    #endregion

    public void Initialize(object attr, Dictionary<Item.ItemType, List<object>> triggerItem)
    {
        // 初始化触发器属性
        InitializeAttr(attr);

        // 初始化触发器物品
        foreach (var (itemKey, itemValue) in triggerItem)
        {
            switch (itemKey)
            {
                case Item.ItemType.BulletItem:
                    Debug.Log($"触发器初始化了{itemValue.Count}个子弹道具");
                    foreach (var item in itemValue)
                    {
                        try 
                        {
                            //BUG: 未能成功创建子弹道具
                            BulletItem tmp = new BulletItem();
                            if(tmp == null)
                            {
                                Debug.LogError("创建子弹道具失败");
                                continue;
                            }
                            tmp.bulletAttribute = (BulletItem.BulletItemAttribute)item;
                            Debug.Log($"子弹道具属性：{tmp.bulletAttribute.bulletCount} {tmp.bulletAttribute.bulletType}");
                            items.Add(tmp);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"创建子弹道具失败: {ex.Message}");
                            Debug.LogException(ex);
                        }
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
    }

    public void TriggerItems()
    {
        Debug.Log($"触发器触发了{items.Count}个物品");
        // 触发器的触发逻辑
        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogError("触发器触发的物品为空");
                continue;    
            }
            item.UseItem();
        }
    }

    public virtual void Destroy()
    {
        // 触发器销毁时的逻辑
        StopTrigger();
        Destroy(this);
    }
    // public override void UseItem()
    // {
    //     // 触发器的使用逻辑
    //     StartTrigger();
    // }
    // public override object GetItemAttribute() => triggerItemAttribute;
}
