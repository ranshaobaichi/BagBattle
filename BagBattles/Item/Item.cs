using System;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [Serializable]
    public enum ItemType
    {
        None,
        BulletItem,
        TriggerItem,
        FoodItem,
        // Add other item types here
    }
    [Header("道具类型")]
    [Tooltip("道具类型")] public ItemType itemType;
    public abstract void UseItem();
    public ItemType GetItemType() => itemType;
}
