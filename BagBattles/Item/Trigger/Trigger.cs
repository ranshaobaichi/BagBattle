using System;
using System.ComponentModel;
using UnityEngine;
using Assets.BagBattles.Types;

[Serializable]
public class Trigger
{
    [Serializable]
    public enum TriggerRange
    {
        /// <summary>
        /// 错误
        /// </summary>
        None,
        /// <summary>
        /// 单格
        /// </summary>
        [Tooltip("单格")] SingleCell,
        /// <summary>
        /// 双格
        /// </summary>
        [Tooltip("双格")] DoubleCell,
        /// <summary>
        /// 三格
        /// </summary>
        [Tooltip("三格")] TripleCell,
        /// <summary>
        /// 整行
        /// </summary>
        [Tooltip("整行")] FullRow,
        /// <summary>
        /// 四直线单格
        /// </summary>
        [Tooltip("四直线单格")] FourStraightSingleCell,

        // [Tooltip("四斜线单格")] FourBiasSingleCell,

        /// <summary>
        /// 九宫格
        /// </summary>
        [Tooltip("九宫格")] NineGrid,
        /// <summary>
        /// 十字
        /// </summary>
        [Tooltip("十字")] Cross,
    }

    [Serializable]
    /// <summary>
    /// 触发器功能类型
    /// </summary>
    public enum TriggerType
    {
        [Tooltip("按时间触发")] ByTime,
        [Tooltip("按开火次数触发")] ByFireTimes,
    }

    // 基础抽象类
    [Serializable]
    public abstract class BaseTriggerAttribute
    {
        [Tooltip("触发范围")] public TriggerRange triggerRange;
        readonly public TriggerType triggerType;
        protected BaseTriggerAttribute(TriggerType type)
        {
            triggerType = type;
            triggerRange = TriggerRange.None;
        }
    }

    // 时间触发属性类
    [Serializable]
    public class TimeTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发间隔(秒)")]
        public float triggerTime = 1.0f;
        public TimeTriggerAttribute() : base(TriggerType.ByTime) { }
    }

    // 开火次数触发属性类
    [Serializable]
    public class FireCountTriggerAttribute : BaseTriggerAttribute
    {
        [Tooltip("触发所需的开火次数")]
        public int fireCount = 0;
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
