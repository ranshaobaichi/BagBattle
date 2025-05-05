using UnityEngine;
using Assets.BagBattles.Types;
using System.Collections.Generic;
using System;
using System.IO;

#region 物体配置结构声明
[Serializable]
public struct BulletItemAttribute
{
    [Header("子弹道具类型")][Assets.Editor.ItemAttributeDrawer.ReadOnly] public BulletType specificBulletType;
    [Header("装载子弹类型")] public Bullet.SingleBulletType bulletType;
    [Header("道具描述")] public string description;
    [Header("子弹装载数量")] public int bulletCount;
    [Header("掉落权重")] [Range(0, 9)] public int dropWeight;
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
        [Tooltip("食物加成持续时间（回合数）")] public float timeLeft;
    }
    [Header("食物类型")][Assets.Editor.ItemAttributeDrawer.ReadOnly] public FoodType specificFoodType;
    [Header("食物触发几次后销毁(-1即为不销毁)")] public int destroyCount;
    [Header("道具描述")] public string description;
    [Header("食物效果配置")] public List<BasicFoodAttribute> foodItemAttributes;
    [Header("掉落权重")] [Range(0, 9)] public int dropWeight;
}

[Serializable]
public struct SurroundItemAttribute
{
    [Header("环绕物类型")][Assets.Editor.ItemAttributeDrawer.ReadOnly] public SurroundType specificSurroundType;
    [Header("道具描述")] public string description;
    [Header("环绕物属性")]
    [Tooltip("召唤的环绕物类型")] public Surrounding.SingleSurroundingType summonedSurroundingType;
    [Tooltip("一次产生的环绕物数量")] public int surroundingCount;
    [Tooltip("环绕物加速持续时间")] public float surroundingDuration;
    [Tooltip("再次触发时的加速百分比")] public float surroundingSpeedPercent;
    [Header("掉落权重")] [Range(0, 9)] public int dropWeight;
    [HideInInspector] public GameObject surroundingPrefab;
}

[Serializable]
public struct OtherItemAttribute
{
    [Header("道具描述")] public string description;
    [Header("其他物品类型")][Assets.Editor.ItemAttributeDrawer.ReadOnly] public OtherType specificOtherType;
    [HideInInspector] public GameObject otherItemPrefab;
    [Header("掉落权重")] [Range(0, 9)] public int dropWeight;
}
#endregion

// 物品组织三层结构
// -- 第一层为实际物品类型，如FoodItem、TriggerItem等
// -- 第二层按物品功能分类，如Trigger下的FireTrigger、TimeTrigger等
// -- 第三层为该属性下物品的具体种类，如FireTriggerType下的详细分类等

public interface IItemAttributeConfig
{
    InventoryItem.ItemShape GetShape();
    InventoryItem.Direction GetDirection();
    string GetName();
    string GetDescription();
}
/// <summary>
/// 触发器配置基础接口
/// </summary>
public interface ITriggerAttributeConfig : IItemAttributeConfig
{
    Trigger.TriggerRange GetTriggerRange();
}

