using UnityEngine;
public class ByOtherTriggerItem : TriggerItem
{
    private Trigger.ByOtherTriggerAttribute byOtherTriggerAttribute;
    public override Trigger.TriggerType GetTriggerType() => byOtherTriggerAttribute.triggerType;
    public override object GetSpecificTriggerType() => byOtherTriggerAttribute.byOtherTriggerType;
    private int currentByOtherCount = 0;

    protected override void InitializeAttr(object specificType)
    {
        if (specificType is not Assets.BagBattles.Types.ByOtherTriggerType type)
        {
            Debug.LogError("触发器属性类型错误");
            return;
        }
        byOtherTriggerAttribute = ItemAttribute.Instance.GetAttribute(Item.ItemType.TriggerItem, Trigger.TriggerType.ByOtherTrigger, type) as Trigger.ByOtherTriggerAttribute;
        currentByOtherCount = 0;
    }

    public override void StartTrigger()
    {
        if (byOtherTriggerAttribute == null)
        {
            Debug.LogError("触发器属性未初始化");
            return;
        }
        // 监听开火事件
        foreach (var trigger in PlayerController.Instance.triggerItems)
        {
            if (trigger.GetTriggerType() != Trigger.TriggerType.ByOtherTrigger)
            {
                trigger.triggerEvent.AddListener(ReceiveOtherTriggers);
            }
        }
    }

    public override void StopTrigger()
    {
        currentByOtherCount = 0; // 重置开火次数
        Debug.Log("触发器已停用");
        // 取消监听开火事件
        foreach (var trigger in PlayerController.Instance.triggerItems)
        {
            if (trigger.GetTriggerType() != Trigger.TriggerType.ByOtherTrigger)
            {
                trigger.triggerEvent.RemoveListener(ReceiveOtherTriggers);
            }
        }
    }
    
    private void ReceiveOtherTriggers()
    {
        currentByOtherCount++;
        Debug.Log($"当前其他触发器触发次数：{currentByOtherCount}");
        if (currentByOtherCount >= byOtherTriggerAttribute.requiredTriggerCount)
        {
            TriggerItems();
            currentByOtherCount = 0; // 重置开火次数
        }
    }
}