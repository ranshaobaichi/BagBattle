using UnityEngine;

public class TimeTriggerItem : TriggerItem
{
    private Trigger.TimeTriggerAttribute timeTriggerAttribute;
    public override Trigger.TriggerType GetTriggerType() => timeTriggerAttribute.triggerType;

    protected override void InitializeAttr(object specificType)
    {
        if (specificType is not Assets.BagBattles.Types.TimeTriggerType type)
        {
            Debug.LogError("触发器属性类型错误");
            return;
        }
        timeTriggerAttribute = ItemAttribute.Instance.GetAttribute(Item.ItemType.TriggerItem, Trigger.TriggerType.ByTime, type) as Trigger.TimeTriggerAttribute;
        if (timeTriggerAttribute == null)
        {
            Debug.LogError($"TimeAttribute触发器属性初始化失败, itemid{transform.GetInstanceID()},itemname{gameObject.name}");
            return;
        }
    }
    
    public override void StartTrigger()
    {
        if (timeTriggerAttribute == null)
        {
            Debug.LogError("触发器属性未初始化");
            return;
        }
        // 触发器的使用逻辑
        InvokeRepeating(nameof(TriggerItems), timeTriggerAttribute.triggerTime, timeTriggerAttribute.triggerTime);
    }    

    public override void StopTrigger()
    {
        CancelInvoke(nameof(TriggerItems));
        Debug.Log("触发器已停用");
    }
}