#region 不同道具类型attribute组织
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
        [Tooltip("开火次数触发器属性")] public Trigger.FireCountTriggerAttribute fireCountTriggerAttribute;
        [Header("道具形状")] public InventoryItem.ItemShape fireTriggerShape;
        [Header("道具方向")] public InventoryItem.Direction fireTriggerDirection;

        public Trigger.TriggerRange GetTriggerRange() => fireCountTriggerAttribute.triggerRange;
        public InventoryItem.ItemShape GetShape() => fireTriggerShape;
        public InventoryItem.Direction GetDirection() => fireTriggerDirection;
        public string GetName() => fireCountTriggerAttribute.fireTriggerType.ToString();
        public string GetDescription() => fireCountTriggerAttribute.description;
        // 初始化方法，设置特定触发器类型
        public static FireCountTriggerAttribute Create(FireTriggerType type)
        {
            return new FireCountTriggerAttribute
            {
                fireTriggerShape = InventoryItem.ItemShape.NONE,
                fireTriggerDirection = InventoryItem.Direction.UP,
                fireCountTriggerAttribute = new Trigger.FireCountTriggerAttribute()
                {
                    fireCount = 1,
                    triggerRange = Trigger.TriggerRange.None,
                    fireTriggerType = type
                }
            };
        }
    }

    [Serializable]
    public struct TimeTriggerAttribute : ITriggerAttributeConfig
    {
        [Tooltip("时间触发器属性")] public Trigger.TimeTriggerAttribute timeTriggerAttribute;
        [Header("道具形状")] public InventoryItem.ItemShape timeTriggerShape;
        [Header("道具方向")] public InventoryItem.Direction timeTriggerDirection;

        public Trigger.TriggerRange GetTriggerRange() => timeTriggerAttribute.triggerRange;
        public InventoryItem.ItemShape GetShape() => timeTriggerShape;
        public InventoryItem.Direction GetDirection() => timeTriggerDirection;
        public string GetName() => timeTriggerAttribute.timeTriggerType.ToString();
        public string GetDescription() => timeTriggerAttribute.description;

        // 初始化方法，设置特定触发器类型
        public static TimeTriggerAttribute Create(TimeTriggerType type)
        {
            return new TimeTriggerAttribute
            {
                timeTriggerShape = InventoryItem.ItemShape.NONE,
                timeTriggerDirection = InventoryItem.Direction.UP,
                timeTriggerAttribute = new Trigger.TimeTriggerAttribute()
                {
                    triggerRange = Trigger.TriggerRange.None,
                    timeTriggerType = type
                }
            };
        }
    }

    [Serializable]
    public struct ByOtherTriggerAttribute : ITriggerAttributeConfig
    {
        [Tooltip("开火次数触发器属性")] public Trigger.ByOtherTriggerAttribute byOtherTriggerAttribute;
        [Header("道具形状")] public InventoryItem.ItemShape byOtherTriggerShape;
        [Header("道具方向")] public InventoryItem.Direction byOtherTriggerDirection;

        public Trigger.TriggerRange GetTriggerRange() => byOtherTriggerAttribute.triggerRange;
        public InventoryItem.ItemShape GetShape() => byOtherTriggerShape;
        public InventoryItem.Direction GetDirection() => byOtherTriggerDirection;
        public string GetName() => byOtherTriggerAttribute.byOtherTriggerType.ToString();
        public string GetDescription() => byOtherTriggerAttribute.description;
        // 初始化方法，设置特定触发器类型
        public static ByOtherTriggerAttribute Create(ByOtherTriggerType type)
        {
            return new ByOtherTriggerAttribute
            {
                byOtherTriggerShape = InventoryItem.ItemShape.NONE,
                byOtherTriggerDirection = InventoryItem.Direction.UP,
                byOtherTriggerAttribute = new Trigger.ByOtherTriggerAttribute()
                {
                    requiredTriggerCount = 1,
                    triggerRange = Trigger.TriggerRange.None,
                    byOtherTriggerType = type
                }
            };
        }
    }

    #endregion

    #region 实例化配置区域
    [SerializeField][Header("开火触发器")] public List<FireCountTriggerAttribute> fireTriggerAttributes = new List<FireCountTriggerAttribute>();
    [SerializeField][Header("时间触发器")] public List<TimeTriggerAttribute> timeTriggerAttributes = new List<TimeTriggerAttribute>();
    [SerializeField][Header("被其他触发器触发触发器")] public List<ByOtherTriggerAttribute> byOtherTriggerAttributes = new List<ByOtherTriggerAttribute>();
    #endregion

    #region 函数及接口

    public TriggerAttribute()
    {
        // 初始化开火次数触发器属性
        foreach (var type in Enum.GetValues(typeof(FireTriggerType)))
        {
            fireTriggerAttributes.Add(FireCountTriggerAttribute.Create((FireTriggerType)type));
        }

        // 初始化时间触发器属性
        foreach (var type in Enum.GetValues(typeof(TimeTriggerType)))
        {
            timeTriggerAttributes.Add(TimeTriggerAttribute.Create((TimeTriggerType)type));
        }

        // 初始化被其他触发器触发触发器属性
        foreach (var type in Enum.GetValues(typeof(ByOtherTriggerType)))
        {
            byOtherTriggerAttributes.Add(ByOtherTriggerAttribute.Create((ByOtherTriggerType)type));
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

    /// <summary>
    /// 尝试获取被其他触发器触发触发器属性
    /// </summary>
    private bool TryGetByOtherTriggerAttribute(object triggerTypeObj, out ByOtherTriggerAttribute attribute)
    {
        attribute = default;
        if (triggerTypeObj is ByOtherTriggerType byOtherTriggerType &&
            (int)byOtherTriggerType >= 0 &&
            (int)byOtherTriggerType < byOtherTriggerAttributes.Count)
        {
            attribute = byOtherTriggerAttributes[(int)byOtherTriggerType];
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
            case Trigger.TriggerType.ByOtherTrigger:
                if (TryGetByOtherTriggerAttribute(trigger, out var byOtherAttr))
                {
                    return byOtherAttr.GetTriggerRange();
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
            case Trigger.TriggerType.ByOtherTrigger:
                if (TryGetByOtherTriggerAttribute(specificType, out var byOtherAttr))
                {
                    return byOtherAttr.byOtherTriggerAttribute;
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
            case Trigger.TriggerType.ByOtherTrigger:
                if (TryGetByOtherTriggerAttribute(specificType, out var byOtherAttr))
                {
                    return byOtherAttr.GetShape();
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
            case Trigger.TriggerType.ByOtherTrigger:
                if (TryGetByOtherTriggerAttribute(specificType, out var byOtherAttr))
                {
                    return byOtherAttr.GetDirection();
                }
                break;
        }

        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器方向");
        return InventoryItem.Direction.NONE;
    }

    public string GetDescription(Trigger.TriggerType triggerType, object specificType)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByFireTimes:
                if (TryGetFireTriggerAttribute(specificType, out var fireAttr))
                {
                    return fireAttr.GetDescription();
                }
                break;
            case Trigger.TriggerType.ByTime:
                if (TryGetTimeTriggerAttribute(specificType, out var timeAttr))
                {
                    return timeAttr.GetDescription();
                }
                break;
            case Trigger.TriggerType.ByOtherTrigger:
                if (TryGetByOtherTriggerAttribute(specificType, out var byOtherAttr))
                {
                    return byOtherAttr.GetDescription();
                }
                break;
        }

        Debug.LogError($"触发器类型{triggerType}错误或未实现,无法获取触发器描述");
        return null;
    }
    #endregion
}

/// <summary>
/// 子弹配置
/// </summary>
[Serializable]
public class BulletAttribute
{
    [Serializable]
    public struct _bulletAttribute : IItemAttributeConfig
    {
        [Header("子弹道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("子弹方向")] public InventoryItem.Direction itemDirection;
        [Header("子弹配置")] public BulletItemAttribute bulletItemAttribute;

        // 初始化方法，设置特定子弹类型
        public static _bulletAttribute Create(BulletType type)
        {
            return new _bulletAttribute
            {
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                bulletItemAttribute = new BulletItemAttribute()
                {
                    bulletType = Bullet.SingleBulletType.None,
                    bulletCount = 1,
                    specificBulletType = type,
                }
            };
        }

        public InventoryItem.ItemShape GetShape() => itemShape;
        public InventoryItem.Direction GetDirection() => itemDirection;
        public string GetName() => bulletItemAttribute.specificBulletType.ToString();
        public string GetDescription() => bulletItemAttribute.description;
    }

    [SerializeField] List<_bulletAttribute> bulletAttributes = new List<_bulletAttribute>();

    public BulletAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(BulletType)))
        {
            bulletAttributes.Add(_bulletAttribute.Create((BulletType)type));
        }
    }

    public InventoryItem.ItemShape GetBulletShape(BulletType bulletType)
    {
        if (bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹形状");
            return InventoryItem.ItemShape.NONE;
        }
        return bulletAttributes[(int)bulletType].GetShape();
    }
    public object GetBulletAttribute(BulletType bulletType)
    {
        if (bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹配置");
            return null;
        }
        return bulletAttributes[(int)bulletType].bulletItemAttribute;
    }
    public InventoryItem.Direction GetBulletDirection(BulletType bulletType)
    {
        if (bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹方向");
            return InventoryItem.Direction.NONE;
        }
        return bulletAttributes[(int)bulletType].GetDirection();
    }
    
    public string GetDescription(BulletType bulletType)
    {
        if (bulletAttributes.Contains(bulletAttributes[(int)bulletType]) == false)
        {
            Debug.LogError($"子弹类型{bulletType}错误,无法获取子弹描述");
            return null;
        }
        return bulletAttributes[(int)bulletType].GetDescription();
    }
}

[Serializable]
public class FoodAttribute
{
    private static readonly int FOOD_TYPE_COUNT = Enum.GetValues(typeof(FoodType)).Length - 1; // 食物类型数量

    [Serializable]
    public struct _foodAttribute : IItemAttributeConfig
    {
        [Header("食物道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("食物方向")] public InventoryItem.Direction itemDirection;
        [Header("食物配置")] public FoodItemAttribute foodItemAttribute;

        // 初始化方法，设置特定食物类型
        public static _foodAttribute Create(FoodType type)
        {
            return new _foodAttribute
            {
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                foodItemAttribute = new FoodItemAttribute()
                {
                    foodItemAttributes = new List<FoodItemAttribute.BasicFoodAttribute>(),
                    specificFoodType = type,
                    destroyCount = 1,
                }
            };
        }

        public InventoryItem.ItemShape GetShape() => itemShape;
        public InventoryItem.Direction GetDirection() => itemDirection;
        public string GetName() => foodItemAttribute.specificFoodType.ToString();
        public string GetDescription() => foodItemAttribute.description;
    }

    [SerializeField] List<_foodAttribute> foodAttributes = new List<_foodAttribute>();

    public FoodAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(FoodType)))
        {
            foodAttributes.Add(_foodAttribute.Create((FoodType)type));
        }
    }

    public InventoryItem.ItemShape GetFoodShape(FoodType foodType)
    {
        if (foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"食物类型{foodType}错误,无法获取食物形状");
            return InventoryItem.ItemShape.NONE;
        }
        return foodAttributes[(int)foodType].itemShape;
    }
    public object GetFoodAttribute(FoodType foodType)
    {
        if (foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"食物类型{foodType}错误,无法获取食物配置");
            return null;
        }
        return foodAttributes[(int)foodType].foodItemAttribute;
    }
    public InventoryItem.Direction GetFoodDirection(FoodType foodType)
    {
        if (foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"食物类型{foodType}错误,无法获取食物方向");
            return InventoryItem.Direction.NONE;
        }
        return foodAttributes[(int)foodType].itemDirection;
    }
    
    public string GetDescription(FoodType foodType)
    {
        if (foodAttributes.Contains(foodAttributes[(int)foodType]) == false)
        {
            Debug.LogError($"食物类型{foodType}错误,无法获取食物描述");
            return null;
        }
        return foodAttributes[(int)foodType].foodItemAttribute.description;
    }
}

