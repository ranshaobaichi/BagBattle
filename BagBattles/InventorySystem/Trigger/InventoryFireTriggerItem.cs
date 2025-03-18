using UnityEngine;

public class InventoryFireTriggerItem : InventoryTriggerItem
{
    [Header("开火触发道具")]
    public Trigger.FireCountTriggerAttribute fireTriggerAttribute;
    public override Trigger.TriggerType GetTriggerType() => fireTriggerAttribute.triggerType;
    public override object GetAttribute() => fireTriggerAttribute;
}