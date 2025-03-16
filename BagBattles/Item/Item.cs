using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [Serializable]
    public enum ItemType
    {
        None,
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
    public abstract void UseItem();
    public ItemType GetItemType() => itemType;
    public abstract object GetItemAttribute();
}