[Serializable]
public class SurroundAttribute
{
    private readonly string SURROUNDING_PREFAB_PATH = "Surroundings/";

    [Serializable]
    public struct _surroundAttribute : IItemAttributeConfig
    {
        [Header("环绕物道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("环绕物方向")] public InventoryItem.Direction itemDirection;
        [Header("环绕物配置")] public SurroundItemAttribute surroundItemAttribute;

        // 初始化方法，设置特定环绕物类型
        public static _surroundAttribute Create(SurroundType type)
        {
            return new _surroundAttribute
            {
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                surroundItemAttribute = new SurroundItemAttribute()
                {
                    specificSurroundType = type,
                    summonedSurroundingType = Surrounding.SingleSurroundingType.None,
                    surroundingCount = 1,
                    surroundingSpeedPercent = 1.0f,
                    surroundingPrefab = null,
                }
            };
        }

        public InventoryItem.ItemShape GetShape() => itemShape;
        public InventoryItem.Direction GetDirection() => itemDirection;
        public string GetName() => surroundItemAttribute.specificSurroundType.ToString();
        public string GetDescription() => surroundItemAttribute.description;
    }

    [SerializeField] List<_surroundAttribute> surroundAttributes = new List<_surroundAttribute>();
    public Dictionary<Surrounding.SingleSurroundingType, GameObject> surroundingPrefabs = new();

    public SurroundAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(SurroundType)))
        {
            surroundAttributes.Add(_surroundAttribute.Create((SurroundType)type));
        }
    }

    public void LoadSurroundingPrefabs()
    {
        foreach (var type in Enum.GetValues(typeof(Surrounding.SingleSurroundingType)))
        {
            if ((Surrounding.SingleSurroundingType)type == Surrounding.SingleSurroundingType.None) continue; // 跳过None类型
            var prefab = Resources.Load<GameObject>(SURROUNDING_PREFAB_PATH + type.ToString());
            if (prefab != null)
            {
                surroundingPrefabs.Add((Surrounding.SingleSurroundingType)type, prefab);
            }
            else
            {
                Debug.LogError($"环绕物预制体{type}未找到,请检查路径{SURROUNDING_PREFAB_PATH + type.ToString()}");
            }
        }
    }

    public InventoryItem.ItemShape GetSurroundShape(SurroundType surroundType)
    {
        if (surroundAttributes.Contains(surroundAttributes[(int)surroundType]) == false)
        {
            Debug.LogError($"环绕物类型{surroundType}错误,无法获取环绕物形状");
            return InventoryItem.ItemShape.NONE;
        }
        return surroundAttributes[(int)surroundType].GetShape();
    }
    public object GetSurroundAttribute(SurroundType surroundType)
    {
        if (surroundAttributes.Contains(surroundAttributes[(int)surroundType]) == false)
        {
            Debug.LogError($"环绕物类型{surroundType}错误,无法获取环绕物配置");
            return null;
        }
        var attr = surroundAttributes[(int)surroundType].surroundItemAttribute;
        surroundingPrefabs.TryGetValue(attr.summonedSurroundingType, out attr.surroundingPrefab);
        if (attr.surroundingPrefab == null)
        {
            Debug.LogError($"环绕物类型{surroundType}未加载入字典, 无法获取环绕物预制体");
            return null;
        }
        return attr;
    }
    public InventoryItem.Direction GetSurroundDirection(SurroundType surroundType)
    {
        if (surroundAttributes.Contains(surroundAttributes[(int)surroundType]) == false)
        {
            Debug.LogError($"环绕物类型{surroundType}错误,无法获取环绕物方向");
            return InventoryItem.Direction.NONE;
        }
        return surroundAttributes[(int)surroundType].GetDirection();
    }

    public string GetDescription(SurroundType surroundType)
    {
        if (surroundAttributes.Contains(surroundAttributes[(int)surroundType]) == false)
        {
            Debug.LogError($"环绕物类型{surroundType}错误,无法获取环绕物描述");
            return null;
        }
        return surroundAttributes[(int)surroundType].GetDescription();
    }
}

