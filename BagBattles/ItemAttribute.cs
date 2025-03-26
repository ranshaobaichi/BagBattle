using UnityEngine;
using Assets.BagBattles.Types;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using UnityEditor.EditorTools;

// 物品组织三层结构
// -- 第一层为实际物品类型，如FoodItem、TriggerItem等
// -- 第二层按物品功能分类，如Trigger下的FireTrigger、TimeTrigger等
// -- 第三层为该属性下物品的具体种类，如FireTriggerType下的详细分类等

/// <summary>
/// 触发器配置基础接口
/// </summary>
public interface ITriggerAttributeConfig
{
    Trigger.TriggerRange GetTriggerRange();
    InventoryItem.ItemShape GetShape();
    InventoryItem.Direction GetDirection();
    string GetName();
}

/// <summary>
/// 触发器配置
/// </summary>
[Serializable]
public class TriggerAttribute
{
    #region 类型配置声明
    [Serializable]
    public struct FireCountTriggerAttribute : ITriggerAttributeConfig
    {
        [HideInInspector] public FireTriggerType fireTriggerType;
        [Tooltip("道具名称")] public string fireTriggerName;
        [Header("道具形状")] public InventoryItem.ItemShape fireTriggerShape;
        [Header("道具方向")] public InventoryItem.Direction fireTriggerDirection;
        [Tooltip("开火次数触发器属性")] public Trigger.FireCountTriggerAttribute fireCountTriggerAttribute;

        public Trigger.TriggerRange GetTriggerRange() => fireCountTriggerAttribute.triggerRange;
        public InventoryItem.ItemShape GetShape() => fireTriggerShape;
        public InventoryItem.Direction GetDirection() => fireTriggerDirection;
        public string GetName() => fireTriggerName;
    }

    [Serializable]
    public struct TimeTriggerAttribute : ITriggerAttributeConfig
    {
        [HideInInspector] public TimeTriggerType timeTriggerType;
        [Tooltip("道具名称")] public string timeTriggerName;
        [Header("道具形状")] public InventoryItem.ItemShape timeTriggerShape;
        [Header("道具方向")] public InventoryItem.Direction timeTriggerDirection;
        [Tooltip("时间触发器属性")] public Trigger.TimeTriggerAttribute timeTriggerAttribute;

        public Trigger.TriggerRange GetTriggerRange() => timeTriggerAttribute.triggerRange;
        public InventoryItem.ItemShape GetShape() => timeTriggerShape;
        public InventoryItem.Direction GetDirection() => timeTriggerDirection;
        public string GetName() => timeTriggerName;
    }
    #endregion

    #region 实例化配置区域
    [SerializeField] public List<FireCountTriggerAttribute> fireTriggerAttributes = new List<FireCountTriggerAttribute>();
    [SerializeField] public List<TimeTriggerAttribute> timeTriggerAttributes = new List<TimeTriggerAttribute>();
    #endregion

    #region 函数及接口
    public TriggerAttribute()
    {
        // 初始化开火次数触发器属性
        foreach (var type in Enum.GetValues(typeof(FireTriggerType)))
        {
            fireTriggerAttributes.Add(new FireCountTriggerAttribute
            {
                fireTriggerType = (FireTriggerType)type,
                fireTriggerName = ((FireTriggerType)type).ToString(),
                fireTriggerShape = InventoryItem.ItemShape.NONE,
                fireTriggerDirection = InventoryItem.Direction.UP,
                fireCountTriggerAttribute = new Trigger.FireCountTriggerAttribute()
                {
                    fireCount = 1,
                    triggerRange = Trigger.TriggerRange.None
                }
            });
        }

        // 初始化时间触发器属性
        foreach (var type in Enum.GetValues(typeof(TimeTriggerType)))
        {
            timeTriggerAttributes.Add(new TimeTriggerAttribute
            {
                timeTriggerType = (TimeTriggerType)type,
                timeTriggerName = ((TimeTriggerType)type).ToString(),
                timeTriggerShape = InventoryItem.ItemShape.NONE,
                timeTriggerDirection = InventoryItem.Direction.UP,
                timeTriggerAttribute = new Trigger.TimeTriggerAttribute()
                {
                    triggerRange = Trigger.TriggerRange.None
                }
            });
        }
    }

