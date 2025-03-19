using UnityEngine;

public class InventoryTimeTriggerItem : TriggerInventoryItem
{
    [Header("时间触发道具")]
    public Trigger.TimeTriggerAttribute timeTriggerAttribute;
    public override Trigger.TriggerType GetTriggerType() => timeTriggerAttribute.triggerType;
    public override object GetAttribute() => timeTriggerAttribute;
}