[Serializable]
public class OtherAttribute
{
    private readonly string OTHER_PREFAB_PATH = "Others/";
    [Serializable]
    public struct _otherAttribute : IItemAttributeConfig
    {
        [Header("其他道具形状")] public InventoryItem.ItemShape itemShape;
        [Tooltip("其他方向")] public InventoryItem.Direction itemDirection;
        [Header("其他配置")] public OtherItemAttribute otherItemAttribute;

        // 初始化方法，设置特定其他类型
        public static _otherAttribute Create(OtherType type)
        {
            return new _otherAttribute
            {
                itemShape = InventoryItem.ItemShape.NONE,
                itemDirection = InventoryItem.Direction.UP,
                otherItemAttribute = new OtherItemAttribute()
                {
                    specificOtherType = type,
                }
            };
        }

        public InventoryItem.ItemShape GetShape() => itemShape;
        public InventoryItem.Direction GetDirection() => itemDirection;
        public string GetName() => otherItemAttribute.specificOtherType.ToString();
        public string GetDescription() => otherItemAttribute.description;
    }

    [SerializeField] List<_otherAttribute> otherAttributes = new List<_otherAttribute>();
    public Dictionary<OtherType, GameObject> otherPrefabs = new();

    public OtherAttribute()
    {
        foreach (var type in Enum.GetValues(typeof(OtherType)))
        {
            otherAttributes.Add(_otherAttribute.Create((OtherType)type));
        }
    }

