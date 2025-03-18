using UnityEngine;

public class TimeTriggerItem : TriggerItem
{
    private Trigger.TimeTriggerAttribute timeTriggerAttribute;
    public override Trigger.TriggerType GetType() => timeTriggerAttribute.triggerType;

    protected override void InitializeAttr(object attr)
    {
        timeTriggerAttribute = attr as Trigger.TimeTriggerAttribute;
        if (timeTriggerAttribute == null)
        {
            Debug.LogError("时间触发器属性初始化失败");
            return;
        }
    }

    public override void StartTrigger()
    {
        Debug.Log("触发器开始工作");
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