using UnityEngine;

public class FireTriggerItem : TriggerItem
{
    private Trigger.FireCountTriggerAttribute fireTriggerAttribute;
    public override Trigger.TriggerType GetTriggerType() => fireTriggerAttribute.triggerType;
    private int currentFireCount = 0;

    protected override void InitializeAttr(object attr)
    {
        fireTriggerAttribute = attr as Trigger.FireCountTriggerAttribute;
        if (fireTriggerAttribute == null)
        {
            Debug.LogError("开火触发器属性初始化失败");
            return;
        }
        currentFireCount = 0;
    }

    public override void StartTrigger()
    {
        Debug.Log("触发器开始工作");
        if (fireTriggerAttribute == null)
        {
            Debug.LogError("触发器属性未初始化");
            return;
        }
        // 监听开火事件
        BulletSpawner.Instance.fireEvent.AddListener(ReceiveFireEvent);
    }

    public override void StopTrigger()
    {
        BulletSpawner.Instance.fireEvent.RemoveListener(ReceiveFireEvent);
        currentFireCount = 0; // 重置开火次数
        Debug.Log("触发器已停用");
    }
    
    private void ReceiveFireEvent()
    {
        currentFireCount++;
        Debug.Log($"当前开火次数：{currentFireCount}");
        if (currentFireCount >= fireTriggerAttribute.fireCount)
        {
            TriggerItems();
            currentFireCount = 0; // 重置开火次数
        }
    }
}