    public void LoadOtherPrefabs()
    {
        foreach (var type in Enum.GetValues(typeof(OtherType)))
        {
            var prefab = Resources.Load<GameObject>(OTHER_PREFAB_PATH + type.ToString());
            if (prefab != null)
            {
                otherPrefabs.Add((OtherType)type, prefab);
            }
            else
            {
                Debug.LogError($"其他类型道具预制体{type}未找到,请检查路径{OTHER_PREFAB_PATH + type.ToString()}");
            }
        }
    }

    public InventoryItem.ItemShape GetOtherShape(OtherType otherType)
    {
        if (otherAttributes.Contains(otherAttributes[(int)otherType]) == false)
        {
            Debug.LogError($"其他类型道具类型{otherType}错误,无法获取其他类型道具形状");
            return InventoryItem.ItemShape.NONE;
        }
        return otherAttributes[(int)otherType].GetShape();
    }
    public object GetOtherAttribute(OtherType otherType)
    {
        var attr = otherAttributes[(int)otherType].otherItemAttribute;
        otherPrefabs.TryGetValue(attr.specificOtherType, out attr.otherItemPrefab);
        if (attr.otherItemPrefab == null)
        {
            Debug.LogError($"其他类型道具类型{otherType}未加载入字典, 无法获取其他类型道具预制体");
            return null;
        }
        return attr;
    }
    public InventoryItem.Direction GetOtherDirection(OtherType otherType)
    {
        if (otherAttributes.Contains(otherAttributes[(int)otherType]) == false)
        {
            Debug.LogError($"其他类型道具类型{otherType}错误,无法获取其他类型道具方向");
            return InventoryItem.Direction.NONE;
        }
        return otherAttributes[(int)otherType].GetDirection();
    }

