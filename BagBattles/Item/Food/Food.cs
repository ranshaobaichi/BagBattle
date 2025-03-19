
using System;
using UnityEngine;

public class Food
{
    [Serializable]
    public enum FoodBonusType
    {
        None,
        [Tooltip("按值增加伤害")] AttackByValue,
        [Tooltip("按百分比增加伤害")] AttackByPercent,
        [Tooltip("速度增加（默认按百分比）")] Speed,
        [Tooltip("增加生命值")] Health,
        // Add other food bonus types here
    }
    [Serializable]
    [Tooltip("食物持续时间类型")]
    public enum FoodDurationType
    {
        None,
        Permanent,
        Temporary,
        // Add other food duration types here
    }
    [Serializable]
    [Tooltip("加成结构体")] public struct Bonus
    {
        [Tooltip("剩余回合数")] public int roundLeft; // 剩余回合数
        [Tooltip("加成值")] public float bonusValue; // 加成值
        public Bonus(float bonusValue, int roundLeft = 1)
        {
            this.roundLeft = roundLeft;
            this.bonusValue = bonusValue;
        }
        public void DecreaseRound() => roundLeft--;
    }
}