
using System;
using System.ComponentModel;
using UnityEngine;

public class Food
{
    [Serializable]
    public enum FoodBonusType
    {
        None,
        [Tooltip("按值增加伤害")][Description("按值增加伤害")] AttackDamageByValue,
        [Tooltip("按百分比增加伤害")] AttackDamageByPercent,
        [Tooltip("按百分比增加攻击范围")] AttackRange,
        [Tooltip("按值增加发射速度")][Description("按值增加发射速度")] AttackSpeed,
        [Tooltip("按值增加装载速度")][Description("按值增加装载速度")] LoadSpeed,
        [Tooltip("按百分比增加速度")] Speed,
        [Tooltip("增加血量颗数")] HealthUp,
        [Tooltip("回复血量")] HealthRecover,
        [Tooltip("降低血量颗数")] HealthDown,
        [Tooltip("增加护甲值")] ArmorUp,
        // Add other food bonus types here
    }
    [Serializable]
    [Tooltip("食物持续时间类型")]
    public enum FoodDurationType
    {
        None,
        [Tooltip("永久加成")] Permanent,
        [Tooltip("临时回合加成")] TemporaryRounds,
        [Tooltip("临时时间加成")] TemporaryTime,
        // Add other food duration types here
    }
    [Serializable]
    [Tooltip("加成结构体")] public struct Bonus
    {
        [Tooltip("剩余回合数")] public float timeLeft; // 剩余回合数
        [Tooltip("加成值")] public float bonusValue; // 加成值
        public Bonus(float bonusValue, float timeLeft = 1f)
        {
            this.timeLeft = timeLeft;
            this.bonusValue = bonusValue;
        }
        public void DecreaseRound() => timeLeft -= 1f;
    }
}