    public string GetDescription(OtherType otherType)
    {
        if (otherAttributes.Contains(otherAttributes[(int)otherType]) == false)
        {
            Debug.LogError($"其他类型道具类型{otherType}错误,无法获取其他类型道具描述");
            return null;
        }
        return otherAttributes[(int)otherType].GetDescription();
    }
}

#endregion
[Serializable]
public class ItemAttributeData
{
    public TriggerAttribute triggerAttribute;
    public BulletAttribute bulletAttribute;
    public FoodAttribute foodAttribute;
    public SurroundAttribute surroundAttribute;
    public OtherAttribute otherAttribute;
}

[CreateAssetMenu(fileName = "ItemAttribute", menuName = "ScriptableObjects/ItemAttribute", order = 1)]
public class ItemAttribute : ScriptableObject
{
    // 定义JSON文件路径
    private static readonly string JSON_FILE_PATH = "ItemAttributes/ItemAttributeConfig";

    private static ItemAttribute _instance;
    public static ItemAttribute Instance
    {
        get
        {
            if (_instance == null)
            {
                // 首先尝试从JSON加载
                _instance = LoadFromJson();

                if (_instance == null)
                {
                    // 如果JSON加载失败，回退到ScriptableObject
                    _instance = Resources.Load<ItemAttribute>("ItemAttributes/ItemAttributeConfig");

                    if (_instance == null)
                    {
                        Debug.LogError("找不到ItemAttribute配置，请确保在Resources/ItemAttributes文件夹下创建了名为ItemAttributeConfig的资源或JSON文件");
                    }
                }
            }
            return _instance;
        }
    }

