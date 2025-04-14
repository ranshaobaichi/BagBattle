using UnityEngine;
using Assets.BagBattles.Types;
using UnityEngine.PlayerLoop;

public class FireInventoryTriggerItem : TriggerInventoryItem
{
    [Header("开火触发器：描述：【触发器名】【范围】【开火几次触发】")]
    public FireTriggerType fireTriggerType;

    /// <summary>
    /// 开火触发器初始化
    /// </summary>
    /// <param name="fireTriggerType">具体属性</param>
    /// <param name="direction">方向</param>
    public override bool Initialize(object type)
    {
        if (type is not FireTriggerType fireTriggerType)
        {
            Debug.LogError($"触发器类型错误,无法获取触发器属性");
            return false;
        }
        triggerType = Trigger.TriggerType.ByFireTimes;
        this.fireTriggerType = fireTriggerType;

        itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, fireTriggerType);
        triggerRange = ItemAttribute.Instance.GetTriggerRange(triggerType, fireTriggerType);
        InitializeDirection(ItemAttribute.Instance.GetItemDirection(itemType, triggerType, fireTriggerType));
        if (itemShape == InventoryItem.ItemShape.NONE ||
            itemDirection == InventoryItem.Direction.NONE)
        {
            Debug.LogError($"触发器初始化错误,无法获取触发器形状");
            return false;
        }

        triggerDectectFlag = true;
        Debug.Log($"触发器种类：{fireTriggerType} 触发器范围：{triggerRange}");
        return true;
    }
    public override object GetSpecificType() => fireTriggerType;
}