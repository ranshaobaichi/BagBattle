using System;
using System.ComponentModel;
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
        readonly public TriggerType triggerType;
        protected BaseTriggerAttribute(TriggerType type)
        {
            triggerType = type;
        }
    }

    // 时间触发属性类
    [Serializable]
    public class TimeTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发间隔(秒)")]
        public float triggerTime = 1.0f;
        public TimeTriggerAttribute() : base(TriggerType.ByTime){ }
    }

    // 开火次数触发属性类
    [Serializable]
    public class FireCountTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发所需的开火次数")]
        public int triggerFireCount = 3;
        public FireCountTriggerAttribute() : base(TriggerType.ByFireTimes) { }
    }
    
    // // 兼容旧代码的属性 - 这个可以后面逐步移除
    // [Serializable]
    // public class TriggerItemAttribute : BaseTriggerAttribute
    // {
    //     // 按时间触发的配置
    //     public float triggerTime = 1.0f;
    //     // 按开火次数触发的配置
    //     public int triggerFireCount = 3;
    // }
}
