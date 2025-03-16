using System;
using UnityEngine;

public class Trigger
{
    [Serializable]
    public enum TriggerRange
    {
        [Tooltip("单格")] SingleCell,
        [Tooltip("双格")] DoubleCell,
    }

    [Serializable]
    public enum TriggerType
    {
        [Tooltip("按时间触发")] ByTime,
        [Tooltip("按开火次数触发")] ByFireTimes,
    }

    // 基础抽象类
    [Serializable]
    public abstract class BaseTriggerAttribute
    {
        public TriggerRange triggerRange;
        public TriggerType triggerType;
    }

    // 时间触发属性类
    [Serializable]
    public class TimeTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发间隔(秒)")]
        public float triggerTime = 1.0f;
        
        [Tooltip("持续时间(秒)，0表示永久")]
        public float duration = 0f;
        
        public TimeTriggerAttribute()
        {
            triggerType = TriggerType.ByTime;
        }
    }

    // 开火次数触发属性类
    [Serializable]
    public class FireCountTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发所需的开火次数")]
        public int triggerFireCount = 3;
        
        [Tooltip("触发后是否重置计数")]
        public bool resetAfterTrigger = true;
        
        public FireCountTriggerAttribute()
        {
            triggerType = TriggerType.ByFireTimes;
        }
    }
    
    // 兼容旧代码的属性 - 这个可以后面逐步移除
    [Serializable]
    public class TriggerItemAttribute : BaseTriggerAttribute
    {
        // 按时间触发的配置
        public float triggerTime = 1.0f;
        // 按开火次数触发的配置
        public int triggerFireCount = 3;
    }
}
