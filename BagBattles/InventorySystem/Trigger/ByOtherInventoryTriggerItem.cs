using UnityEngine;
using Assets.BagBattles.Types;
public class ByOtherInventoryTriggerItem : TriggerInventoryItem
{
    public ByOtherTriggerType byOtherTriggerType;

    public override bool Initialize(object type)
    {
        if (type is not ByOtherTriggerType byOtherTriggerType)
        {
            Debug.LogError($"触发器类型错误,无法获取触发器属性");
            return false;
        }
        triggerType = Trigger.TriggerType.ByOtherTrigger;
        this.byOtherTriggerType = byOtherTriggerType;

        itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, byOtherTriggerType);
        triggerRange = ItemAttribute.Instance.GetTriggerRange(triggerType, byOtherTriggerType);
        description = ItemAttribute.Instance.GetDescription(itemType, triggerType, byOtherTriggerType);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, triggerType, byOtherTriggerType));
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"触发器初始化错误,无法获取触发器形状");
            return false;
        }

        triggerDectectFlag = true;
        Debug.Log($"触发器种类：{byOtherTriggerType} 触发器范围：{triggerRange}");
        return true;
    }
    public override object GetSpecificType() => byOtherTriggerType;
}