using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
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
        public int bulletCount;
        public Bullet.BulletType bulletType;
    }

    [Header("道具类型")]
    [Tooltip("道具类型")] public ItemType itemType;
    public bool triggerDectectFlag = false; // 可否被触发器检测标志位

    public abstract void UseItem();
    public ItemType GetItemType() => itemType;
    public abstract object GetAttribute();

    protected void OnEnable() => triggerDectectFlag = true;
}