    /// <summary>
    /// 尝试获取火力触发器属性
    /// </summary>
    private bool TryGetFireTriggerAttribute(object triggerTypeObj, out FireCountTriggerAttribute attribute)
    {
        attribute = default;
        if (triggerTypeObj is FireTriggerType fireTriggerType && 
            (int)fireTriggerType >= 0 && 
            (int)fireTriggerType < fireTriggerAttributes.Count)
        {
            attribute = fireTriggerAttributes[(int)fireTriggerType];
            return true;
        }
        return false;
    }

    /// <summary>
    /// 尝试获取时间触发器属性
    /// </summary>
    private bool TryGetTimeTriggerAttribute(object triggerTypeObj, out TimeTriggerAttribute attribute)
    {
        attribute = default;
        if (triggerTypeObj is TimeTriggerType timeTriggerType && 
            (int)timeTriggerType >= 0 && 
            (int)timeTriggerType < timeTriggerAttributes.Count)
        {
            attribute = timeTriggerAttributes[(int)timeTriggerType];
            return true;
        }
        return false;
    }

    public Trigger.TriggerRange GetTriggerRange(Trigger.TriggerType triggerType, object trigger)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (TryGetFireTriggerAttribute(trigger, out var fireAttr))
                {
                    return fireAttr.GetTriggerRange();
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (TryGetTimeTriggerAttribute(trigger, out var timeAttr))
                {
                    return timeAttr.GetTriggerRange();
                }
                break;
        }
        
        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器范围");
        return Trigger.TriggerRange.None;
    }

    public object GetAttributeValue(Trigger.TriggerType triggerType, object specificType)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (TryGetFireTriggerAttribute(specificType, out var fireAttr))
                {
                    return fireAttr.fireCountTriggerAttribute;
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (TryGetTimeTriggerAttribute(specificType, out var timeAttr))
                {
                    return timeAttr.timeTriggerAttribute;
                }
                break;
        }
        
        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器属性");
        return null;
    }

    public InventoryItem.ItemShape GetTriggerShape(Trigger.TriggerType triggerType, object specificType)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (TryGetFireTriggerAttribute(specificType, out var fireAttr))
                {
                    return fireAttr.GetShape();
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (TryGetTimeTriggerAttribute(specificType, out var timeAttr))
                {
                    return timeAttr.GetShape();
                }
                break;
        }
        
        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器形状");
        return InventoryItem.ItemShape.NONE;
    }

    public InventoryItem.Direction GetTriggerDirection(Trigger.TriggerType triggerType, object specificType)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (TryGetFireTriggerAttribute(specificType, out var fireAttr))
                {
                    return fireAttr.GetDirection();
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (TryGetTimeTriggerAttribute(specificType, out var timeAttr))
                {
                    return timeAttr.GetDirection();
                }
                break;
        }
        
        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器方向");
        return InventoryItem.Direction.NONE;
    }
    #endregion
}

[Serializable]
public class BulletAttribute
{
    [Serializable]
    public struct _bulletAttribute
    {
        [HideInInspector] public BulletType bulletItemType;
        [Header("子弹道具类型")] [SerializeField] public string bulletName;
        [Header("子弹道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("子弹方向")] public InventoryItem.Direction itemDirection;
        [Header("子弹配置")] public Item.BulletItemAttribute bulletItemAttribute;
    }
    [SerializeField] List<_bulletAttribute> bulletAttributes = new List<_bulletAttribute>();
    public BulletAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(BulletType)))
        {
            bulletAttributes.Add(new _bulletAttribute
            {
                bulletItemType = (BulletType)type,
                bulletName = ((BulletType)type).ToString(),
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                bulletItemAttribute = new Item.BulletItemAttribute()
                {
                    bulletType = Bullet.BulletType.None,
                    bulletCount = 1
                }
            });
        }
    }

    public InventoryItem.ItemShape GetBulletShape(BulletType bulletType)
    {
        if (bulletType == BulletType.None || bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹形状");
            return InventoryItem.ItemShape.NONE;
        }
        return bulletAttributes[(int)bulletType].itemShape;
    }
    public object GetBulletAttribute(BulletType bulletType)
    {
        if (bulletType == BulletType.None || bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹配置");
            return null;
        }
        return bulletAttributes[(int)bulletType].bulletItemAttribute;
    }
    public InventoryItem.Direction GetBulletDirection(BulletType bulletType)
    {
        if (bulletType == BulletType.None || bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹方向");
            return InventoryItem.Direction.NONE;
        }
        return bulletAttributes[(int)bulletType].itemDirection;
    }
}