    public List<int> typeDropWeights = new List<int>(); // 物品掉落权重

    public TriggerAttribute triggerAttribute = new();
    public BulletAttribute bulletAttribute = new();
    public FoodAttribute foodAttribute = new();
    public SurroundAttribute surroundAttribute = new();
    public OtherAttribute otherAttribute = new();

    // 从JSON加载数据
    private static ItemAttribute LoadFromJson()
    {
        try
        {
            // 从Resources文件夹加载JSON文件
            TextAsset jsonFile = Resources.Load<TextAsset>(JSON_FILE_PATH);

            if (jsonFile != null)
            {
                // 创建一个新实例
                ItemAttribute instance = CreateInstance<ItemAttribute>();

                // 从JSON加载数据
                ItemAttributeData data = JsonUtility.FromJson<ItemAttributeData>(jsonFile.text);

                // 应用数据到实例
                instance.triggerAttribute = data.triggerAttribute;
                instance.bulletAttribute = data.bulletAttribute;
                instance.foodAttribute = data.foodAttribute;
                instance.surroundAttribute = data.surroundAttribute;
                instance.otherAttribute = data.otherAttribute;

                instance.surroundAttribute.LoadSurroundingPrefabs(); // 加载环绕物预制体
                instance.otherAttribute.LoadOtherPrefabs();
                Debug.Log("从JSON文件加载道具属性成功");
                return instance;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"从JSON加载道具属性失败: {ex.Message}");
        }
        return null;
    }

