using System.Collections.Generic;
using UnityEngine;
using Assets.BagBattles.Types;
using System;

[CreateAssetMenu(fileName = "ItemIcon", menuName = "ItemIcon", order = 0)]
public class ItemIcon : ScriptableObject
{
    private class IconData
    {
        public class TriggerIcon
        {
            public Dictionary<Trigger.TriggerType, Dictionary<Enum, Sprite>> triggerIcons;

            public TriggerIcon()
            {
                triggerIcons = new Dictionary<Trigger.TriggerType, Dictionary<Enum, Sprite>>();

                foreach (Trigger.TriggerType type in Enum.GetValues(typeof(Trigger.TriggerType)))
                {
                    Type specificType = GetTriggerTypeEnum(type);
                    if (specificType != null)
                    {
                        triggerIcons[type] = new Dictionary<Enum, Sprite>();
                        
                        // 初始化为空值
                        foreach (Enum enumValue in Enum.GetValues(specificType))
                        {
                            triggerIcons[type][enumValue] = null;
                        }
                    }
                    else
                    {
                        Debug.LogError($"未找到类型：{type}Type");
                    }
                }
            }
            
            private Type GetTriggerTypeEnum(Trigger.TriggerType type)
            {
                switch (type)
                {
                    case Trigger.TriggerType.ByTime:
                        return typeof(TimeTriggerType);
                    case Trigger.TriggerType.ByFireTimes:
                        return typeof(FireTriggerType);
                    case Trigger.TriggerType.ByOtherTrigger:
                        return typeof(ByOtherTriggerType);
                    default:
                        return null;
                }
            }

            public bool TryGetIcon(Trigger.TriggerType functionType, Enum triggerEnum, out Sprite sprite)
            {
                if (triggerIcons.TryGetValue(functionType, out var icon))
                {
                    if (!icon.TryGetValue(triggerEnum, out sprite))
                    {
                        Debug.LogError($"未找到图标：{triggerEnum}");
                        sprite = null;
                        return false;
                    }
                    return sprite != null;
                }
                else
                {
                    Debug.LogError($"未找到功能类型：{functionType}");
                    sprite = null;
                    return false;
                }
            }

            public Sprite GetIcon(Trigger.TriggerType functionType, Enum triggerEnum)
            {
                if (TryGetIcon(functionType, triggerEnum, out var sprite))
                {
                    return sprite;
                }
                else
                {
                    Debug.LogError($"未找到道具图标：{triggerEnum}");
                    return null;
                }
            }
        }

        public class ItemIcon
        {
            public Dictionary<Item.ItemType, Dictionary<Enum, Sprite>> itemIcons;
            public ItemIcon()
            {
                itemIcons = new Dictionary<Item.ItemType, Dictionary<Enum, Sprite>>();

                InitItemType(Item.ItemType.BulletItem, typeof(BulletType));
                InitItemType(Item.ItemType.FoodItem, typeof(FoodType));
                InitItemType(Item.ItemType.SurroundItem, typeof(SurroundType));
                InitItemType(Item.ItemType.OtherItem, typeof(OtherType));
            }
            
            private void InitItemType(Item.ItemType itemType, Type enumType)
            {
                itemIcons[itemType] = new Dictionary<Enum, Sprite>();
                
                // 初始化为空值
                foreach (Enum enumValue in Enum.GetValues(enumType))
                {
                    itemIcons[itemType][enumValue] = null;
                }
            }

            public bool TryGetIcon(Item.ItemType itemType, Enum specificType, out Sprite sprite)
            {
                if (itemIcons.TryGetValue(itemType, out var icons))
                {
                    if (!icons.TryGetValue(specificType, out sprite))
                    {
                        Debug.LogError($"未找到图标：{specificType}");
                        sprite = null;
                        return false;
                    }
                    return sprite != null;
                }
                else
                {
                    Debug.LogError($"未找到道具类型{itemType}");
                    sprite = null;
                    return false;
                }
            }

            public Sprite GetIcon(Item.ItemType itemType, Enum specificType)
            {
                if (TryGetIcon(itemType, specificType, out var sprite))
                {
                    return sprite;
                }
                else
                {
                    Debug.LogError($"未找到图标：{specificType}");
                    return null;
                }
            }
        }

        public TriggerIcon triggerIcon = new();
        public ItemIcon itemIcon = new();
    }

    protected static ItemIcon instance = null;
    public static ItemIcon Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ItemIcon>("ItemIcon");
                if (instance == null)
                {
                    instance = CreateInstance<ItemIcon>();
                }
            }
            return instance;
        }
    }

    IconData iconData = new();
    public bool TryGetIcon(Enum type1, Enum type2, bool isTriggerItem, out Sprite sprite)
    {
        if (isTriggerItem)
        {
            return iconData.triggerIcon.TryGetIcon((Trigger.TriggerType)type1, type2, out sprite);
        }
        else
        {
            return iconData.itemIcon.TryGetIcon((Item.ItemType)type1, type2, out sprite);
        }
    }
    
    // 添加一个OnValidate方法，确保iconData总是被初始化
    private void OnValidate()
    {
        if (iconData == null)
        {
            iconData = new IconData();
        }
    }
}