[Serializable]
public class FoodAttribute
{
    [Serializable]
    public struct _foodAttribute
    {
        [HideInInspector] public FoodType foodItemType;
        [Header("子弹道具类型")] [SerializeField] public string foodName;
        [Header("子弹道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("子弹方向")] public InventoryItem.Direction itemDirection;
        [Header("子弹配置")] public Item.FoodItemAttribute foodItemAttribute;
    }
    [SerializeField] List<_foodAttribute> foodAttributes = new List<_foodAttribute>();
    public FoodAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(FoodType)))
        {
            foodAttributes.Add(new _foodAttribute
            {
                foodItemType = (FoodType)type,
                foodName = ((FoodType)type).ToString(),
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                foodItemAttribute = new Item.FoodItemAttribute()
                {
                    foodItemAttributes = new List<Item.FoodItemAttribute.BasicFoodAttribute>(),
                }
            });
        }
    }

    public InventoryItem.ItemShape GetFoodShape(FoodType foodType)
    {
        if (foodType == FoodType.None || foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"子弹类型{foodType}错误,无法获取子弹形状");
            return InventoryItem.ItemShape.NONE;
        }
        return foodAttributes[(int)foodType].itemShape;
    }
    public object GetFoodAttribute(FoodType foodType)
    {
        if (foodType == FoodType.None || foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"食物类型{foodType}错误,无法获取食物配置");
            return null;
        }
        return foodAttributes[(int)foodType].foodItemAttribute;
    }
    public InventoryItem.Direction GetFoodDirection(FoodType foodType)
    {
        if (foodType == FoodType.None || foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"子弹类型{foodType}错误,无法获取子弹方向");
            return InventoryItem.Direction.NONE;
        }
        return foodAttributes[(int)foodType].itemDirection;
    }
}

[CreateAssetMenu(fileName = "ItemAttribute", menuName = "ScriptableObjects/ItemAttribute", order = 1)]
public class ItemAttribute : ScriptableObject
{
    // 添加单例实例
    private static ItemAttribute _instance;
    public static ItemAttribute Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemAttribute>("ItemAttributes/ItemAttributeConfig");