    // 导出当前配置到JSON (编辑器工具)
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/导出道具属性到JSON")]
    public static void ExportToJson()
    {
        var asset = UnityEditor.Selection.activeObject as ItemAttribute;
        if (asset == null)
        {
            asset = Resources.Load<ItemAttribute>("ItemAttributes/ItemAttributeConfig");
        }

        if (asset != null)
        {
            // 创建数据对象
            ItemAttributeData data = new ItemAttributeData
            {
                triggerAttribute = asset.triggerAttribute,
                bulletAttribute = asset.bulletAttribute,
                foodAttribute = asset.foodAttribute,
                surroundAttribute = asset.surroundAttribute,
                otherAttribute = asset.otherAttribute
            };

            // 转换为JSON
            string jsonData = JsonUtility.ToJson(data, true);

            // 创建Resources/ItemAttributes目录
            string directory = Path.Combine(Application.dataPath, "Resources/ItemAttributes");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 保存JSON文件
            string filePath = Path.Combine(directory, "ItemAttributeConfig.json");
            File.WriteAllText(filePath, jsonData);

            Debug.Log($"已导出道具属性到JSON文件: {filePath}");
            UnityEditor.AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("未找到ItemAttribute资源");
        }
    }
#endif

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
            case Trigger.TriggerType.ByOtherTrigger:
                if (specificType is ByOtherTriggerType byOtherTriggerType)
                {
                    return triggerAttribute.GetAttributeValue(Trigger.TriggerType.ByOtherTrigger, byOtherTriggerType);
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
    public string GetDescription(Item.ItemType itemType, object functionType, object specificType)
    {
        if (itemType != Item.ItemType.TriggerItem)
        {
            Debug.LogError($"{itemType}错误调用触发器类型获取描述");
            return null;
        }
        if (functionType is Trigger.TriggerType triggerType)
        {
            return triggerAttribute.GetDescription(triggerType, specificType);
        }
        else
        {
            Debug.LogError($"非触发器类型{itemType}错误调用触发器获取描述接口");
            return null;
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
                if (specificType is FoodType foodType)
                {
                    return foodAttribute.GetFoodShape(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物形状");
                    break;
                }
            case Item.ItemType.BulletItem:
                if (specificType is BulletType bulletType)
                {
                    return bulletAttribute.GetBulletShape(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹形状");
                    break;
                }
            case Item.ItemType.SurroundItem:
                if (specificType is SurroundType surroundType)
                {
                    return surroundAttribute.GetSurroundShape(surroundType);
                }
                else
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误,无法获取环绕物形状");
                    break;
                }
            case Item.ItemType.OtherItem:
                if (specificType is OtherType otherType)
                {
                    return otherAttribute.GetOtherShape(otherType);
                }
                else
                {
                    Debug.LogError($"其他道具类型{(OtherType)specificType}错误,无法获取其他道具形状");
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
                if (specificType is BulletType bulletType)
                {
                    return bulletAttribute.GetBulletAttribute(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误或未实现,无法获取子弹属性");
                    return null;
                }
            case Item.ItemType.FoodItem:
                if (specificType is FoodType foodType)
                {
                    return foodAttribute.GetFoodAttribute(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误或未实现,无法获取食物属性");
                    return null;
                }
            case Item.ItemType.SurroundItem:
                if (specificType is SurroundType surroundType)
                {
                    return surroundAttribute.GetSurroundAttribute(surroundType);
                }
                else
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误或未实现,无法获取环绕物属性");
                    return null;
                }
            case Item.ItemType.OtherItem:
                if (specificType is OtherType otherType)
                {
                    return otherAttribute.GetOtherAttribute(otherType);
                }
                else
                {
                    Debug.LogError($"其他道具类型{(OtherType)specificType}错误或未实现,无法获取其他道具属性");
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
                if (specificType is BulletType bulletType)
                {
                    return bulletAttribute.GetBulletDirection(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹方向");
                    break;
                }
            case Item.ItemType.FoodItem:
                if (specificType is FoodType foodType)
                {
                    return foodAttribute.GetFoodDirection(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物方向");
                    break;
                }
            case Item.ItemType.SurroundItem:
                if (specificType is SurroundType surroundType)
                {
                    return surroundAttribute.GetSurroundDirection(surroundType);
                }
                else
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误,无法获取环绕物方向");
                    break;
                }
            case Item.ItemType.OtherItem:
                if (specificType is OtherType otherType)
                {
                    return otherAttribute.GetOtherDirection(otherType);
                }
                else
                {
                    Debug.LogError($"其他道具类型{(OtherType)specificType}错误,无法获取其他道具方向");
                    break;
                }
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取方向属性");
                break;
        }

        return InventoryItem.Direction.NONE;
    }

    public string GetDescription(Item.ItemType itemType, object specificType)
    {
        if (itemType == Item.ItemType.TriggerItem)
        {
            Debug.LogError("触发器类型获取描述错误");
            return string.Empty;
        }

        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                if (specificType is BulletType bulletType)
                {
                    return bulletAttribute.GetDescription(bulletType);
                }
                else
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹描述");
                    break;
                }
            case Item.ItemType.FoodItem:
                if (specificType is FoodType foodType)
                {
                    return foodAttribute.GetDescription(foodType);
                }
                else
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物描述");
                    break;
                }
            case Item.ItemType.SurroundItem:
                if (specificType is SurroundType surroundType)
                {
                    return surroundAttribute.GetDescription(surroundType);
                }
                else
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误,无法获取环绕物描述");
                    break;
                }
            case Item.ItemType.OtherItem:
                if (specificType is OtherType otherType)
                {
                    return otherAttribute.GetDescription(otherType);
                }
                else
                {
                    Debug.LogError($"其他道具类型{(OtherType)specificType}错误,无法获取其他道具描述");
                    break;
                }
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取描述");
                break;
        }

        return string.Empty;
    }
    #endregion
}