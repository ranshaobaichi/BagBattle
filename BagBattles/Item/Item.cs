using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    [Serializable]
    public enum ItemType
    {
        None,
        TriggerItem,
        BulletItem,
        FoodItem,
        // Add other item types here
    }
    [Serializable]
    public struct BulletItemAttribute
    {
        [Tooltip("子弹类型")] public Bullet.BulletType bulletType;
        [Tooltip("子弹发射数量")] public int bulletCount;
    }
    [Serializable]
    public struct FoodItemAttribute
    {
        [Serializable]
        public struct BasicFoodAttribute
        {
            [Tooltip("食物加成种类")] public Food.FoodBonusType foodBonusType;
            [Tooltip("食物加成数值")] public float foodBonusValue;
            [Tooltip("食物持续时间类型")] public Food.FoodDurationType foodDurationType;
            [Tooltip("食物加成持续时间（回合数）")] public int roundLeft;
        }
        [Tooltip("食物道具名称")] public string foodName;
        [Tooltip("食物效果配置")] public List<BasicFoodAttribute> foodItemAttributes;
        public FoodInventoryItem foodInventoryItem;
    }
    public abstract void UseItem(); // 使用道具
}