                if (_instance == null)
                {
                    Debug.LogError("找不到ItemAttribute配置，请确保在Resources/ItemAttributes文件夹下创建了名为ItemAttributeConfig的资源");
                }
            }
            return _instance;
        }
    }

    [Header("触发器配置")] public TriggerAttribute triggerAttribute = new();
    [Header("子弹配置")] public BulletAttribute bulletAttribute = new();
    [Header("食物配置")] public FoodAttribute foodAttribute = new();

    #region 触发器相关接口
    public Trigger.TriggerRange GetTriggerRange(Trigger.TriggerType triggerType, object trigger) => triggerAttribute.GetTriggerRange(triggerType, trigger);
    /// <summary>
    /// 获取触发器形状
    /// </summary>
    /// <param name="itemType">道具所属大类</param>
    /// <param name="functionType">功能类型</param>
    /// <param name="specificType">具体类型</param>
    /// <returns>具体形状</returns>
    public InventoryItem.ItemShape GetItemShape(Item.ItemType itemType, object functionType, object specificType)
    {
        if (functionType is Trigger.TriggerType triggerType)
        {
            return triggerAttribute.GetTriggerShape(triggerType, specificType);
        }
        else
        {
            Debug.LogError($"非触发器类型{itemType}错误调用触发器获取形状接口");
            return InventoryItem.ItemShape.NONE;
        }
    }
    /// <summary>
    /// 获取触发器具体配置
    /// </summary>
    /// <param name="itemType">道具所属大类</param>
    /// <param name="functionType">功能类型</param>
    /// <param name="specificType">具体类型</param>
    /// <returns> 具体配置 </returns>
    public object GetAttribute(Item.ItemType itemType, object functionType, object specificType)
    {
        if (itemType != Item.ItemType.TriggerItem)
        {
            Debug.LogError($"{itemType}错误调用触发器类型获取配置");
            return null;
        }

        switch (functionType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (specificType is FireTriggerType fireTriggerType)
                {
                    return triggerAttribute.GetAttributeValue(Trigger.TriggerType.ByFireTimes, fireTriggerType);
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (specificType is TimeTriggerType timeTriggerType)
                {
                    return triggerAttribute.GetAttributeValue(Trigger.TriggerType.ByTime, timeTriggerType);
                }
                break;
            default:
                Debug.LogError($"触发器类型下未实现的功能类型: {(Trigger.TriggerType)functionType}");
                return null;
        }
        Debug.LogError($"触发器功能类型{(Trigger.TriggerType)functionType}下的具体类型{(FireTriggerType)specificType}错误或未实现,无法获取触发器属性");
        return null;
    }
    public InventoryItem.Direction GetItemDirection(Item.ItemType itemType, object functionType, object specificType)
    {
        if (itemType != Item.ItemType.TriggerItem)
        {
            Debug.LogError($"{itemType}错误调用触发器类型获取方向");
            return InventoryItem.Direction.NONE;
        }
        if (functionType is Trigger.TriggerType triggerType)
        {
            return triggerAttribute.GetTriggerDirection(triggerType, specificType);
        }
        else
        {
            Debug.LogError($"非触发器类型{itemType}错误调用触发器获取方向接口");
            return InventoryItem.Direction.NONE;
        }
    }

    #endregion

    #region 普通接口
    public InventoryItem.ItemShape GetItemShape(Item.ItemType itemType, object specificType)
    {
        if (itemType == Item.ItemType.TriggerItem)
        {
            Debug.LogError("触发器类型错误调用普通接口");
            return InventoryItem.ItemShape.NONE;
        }
        switch (itemType)
        {
            case Item.ItemType.FoodItem:
                if(specificType is FoodType foodType && foodType != FoodType.None)
                {
                    return foodAttribute.GetFoodShape(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物形状");
                    break;
                }
            case Item.ItemType.BulletItem:
                if (specificType is BulletType bulletType && bulletType != BulletType.None)
                {
                    return bulletAttribute.GetBulletShape(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹形状");
                    break;
                }
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取形状属性");
                break;
        }

        return InventoryItem.ItemShape.NONE;
    }
    public object GetAttribute(Item.ItemType itemType, object specificType)
    {
        if (itemType == Item.ItemType.TriggerItem)
        {
            Debug.LogError("触发器类型获取配置错误");
            return null;
        }

        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                if (specificType is BulletType bulletType && bulletType != BulletType.None)
                {
                    return bulletAttribute.GetBulletAttribute(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误或未实现,无法获取子弹属性");
                    return null;
                }
            case Item.ItemType.FoodItem:
                if (specificType is FoodType foodType && foodType != FoodType.None)
                {
                    return foodAttribute.GetFoodAttribute(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误或未实现,无法获取食物属性");
                    return null;
                }
            default:
                Debug.LogError($"未实现的物品类型: {itemType}");
                return null;
        }
    }
    public InventoryItem.Direction GetItemDirection(Item.ItemType itemType, object specificType)
    {
        if (itemType == Item.ItemType.TriggerItem)
        {
            Debug.LogError("触发器类型获取方向错误");
            return InventoryItem.Direction.NONE;
        }

        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                if (specificType is BulletType bulletType && bulletType != BulletType.None)
                {
                    return bulletAttribute.GetBulletDirection(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹方向");
                    break;
                }
            case Item.ItemType.FoodItem:
                if (specificType is FoodType foodType && foodType != FoodType.None)
                {
                    return foodAttribute.GetFoodDirection(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物方向");
                    break;
                }
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取方向属性");
                break;
        }

        return InventoryItem.Direction.NONE;
    }
    #endregion
}