using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.BagBattles.Types;

[CustomEditor(typeof(ItemIcon))]
public class ItemIconEditor : Editor
{
    private ItemIcon itemIcon;
    private bool showTriggerIcons = true;
    private bool showItemIcons = true;

    // 子分类的折叠状态
    private Dictionary<Trigger.TriggerType, bool> triggerTypeFoldouts = new Dictionary<Trigger.TriggerType, bool>();
    private Dictionary<Item.ItemType, bool> itemTypeFoldouts = new Dictionary<Item.ItemType, bool>();

    // 通过反射访问的字段
    private object iconData;
    private object triggerIconObj;
    private object itemIconObj;
    private Dictionary<Trigger.TriggerType, Dictionary<Enum, Sprite>> triggerIcons;
    private Dictionary<Item.ItemType, Dictionary<Enum, Sprite>> itemIcons;
    
    private void OnEnable()
    {
        itemIcon = (ItemIcon)target;
        InitializeReflectionFields();
    }

    private void InitializeReflectionFields()
    {
        // 通过反射获取私有字段
        FieldInfo iconDataField = typeof(ItemIcon).GetField("iconData", BindingFlags.NonPublic | BindingFlags.Instance);
        if (iconDataField != null)
        {
            iconData = iconDataField.GetValue(itemIcon);
            
            Type iconDataType = iconData.GetType();
            FieldInfo triggerIconField = iconDataType.GetField("triggerIcon");
            FieldInfo itemIconField = iconDataType.GetField("itemIcon");
            
            if (triggerIconField != null && itemIconField != null)
            {
                triggerIconObj = triggerIconField.GetValue(iconData);
                itemIconObj = itemIconField.GetValue(iconData);
                
                FieldInfo triggerIconsField = triggerIconObj.GetType().GetField("triggerIcons");
                FieldInfo itemIconsField = itemIconObj.GetType().GetField("itemIcons");
                
                if (triggerIconsField != null && itemIconsField != null)
                {
                    triggerIcons = (Dictionary<Trigger.TriggerType, Dictionary<Enum, Sprite>>)
                        triggerIconsField.GetValue(triggerIconObj);
                    
                    itemIcons = (Dictionary<Item.ItemType, Dictionary<Enum, Sprite>>)
                        itemIconsField.GetValue(itemIconObj);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (GUILayout.Button("自动加载所有图标", GUILayout.Height(30)))
        {
            AutoLoadAllIcons();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
        
        DrawTriggerIconsSection();
        EditorGUILayout.Space(10);
        DrawItemIconsSection();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTriggerIconsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showTriggerIcons = EditorGUILayout.Foldout(showTriggerIcons, "触发器图标", true, EditorStyles.foldoutHeader);
        
        if (showTriggerIcons)
        {
            EditorGUI.indentLevel++;
            
            if (GUILayout.Button("自动加载所有触发器图标"))
            {
                AutoLoadTriggerIcons();
            }
            
            foreach (Trigger.TriggerType triggerType in Enum.GetValues(typeof(Trigger.TriggerType)))
            {
                if (!triggerTypeFoldouts.ContainsKey(triggerType))
                    triggerTypeFoldouts[triggerType] = false;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                triggerTypeFoldouts[triggerType] = EditorGUILayout.Foldout(
                    triggerTypeFoldouts[triggerType], 
                    triggerType.ToString(), 
                    true
                );
                
                if (triggerTypeFoldouts[triggerType])
                {
                    EditorGUI.indentLevel++;
                    
                    if (GUILayout.Button($"加载 {triggerType} 图标"))
                    {
                        AutoLoadSpecificTriggerTypeIcons(triggerType);
                    }
                    
                    if (triggerIcons != null && triggerIcons.ContainsKey(triggerType))
                    {
                        foreach (var pair in new Dictionary<Enum, Sprite>(triggerIcons[triggerType]))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(pair.Key.ToString(), GUILayout.Width(150));
                            
                            // 添加预览图标
                            if (pair.Value != null)
                            {
                                GUILayout.Box(pair.Value.texture, GUILayout.Width(32), GUILayout.Height(32));
                            }
                            else
                            {
                                GUILayout.Box("无图标", GUILayout.Width(32), GUILayout.Height(32));
                            }
                            
                            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(
                                pair.Value, 
                                typeof(Sprite), 
                                false
                            );
                            
                            if (newSprite != pair.Value)
                            {
                                triggerIcons[triggerType][pair.Key] = newSprite;
                                EditorUtility.SetDirty(itemIcon);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawItemIconsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showItemIcons = EditorGUILayout.Foldout(showItemIcons, "物品图标", true, EditorStyles.foldoutHeader);
        
        if (showItemIcons)
        {
            EditorGUI.indentLevel++;
            
            if (GUILayout.Button("自动加载所有物品图标"))
            {
                AutoLoadItemIcons();
            }
            
            foreach (Item.ItemType itemType in Enum.GetValues(typeof(Item.ItemType)))
            {
                if (!itemTypeFoldouts.ContainsKey(itemType))
                    itemTypeFoldouts[itemType] = false;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                itemTypeFoldouts[itemType] = EditorGUILayout.Foldout(
                    itemTypeFoldouts[itemType], 
                    itemType.ToString(), 
                    true
                );
                
                if (itemTypeFoldouts[itemType])
                {
                    EditorGUI.indentLevel++;
                    
                    if (GUILayout.Button($"加载 {itemType} 图标"))
                    {
                        AutoLoadSpecificItemTypeIcons(itemType);
                    }
                    
                    if (itemIcons != null && itemIcons.ContainsKey(itemType))
                    {
                        foreach (var pair in new Dictionary<Enum, Sprite>(itemIcons[itemType]))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(pair.Key.ToString(), GUILayout.Width(150));
                            
                            // 添加预览图标
                            if (pair.Value != null)
                            {
                                GUILayout.Box(pair.Value.texture, GUILayout.Width(32), GUILayout.Height(32));
                            }
                            else
                            {
                                GUILayout.Box("无图标", GUILayout.Width(32), GUILayout.Height(32));
                            }
                            
                            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(
                                pair.Value, 
                                typeof(Sprite), 
                                false
                            );
                            
                            if (newSprite != pair.Value)
                            {
                                itemIcons[itemType][pair.Key] = newSprite;
                                EditorUtility.SetDirty(itemIcon);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void AutoLoadAllIcons()
    {
        AutoLoadTriggerIcons();
        AutoLoadItemIcons();
    }

    private void AutoLoadTriggerIcons()
    {
        foreach (Trigger.TriggerType triggerType in Enum.GetValues(typeof(Trigger.TriggerType)))
        {
            AutoLoadSpecificTriggerTypeIcons(triggerType);
        }
    }

    private void AutoLoadItemIcons()
    {
        foreach (Item.ItemType itemType in Enum.GetValues(typeof(Item.ItemType)))
        {
            AutoLoadSpecificItemTypeIcons(itemType);
        }
    }

    private void AutoLoadSpecificTriggerTypeIcons(Trigger.TriggerType triggerType)
    {
        // 所有触发器图标现在都在同一个文件夹 TriggerItem 下
        string folderPath = "Resources/Icon/TriggerItem";
        Type enumType = null;

        // 根据触发器类型确定对应的枚举类型
        switch (triggerType)
        {
            case Trigger.TriggerType.ByTime:
                enumType = typeof(TimeTriggerType);
                break;
            case Trigger.TriggerType.ByFireTimes:
                enumType = typeof(FireTriggerType);
                break;
            case Trigger.TriggerType.ByOtherTrigger:
                enumType = typeof(ByOtherTriggerType);
                break;
            default:
                Debug.LogWarning($"未知触发器类型: {triggerType}");
                return;
        }

        if (enumType == null)
        {
            Debug.LogWarning($"找不到枚举类型: {triggerType}Type");
            return;
        }

        // 在统一的TriggerItem文件夹中查找图标
        LoadIconsForTriggerEnumType(triggerType, enumType, folderPath);
    }

    private void AutoLoadSpecificItemTypeIcons(Item.ItemType itemType)
    {
        string folderPath = "";
        Type enumType = null;

        // 根据物品类型确定文件夹路径和对应的枚举类型
        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                folderPath = "Resources/Icon/BulletItem";
                enumType = typeof(BulletType);
                break;
            case Item.ItemType.FoodItem:
                folderPath = "Resources/Icon/FoodItem";
                enumType = typeof(FoodType);
                break;
            case Item.ItemType.SurroundItem:
                folderPath = "Resources/Icon/SurroundItem";
                enumType = typeof(SurroundType);
                break;
            case Item.ItemType.OtherItem:
                folderPath = "Resources/Icon/OtherItem";
                enumType = typeof(OtherType);
                break;
            case Item.ItemType.TriggerItem:
            case Item.ItemType.None:
                return;
            default:
                Debug.LogWarning($"未知物品类型: {itemType}");
                return;
        }

        if (enumType == null)
        {
            Debug.LogWarning($"找不到枚举类型: {itemType}Type");
            return;
        }

        LoadIconsForItemEnumType(itemType, enumType, folderPath);
    }

    private void LoadIconsForTriggerEnumType(Trigger.TriggerType triggerType, Type enumType, string folderPath)
    {
        // 确保路径存在
        string fullPath = Path.Combine(Application.dataPath, folderPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"路径不存在: {fullPath}，正在创建...");
            Directory.CreateDirectory(fullPath);
        }

        // 获取该枚举类型的所有值
        Array enumValues = Enum.GetValues(enumType);
        
        int matchCount = 0;
        string prefix = GetTriggerTypePrefix(triggerType);
        
        // 获取所有图标文件
        LoadSpritesForEnum(triggerType, enumValues, folderPath, prefix, ref matchCount);
        
        Debug.Log($"已加载 {matchCount} 个 {triggerType} 图标，共 {enumValues.Length} 个枚举值");
        EditorUtility.SetDirty(itemIcon);
    }
    
    private void LoadIconsForItemEnumType(Item.ItemType itemType, Type enumType, string folderPath)
    {
        // 确保路径存在
        string fullPath = Path.Combine(Application.dataPath, folderPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"路径不存在: {fullPath}，正在创建...");
            Directory.CreateDirectory(fullPath);
        }

        // 获取该枚举类型的所有值
        Array enumValues = Enum.GetValues(enumType);
        
        int matchCount = 0;
        string prefix = ""; // 物品图标不需要前缀
        
        // 获取所有图标文件
        LoadSpritesForEnum(itemType, enumValues, folderPath, prefix, ref matchCount);
        
        Debug.Log($"已加载 {matchCount} 个 {itemType} 图标，共 {enumValues.Length} 个枚举值");
        EditorUtility.SetDirty(itemIcon);
    }
    
    // 获取触发器类型对应的文件名前缀
    private string GetTriggerTypePrefix(Trigger.TriggerType triggerType)
    {
        switch (triggerType)
        {
            case Trigger.TriggerType.ByTime:
                return "Time_";
            case Trigger.TriggerType.ByFireTimes:
                return "Fire_";
            case Trigger.TriggerType.ByOtherTrigger:
                return "ByOther_";
            default:
                return "";
        }
    }
    
    // 加载枚举对应的Sprite
    private void LoadSpritesForEnum(Enum typeEnum, Array enumValues, string folderPath, string prefix, ref int matchCount)
    {
        // 1. 首先尝试查找单独的图片文件
        string[] imageFiles = Directory.GetFiles(Path.Combine(Application.dataPath, folderPath), "*.png")
            .Concat(Directory.GetFiles(Path.Combine(Application.dataPath, folderPath), "*.jpg"))
            .ToArray();

        // 2. 然后获取文件夹中的所有纹理资产，用于获取Sprite Sheet中的子Sprite
        string[] allAssetGuids = AssetDatabase.FindAssets("t:texture", new[] { "Assets/" + folderPath });
        List<Sprite> allSprites = new List<Sprite>();
        
        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 获取该纹理的所有子Sprite
            allSprites.AddRange(
                AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .ToArray());
        }
        
        // 3. 遍历枚举值，寻找匹配的图标
        foreach (Enum enumValue in enumValues)
        {
            string enumName = enumValue.ToString();
            string fileNameWithPrefix = prefix + enumName;
            bool foundMatch = false;
            
            // 3.1 先尝试查找完全匹配的单个文件
            string matchedFile = imageFiles.FirstOrDefault(f => 
                Path.GetFileNameWithoutExtension(f).Equals(fileNameWithPrefix, StringComparison.OrdinalIgnoreCase));
            
            if (matchedFile != null)
            {
                string assetPath = "Assets/" + folderPath + "/" + Path.GetFileName(matchedFile);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                
                if (sprite != null)
                {
                    AssignSpriteToEnum(typeEnum, enumValue, sprite);
                    matchCount++;
                    foundMatch = true;
                }
            }
            
            // 3.2 如果没有文件匹配，尝试查找子Sprite
            if (!foundMatch)
            {
                // 查找名称匹配的Sprite(带前缀)
                Sprite matchedSprite = allSprites.FirstOrDefault(s => 
                    s.name.Equals(fileNameWithPrefix, StringComparison.OrdinalIgnoreCase));
                
                // 如果没找到带前缀的，尝试查找不带前缀的
                if (matchedSprite == null)
                {
                    matchedSprite = allSprites.FirstOrDefault(s => 
                        s.name.Equals(enumName, StringComparison.OrdinalIgnoreCase));
                }
                
                if (matchedSprite != null)
                {
                    AssignSpriteToEnum(typeEnum, enumValue, matchedSprite);
                    matchCount++;
                    foundMatch = true;
                }
            }
            
            if (!foundMatch)
            {
                Debug.Log($"找不到枚举 {enumName} 对应的图标 (尝试查找: {fileNameWithPrefix} 或 {enumName})");
            }
        }
    }

    private void AssignSpriteToEnum(Enum typeCategory, Enum enumValue, Sprite sprite)
    {
        // 根据类型分别存储到触发器图标或物品图标字典
        if (typeCategory is Trigger.TriggerType triggerType)
        {
            if (!triggerIcons.ContainsKey(triggerType))
            {
                triggerIcons[triggerType] = new Dictionary<Enum, Sprite>();
            }
            triggerIcons[triggerType][enumValue] = sprite;
        }
        else if (typeCategory is Item.ItemType itemType)
        {
            if (!itemIcons.ContainsKey(itemType))
            {
                itemIcons[itemType] = new Dictionary<Enum, Sprite>();
            }
            itemIcons[itemType][enumValue] = sprite;
        }
    }
}