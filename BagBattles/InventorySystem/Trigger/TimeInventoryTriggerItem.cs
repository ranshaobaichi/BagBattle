using UnityEngine;
using Assets.BagBattles.Types;

public class TimeInventoryTriggerItem : TriggerInventoryItem
{
    [Header("时间触发道具")]
    public TimeTriggerType timeTriggerType;
    public override bool Initialize(object type)
    {
        if (type is not TimeTriggerType timeTriggerType)
        {
            Debug.LogError($"触发器类型错误,无法获取触发器属性");
            return false;
        }
        triggerType = Trigger.TriggerType.ByTime;
        this.timeTriggerType = timeTriggerType;

        itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, timeTriggerType);
        triggerRange = ItemAttribute.Instance.GetTriggerRange(triggerType, timeTriggerType);
        description = ItemAttribute.Instance.GetDescription(itemType, triggerType, timeTriggerType);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, triggerType, timeTriggerType));
        if (itemShape == InventoryItem.ItemShape.NONE ||
           itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"触发器初始化错误,无法获取触发器形状");
            return false;
        }

        triggerDectectFlag = true;
        return true;
    }
    public override object GetSpecificType() => timeTriggerType;
}