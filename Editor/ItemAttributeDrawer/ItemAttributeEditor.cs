using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using Assets.BagBattles.Types;

/// <summary>
/// ItemAttribute的自定义编辑器，提供属性配置和JSON导入/导出功能
/// </summary>
[CustomEditor(typeof(ItemAttribute))]
public class ItemAttributeEditor : Editor
{
    #region 序列化属性
    private SerializedProperty triggerAttributeProperty;
    private SerializedProperty bulletAttributeProperty;
    private SerializedProperty foodAttributeProperty;
    private SerializedProperty surroundAttributeProperty;
    private SerializedProperty otherAttributeProperty;
    #endregion

    #region 折叠状态
    // 使用EditorPrefs来保存折叠状态，解决折叠菜单无法展开的问题
    private bool ShowTriggerFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_TriggerFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_TriggerFoldout", value); }
    }
    
    private bool ShowBulletFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_BulletFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_BulletFoldout", value); }
    }
    
    private bool ShowFoodFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_FoodFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_FoodFoldout", value); }
    }
    
    private bool ShowSurroundFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_SurroundFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_SurroundFoldout", value); }
    }

    private bool ShowOtherFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_OtherFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_OtherFoldout", value); }
    }
    // 子类别折叠状态
    private bool ShowFireTriggerFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_FireTriggerFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_FireTriggerFoldout", value); }
    }
    
    private bool ShowTimeTriggerFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_TimeTriggerFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_TimeTriggerFoldout", value); }
    }
    
    private bool ShowByOtherTriggerFoldout
    {
        get { return EditorPrefs.GetBool("ItemAttributeEditor_ByOtherTriggerFoldout", false); }
        set { EditorPrefs.SetBool("ItemAttributeEditor_ByOtherTriggerFoldout", value); }
    }
    #endregion

    // 初始化
    private void OnEnable()
    {
        triggerAttributeProperty = serializedObject.FindProperty("triggerAttribute");
        bulletAttributeProperty = serializedObject.FindProperty("bulletAttribute");
        foodAttributeProperty = serializedObject.FindProperty("foodAttribute");
        surroundAttributeProperty = serializedObject.FindProperty("surroundAttribute");
        otherAttributeProperty = serializedObject.FindProperty("otherAttribute");
    }

    public override void OnInspectorGUI()
    {
        // 改进折叠菜单点击响应
        EditorGUIUtility.hierarchyMode = true;
        
        // 保存GUI变更状态
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        GUILayout.Space(10);

        // 触发器配置 - 第一个顶级折叠组
        bool triggerFoldout = ShowTriggerFoldout;
        triggerFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(triggerFoldout, "触发器配置");
        ShowTriggerFoldout = triggerFoldout;
        
        if (triggerFoldout)
        {
            EditorGUI.indentLevel++;

            // 开火触发器 - 使用自定义折叠控件而不是嵌套BeginFoldoutHeaderGroup
            bool fireTriggerFoldout = ShowFireTriggerFoldout;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                fireTriggerFoldout = EditorGUILayout.Toggle(fireTriggerFoldout, EditorStyles.foldout, GUILayout.Width(15));
                if (GUILayout.Button("开火触发器", EditorStyles.label))
                {
                    fireTriggerFoldout = !fireTriggerFoldout;
                }
            }
            
            ShowFireTriggerFoldout = fireTriggerFoldout;
            
            if (fireTriggerFoldout)
            {
                DrawFireTriggers();
            }

            GUILayout.Space(5);

            // 时间触发器 - 使用自定义折叠控件
            bool timeTriggerFoldout = ShowTimeTriggerFoldout;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                timeTriggerFoldout = EditorGUILayout.Toggle(timeTriggerFoldout, EditorStyles.foldout, GUILayout.Width(15));
                if (GUILayout.Button("时间触发器", EditorStyles.label))
                {
                    timeTriggerFoldout = !timeTriggerFoldout;
                }
            }
            
            ShowTimeTriggerFoldout = timeTriggerFoldout;
            
            if (timeTriggerFoldout)
            {
                DrawTimeTriggers();
            }

            GUILayout.Space(5);

            // 其他触发器 - 使用自定义折叠控件
            bool byOtherTriggerFoldout = ShowByOtherTriggerFoldout;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                byOtherTriggerFoldout = EditorGUILayout.Toggle(byOtherTriggerFoldout, EditorStyles.foldout, GUILayout.Width(15));
                if (GUILayout.Button("被其他触发器触发触发器", EditorStyles.label))
                {
                    byOtherTriggerFoldout = !byOtherTriggerFoldout;
                }
            }

            ShowByOtherTriggerFoldout = byOtherTriggerFoldout;

            if (byOtherTriggerFoldout)
            {
                DrawByOtherTriggers();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup(); // 结束第一个折叠组

        GUILayout.Space(5);

        // 子弹配置 - 第二个顶级折叠组
        bool bulletFoldout = ShowBulletFoldout;
        bulletFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(bulletFoldout, "子弹配置");
        ShowBulletFoldout = bulletFoldout;
        
        if (bulletFoldout)
        {
            EditorGUI.indentLevel++;
            DrawBullets();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup(); // 结束第二个折叠组

        GUILayout.Space(5);

        // 食物配置 - 第三个顶级折叠组
        bool foodFoldout = ShowFoodFoldout;
        foodFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(foodFoldout, "食物配置");
        EditorGUILayout.EndFoldoutHeaderGroup(); // 结束第三个折叠组
        ShowFoodFoldout = foodFoldout;
        
        if (foodFoldout)
        {
            EditorGUI.indentLevel++;
            DrawFoods();
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(5);

        // 环绕物配置 - 第四个顶级折叠组
        bool surroundFoldout = ShowSurroundFoldout;
        surroundFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(surroundFoldout, "环绕物配置");
        ShowSurroundFoldout = surroundFoldout;
        
        if (surroundFoldout)
        {
            EditorGUI.indentLevel++;
            DrawSurrounds();

            // 环绕物预制体映射 - 使用自定义折叠控件
            // bool surroundPrefabsFoldout = ShowSurroundPrefabsFoldout;
            // using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            // {
            //     surroundPrefabsFoldout = EditorGUILayout.Toggle(surroundPrefabsFoldout, EditorStyles.foldout, GUILayout.Width(15));
            //     if (GUILayout.Button("环绕物预制体映射", EditorStyles.label))
            //     {
            //         surroundPrefabsFoldout = !surroundPrefabsFoldout;
            //     }
            // }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup(); // 结束第四个折叠组

        GUILayout.Space(5);

        // 其他物品配置 - 第五个顶级折叠组
        bool otherFoldout = ShowOtherFoldout;
        otherFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(otherFoldout, "其他物品配置");
        ShowOtherFoldout = otherFoldout;
        
        if (otherFoldout)
        {
            EditorGUI.indentLevel++;
            DrawOther();

            // 其他物品预制体映射 - 使用自定义折叠控件
            // bool otherPrefabsFoldout = ShowOtherPrefabsFoldout;
            // using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            // {
            //     otherPrefabsFoldout = EditorGUILayout.Toggle(otherPrefabsFoldout, EditorStyles.foldout, GUILayout.Width(15));
            //     if (GUILayout.Button("其他物品预制体映射", EditorStyles.label))
            //     {
            //         otherPrefabsFoldout = !otherPrefabsFoldout;
            //     }
            // }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup(); // 结束第四个折叠组

        GUILayout.Space(10);
        
        // 绘制JSON导入/导出区域
        DrawJSONSection();

        serializedObject.ApplyModifiedProperties();
        
        // 检测GUI变更并保存
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }

    #region 绘制方法

    /// <summary>
    /// 绘制JSON导入导出区域
    /// </summary>
    private void DrawJSONSection()
    {
        EditorGUILayout.LabelField("JSON导入/导出功能", EditorStyles.boldLabel);

        // 检查是否正在编辑多个对象
        if (serializedObject.isEditingMultipleObjects)
        {
            EditorGUILayout.HelpBox("JSON导入/导出功能不支持多对象编辑模式。请选择单个ItemAttribute对象。", MessageType.Warning);
            return;
        }

        // 获取当前编辑的目标对象
        ItemAttribute itemAttribute = (ItemAttribute)target;

        // 创建水平布局，使按钮并排显示
        using (new EditorGUILayout.HorizontalScope())
        {
            // 添加导出按钮
            if (GUILayout.Button("导出到JSON文件", GUILayout.Height(30)))
            {
                ExportToJson(itemAttribute);
            }

            // 添加从JSON加载按钮
            if (GUILayout.Button("从JSON文件加载", GUILayout.Height(30)))
            {
                ImportFromJson(itemAttribute);
            }
        }

        // 显示JSON文件路径信息
        string jsonPath = Path.Combine(Application.dataPath, "Resources/ItemAttributes/ItemAttributeConfig.json");
        bool fileExists = File.Exists(jsonPath);
        EditorGUILayout.HelpBox(
            $"JSON文件位置: {jsonPath}\n状态: {(fileExists ? "文件已存在" : "文件不存在")}",
            fileExists ? MessageType.Info : MessageType.Warning);
    }
    
    /// <summary>
    /// 绘制开火触发器
    /// </summary>
    private void DrawFireTriggers()
    {
        EditorGUI.indentLevel++;
        
        // 获取开火触发器属性列表
        SerializedProperty fireTriggerAttributes = triggerAttributeProperty.FindPropertyRelative("fireTriggerAttributes");
        
        // 遍历所有 FireTriggerType 枚举值
        Array fireTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.FireTriggerType));
        foreach (Assets.BagBattles.Types.FireTriggerType type in fireTypes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"类型: {type}", EditorStyles.boldLabel);
            
            // 查找该类型在属性数组中的索引
            int index = -1;
            for (int i = 0; i < fireTriggerAttributes.arraySize; i++)
            {
                SerializedProperty itemProperty = fireTriggerAttributes.GetArrayElementAtIndex(i);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("fireCountTriggerAttribute");
                SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("fireTriggerType");
                
                if ((int)type == typeProperty.enumValueIndex)
                {
                    index = i;
                    break;
                }
            }
            
            if (index >= 0)
            {
                // 显示已有配置
                SerializedProperty itemProperty = fireTriggerAttributes.GetArrayElementAtIndex(index);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("fireCountTriggerAttribute");
                
                // 显示相关属性
                SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
                EditorGUILayout.PropertyField(rangeProperty, new GUIContent("触发范围"));
                
                SerializedProperty countProperty = attributeProperty.FindPropertyRelative("fireCount");
                EditorGUILayout.PropertyField(countProperty, new GUIContent("开火次数"));
                
                SerializedProperty descriptionProperty = attributeProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("描述"));

                if (itemProperty.FindPropertyRelative("fireTriggerShape") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("fireTriggerShape"), new GUIContent("形状"));
                
                if (itemProperty.FindPropertyRelative("fireTriggerDirection") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("fireTriggerDirection"), new GUIContent("方向"));
            }
            else
            {
                // 显示添加按钮
                EditorGUILayout.HelpBox("该类型尚未配置", MessageType.Info);
                
                if (GUILayout.Button("添加配置"))
                {
                    AddFireTriggerConfig(fireTriggerAttributes, type);
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制时间触发器
    /// </summary>
    private void DrawTimeTriggers()
    {
        EditorGUI.indentLevel++;
        
        // 获取时间触发器属性列表
        SerializedProperty timeTriggerAttributes = triggerAttributeProperty.FindPropertyRelative("timeTriggerAttributes");
        
        // 遍历所有 TimeTriggerType 枚举值
        Array timeTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.TimeTriggerType));
        foreach (Assets.BagBattles.Types.TimeTriggerType type in timeTypes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"类型: {type}", EditorStyles.boldLabel);
            
            // 查找该类型在属性数组中的索引
            int index = -1;
            for (int i = 0; i < timeTriggerAttributes.arraySize; i++)
            {
                SerializedProperty itemProperty = timeTriggerAttributes.GetArrayElementAtIndex(i);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("timeTriggerAttribute");
                SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("timeTriggerType");
                
                if ((int)type == typeProperty.enumValueIndex)
                {
                    index = i;
                    break;
                }
            }
            
            if (index >= 0)
            {
                // 显示已有配置
                SerializedProperty itemProperty = timeTriggerAttributes.GetArrayElementAtIndex(index);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("timeTriggerAttribute");
                
                // 显示相关属性
                SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
                EditorGUILayout.PropertyField(rangeProperty, new GUIContent("触发范围"));
                
                SerializedProperty countProperty = attributeProperty.FindPropertyRelative("triggerTime");
                EditorGUILayout.PropertyField(countProperty, new GUIContent("触发时间"));
                
                SerializedProperty descriptionProperty = attributeProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("描述"));

                if (itemProperty.FindPropertyRelative("timeTriggerShape") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("timeTriggerShape"), new GUIContent("形状"));
                
                if (itemProperty.FindPropertyRelative("timeTriggerDirection") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("timeTriggerDirection"), new GUIContent("方向"));
            }
            else
            {
                // 显示添加按钮
                EditorGUILayout.HelpBox("该类型尚未配置", MessageType.Info);
                
                if (GUILayout.Button("添加配置"))
                {
                    AddTimeTriggerConfig(timeTriggerAttributes, type);
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制被其他触发器触发触发器
    /// </summary>
    private void DrawByOtherTriggers()
    {
        EditorGUI.indentLevel++;
        
        // 获取开火触发器属性列表
        SerializedProperty byOtherTriggerAttributes = triggerAttributeProperty.FindPropertyRelative("byOtherTriggerAttributes");
        
        // 遍历所有 ByOtherTriggerType 枚举值
        Array byOtherTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.ByOtherTriggerType));
        foreach (Assets.BagBattles.Types.ByOtherTriggerType type in byOtherTypes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"类型: {type}", EditorStyles.boldLabel);
            
            // 查找该类型在属性数组中的索引
            int index = -1;
            for (int i = 0; i < byOtherTriggerAttributes.arraySize; i++)
            {
                SerializedProperty itemProperty = byOtherTriggerAttributes.GetArrayElementAtIndex(i);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("byOtherTriggerAttribute");
                SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("byOtherTriggerType");
                
                if ((int)type == typeProperty.enumValueIndex)
                {
                    index = i;
                    break;
                }
            }
            
            if (index >= 0)
            {
                // 显示已有配置
                SerializedProperty itemProperty = byOtherTriggerAttributes.GetArrayElementAtIndex(index);
                SerializedProperty attributeProperty = itemProperty.FindPropertyRelative("byOtherTriggerAttribute");
                
                // 显示相关属性
                SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
                EditorGUILayout.PropertyField(rangeProperty, new GUIContent("触发范围"));
                
                SerializedProperty countProperty = attributeProperty.FindPropertyRelative("requiredTriggerCount");
                EditorGUILayout.PropertyField(countProperty, new GUIContent("触发时所需其他触发器触发次数"));
                
                SerializedProperty descriptionProperty = attributeProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("描述"));

                if (itemProperty.FindPropertyRelative("byOtherTriggerShape") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("byOtherTriggerShape"), new GUIContent("形状"));
                
                if (itemProperty.FindPropertyRelative("byOtherTriggerDirection") != null)
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("byOtherTriggerDirection"), new GUIContent("方向"));
            }
            else
            {
                // 显示添加按钮
                EditorGUILayout.HelpBox("该类型尚未配置", MessageType.Info);
                
                if (GUILayout.Button("添加配置"))
                {
                    AddByOtherTriggerConfig(byOtherTriggerAttributes, type);
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制子弹配置
    /// </summary>
    private void DrawBullets()
    {
        // 检查属性是否存在
        if (bulletAttributeProperty == null)
        {
            EditorGUILayout.HelpBox("子弹属性不存在", MessageType.Warning);
            return;
        }

        SerializedProperty bulletAttributes = bulletAttributeProperty.FindPropertyRelative("bulletAttributes");

        EditorGUILayout.LabelField("子弹类型", EditorStyles.boldLabel);

        // 遍历所有 BulletType 枚举值
        Array bulletTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.BulletType));
        foreach (Assets.BagBattles.Types.BulletType type in bulletTypes)
        {
            // 跳过None类型 (如果有)
            if ((int)type == 0 && type.ToString() == "None") continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            using (new EditorGUILayout.HorizontalScope())
            {
                // EditorGUILayout.LabelField($"子弹类型:", GUILayout.Width(60));
                // EditorGUILayout.LabelField($"{type}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"子弹类型：{type}", EditorStyles.boldLabel);
            }

            // 查找该类型在属性数组中的索引
            int index = FindTypeIndex(bulletAttributes, "bulletItemAttribute", "specificBulletType", (int)type);

            if (index >= 0)
            {
                // 找到了对应类型的属性
                SerializedProperty itemProperty = bulletAttributes.GetArrayElementAtIndex(index);
                SerializedProperty bulletItemAttributeProperty = itemProperty.FindPropertyRelative("bulletItemAttribute");

                // 显示子弹属性
                EditorGUILayout.PropertyField(bulletItemAttributeProperty.FindPropertyRelative("bulletType"), new GUIContent("子弹种类"));
                EditorGUILayout.PropertyField(bulletItemAttributeProperty.FindPropertyRelative("bulletCount"), new GUIContent("子弹数量"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemShape"), new GUIContent("形状"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDirection"), new GUIContent("方向"));
                EditorGUILayout.PropertyField(bulletItemAttributeProperty.FindPropertyRelative("description"), new GUIContent("描述"));
            }
            else
            {
                EditorGUILayout.HelpBox("该类型尚未配置。", MessageType.Info);

                // 添加"添加配置"按钮
                if (GUILayout.Button("添加配置", GUILayout.Height(25)))
                {
                    // 添加新的子弹配置
                    AddBulletConfig(bulletAttributes, (Assets.BagBattles.Types.BulletType)type);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    /// <summary>
    /// 绘制食物配置
    /// </summary>
    private void DrawFoods()
    {
        // 检查属性是否存在
        if (foodAttributeProperty == null)
        {
            EditorGUILayout.HelpBox("食物属性不存在", MessageType.Warning);
            return;
        }

        SerializedProperty foodAttributes = foodAttributeProperty.FindPropertyRelative("foodAttributes");

        EditorGUILayout.LabelField("食物类型", EditorStyles.boldLabel);

        // 遍历所有 FoodType 枚举值
        Array foodTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.FoodType));
        foreach (Assets.BagBattles.Types.FoodType type in foodTypes)
        {
            // 跳过None类型 (如果有)
            if ((int)type == 0 && type.ToString() == "None") continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"食物类型：{type}", EditorStyles.boldLabel);
            }

            // 查找该类型在属性数组中的索引
            int index = FindTypeIndex(foodAttributes, "foodItemAttribute", "specificFoodType", (int)type);

            if (index >= 0)
            {
                // 找到了对应类型的属性
                SerializedProperty itemProperty = foodAttributes.GetArrayElementAtIndex(index);
                SerializedProperty foodItemAttributeProperty = itemProperty.FindPropertyRelative("foodItemAttribute");

                // 显示食物基本属性
                EditorGUILayout.PropertyField(foodItemAttributeProperty.FindPropertyRelative("specificFoodType"), new GUIContent("食物类型"));

                // 显示食物效果列表
                EditorGUILayout.PropertyField(foodItemAttributeProperty.FindPropertyRelative("foodItemAttributes"), new GUIContent("食物效果"), true);

                // 显示形状和方向
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemShape"), new GUIContent("形状"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDirection"), new GUIContent("方向"));
                
                EditorGUILayout.PropertyField(foodItemAttributeProperty.FindPropertyRelative("description"), new GUIContent("描述"));
            }
            else
            {
                EditorGUILayout.HelpBox("该类型尚未配置。", MessageType.Info);

                // 添加"添加配置"按钮
                if (GUILayout.Button("添加配置", GUILayout.Height(25)))
                {
                    // 添加新的食物配置
                    AddFoodConfig(foodAttributes, (Assets.BagBattles.Types.FoodType)type);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    /// <summary>
    /// 绘制环绕物配置
    /// </summary>
    private void DrawSurrounds()
    {
        // 检查属性是否存在
        if (surroundAttributeProperty == null)
        {
            EditorGUILayout.HelpBox("环绕物属性不存在", MessageType.Warning);
            return;
        }

        SerializedProperty surroundAttributes = surroundAttributeProperty.FindPropertyRelative("surroundAttributes");

        EditorGUILayout.LabelField("环绕物类型", EditorStyles.boldLabel);

        // 遍历所有 SurroundType 枚举值
        Array surroundTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.SurroundType));
        foreach (Assets.BagBattles.Types.SurroundType type in surroundTypes)
        {
            // 跳过None类型 (如果有)
            if ((int)type == 0 && type.ToString() == "None") continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // EditorGUILayout.LabelField($"环绕物类型:", GUILayout.Width(70));
                // EditorGUILayout.LabelField($"{type}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"环绕物类型：{type}", EditorStyles.boldLabel);
            }

            // 查找该类型在属性数组中的索引
            int index = FindTypeIndex(surroundAttributes, "surroundItemAttribute", "specificSurroundType", (int)type);

            if (index >= 0)
            {
                // 找到了对应类型的属性
                SerializedProperty itemProperty = surroundAttributes.GetArrayElementAtIndex(index);
                SerializedProperty surroundItemAttributeProperty = itemProperty.FindPropertyRelative("surroundItemAttribute");

                // 显示环绕物属性
                EditorGUILayout.PropertyField(surroundItemAttributeProperty.FindPropertyRelative("summonedSurroundingType"), new GUIContent("环绕物种类"));
                EditorGUILayout.PropertyField(surroundItemAttributeProperty.FindPropertyRelative("surroundingCount"), new GUIContent("环绕物数量"));
                EditorGUILayout.PropertyField(surroundItemAttributeProperty.FindPropertyRelative("surroundingSpeedPercent"), new GUIContent("触发时加速百分比"));
                EditorGUILayout.PropertyField(surroundItemAttributeProperty.FindPropertyRelative("surroundingDuration"), new GUIContent("环绕物加速持续时间"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemShape"), new GUIContent("形状"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDirection"), new GUIContent("方向"));
                EditorGUILayout.PropertyField(surroundItemAttributeProperty.FindPropertyRelative("description"), new GUIContent("描述"));
            }
            else
            {
                EditorGUILayout.HelpBox("该类型尚未配置。", MessageType.Info);

                // 添加"添加配置"按钮
                if (GUILayout.Button("添加配置", GUILayout.Height(25)))
                {
                    // 添加新的环绕物配置
                    AddSurroundConfig(surroundAttributes, (Assets.BagBattles.Types.SurroundType)type);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
    
    /// <summary>
    /// 绘制其他物品配置
    /// </summary>
    private void DrawOther()
    {
        // 检查属性是否存在
        if (otherAttributeProperty == null)
        {
            EditorGUILayout.HelpBox("其他类别道具属性不存在", MessageType.Warning);
            return;
        }

        SerializedProperty otherAttributes = otherAttributeProperty.FindPropertyRelative("otherAttributes");

        EditorGUILayout.LabelField("其他类别道具类型", EditorStyles.boldLabel);

        // 遍历所有 OtherType 枚举值
        Array otherTypes = Enum.GetValues(typeof(Assets.BagBattles.Types.OtherType));
        foreach (Assets.BagBattles.Types.OtherType type in otherTypes)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // EditorGUILayout.LabelField($"其他类别道具类型:", GUILayout.Width(70));
                // EditorGUILayout.LabelField($"{type}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"其他类别道具类型：{type}", EditorStyles.boldLabel);
            }

            // 查找该类型在属性数组中的索引
            int index = FindTypeIndex(otherAttributes, "otherItemAttribute", "specificOtherType", (int)type);

            if (index >= 0)
            {
                // 找到了对应类型的属性
                SerializedProperty itemProperty = otherAttributes.GetArrayElementAtIndex(index);
                SerializedProperty otherItemAttributeProperty = itemProperty.FindPropertyRelative("otherItemAttribute");

                // 显示其他类别道具属性
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemShape"), new GUIContent("形状"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDirection"), new GUIContent("方向"));
                EditorGUILayout.PropertyField(otherItemAttributeProperty.FindPropertyRelative("description"), new GUIContent("描述"));
            }
            else
            {
                EditorGUILayout.HelpBox("该类型尚未配置。", MessageType.Info);

                // 添加"添加配置"按钮
                if (GUILayout.Button("添加配置", GUILayout.Height(25)))
                {
                    // 添加新的其他类别道具配置
                    AddOtherConfig(otherAttributes, type);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 添加新的子弹配置
    /// </summary>
    private void AddBulletConfig(SerializedProperty bulletAttributes, Assets.BagBattles.Types.BulletType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        bulletAttributes.arraySize++;
        SerializedProperty newItem = bulletAttributes.GetArrayElementAtIndex(bulletAttributes.arraySize - 1);

        // 设置子弹类型
        SerializedProperty bulletItemAttributeProperty = newItem.FindPropertyRelative("bulletItemAttribute");
        SerializedProperty typeProperty = bulletItemAttributeProperty.FindPropertyRelative("specificBulletType");
        typeProperty.enumValueIndex = (int)type;

        // 设置其他属性的默认值
        SerializedProperty bulletTypeProperty = bulletItemAttributeProperty.FindPropertyRelative("bulletType");
        if (bulletTypeProperty != null)
            bulletTypeProperty.enumValueIndex = 0; // 默认子弹种类

        SerializedProperty bulletCountProperty = bulletItemAttributeProperty.FindPropertyRelative("bulletCount");
        if (bulletCountProperty != null)
            bulletCountProperty.intValue = 1; // 默认子弹数量

        SerializedProperty itemShapeProperty = newItem.FindPropertyRelative("itemShape");
        if (itemShapeProperty != null)
            itemShapeProperty.enumValueIndex = 0; // 默认形状

        SerializedProperty itemDirectionProperty = newItem.FindPropertyRelative("itemDirection");
        if (itemDirectionProperty != null)
            itemDirectionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加子弹配置: {type}");
    }

    /// <summary>
    /// 添加新的食物配置
    /// </summary>
    private void AddFoodConfig(SerializedProperty foodAttributes, Assets.BagBattles.Types.FoodType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        foodAttributes.arraySize++;
        SerializedProperty newItem = foodAttributes.GetArrayElementAtIndex(foodAttributes.arraySize - 1);

        // 设置食物类型
        SerializedProperty foodItemAttributeProperty = newItem.FindPropertyRelative("foodItemAttribute");
        SerializedProperty typeProperty = foodItemAttributeProperty.FindPropertyRelative("specificFoodType");
        typeProperty.enumValueIndex = (int)type;

        // 创建空的食物效果列表
        SerializedProperty foodItemAttributes = foodItemAttributeProperty.FindPropertyRelative("foodItemAttributes");
        if (foodItemAttributes != null)
            foodItemAttributes.arraySize = 0; // 初始为空列表

        // 设置其他默认属性
        SerializedProperty itemShapeProperty = newItem.FindPropertyRelative("itemShape");
        if (itemShapeProperty != null)
            itemShapeProperty.enumValueIndex = 0; // 默认形状

        SerializedProperty itemDirectionProperty = newItem.FindPropertyRelative("itemDirection");
        if (itemDirectionProperty != null)
            itemDirectionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加食物配置: {type}");
    }

    /// <summary>
    /// 添加新的环绕物配置
    /// </summary>
    private void AddSurroundConfig(SerializedProperty surroundAttributes, Assets.BagBattles.Types.SurroundType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        surroundAttributes.arraySize++;
        SerializedProperty newItem = surroundAttributes.GetArrayElementAtIndex(surroundAttributes.arraySize - 1);

        // 设置环绕物类型
        SerializedProperty surroundItemAttributeProperty = newItem.FindPropertyRelative("surroundItemAttribute");
        SerializedProperty typeProperty = surroundItemAttributeProperty.FindPropertyRelative("specificSurroundType");
        typeProperty.enumValueIndex = (int)type;

        // 设置其他属性的默认值
        SerializedProperty summonedTypeProperty = surroundItemAttributeProperty.FindPropertyRelative("summonedSurroundingType");
        if (summonedTypeProperty != null)
            summonedTypeProperty.enumValueIndex = 0; // 默认环绕物种类

        SerializedProperty countProperty = surroundItemAttributeProperty.FindPropertyRelative("surroundingCount");
        if (countProperty != null)
            countProperty.intValue = 1; // 默认环绕物数量

        SerializedProperty speedProperty = surroundItemAttributeProperty.FindPropertyRelative("surroundingSpeedPercent");
        if (speedProperty != null)
            speedProperty.floatValue = 1.0f; // 默认加速百分比
            
        SerializedProperty prefabProperty = surroundItemAttributeProperty.FindPropertyRelative("surroundingPrefab");
        if (prefabProperty != null)
            prefabProperty.objectReferenceValue = null; // 默认无预制体
            
        SerializedProperty itemShapeProperty = newItem.FindPropertyRelative("itemShape");
        if (itemShapeProperty != null)
            itemShapeProperty.enumValueIndex = 0; // 默认形状
            
        SerializedProperty itemDirectionProperty = newItem.FindPropertyRelative("itemDirection");
        if (itemDirectionProperty != null)
            itemDirectionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加环绕物配置: {type}");
    }
    
    /// <summary>
    /// 添加新的环绕物配置
    /// </summary>
    private void AddOtherConfig(SerializedProperty otherAttributes, Assets.BagBattles.Types.OtherType type)
    {
        serializedObject.Update();

        // 增加数组元素
        otherAttributes.arraySize++;
        SerializedProperty newItem = otherAttributes.GetArrayElementAtIndex(otherAttributes.arraySize - 1);

        // 设置环绕物类型
        SerializedProperty otherItemAttributeProperty = newItem.FindPropertyRelative("otherItemAttribute");
        SerializedProperty typeProperty = otherItemAttributeProperty.FindPropertyRelative("specificOtherType");
        typeProperty.enumValueIndex = (int)type;

        SerializedProperty itemShapeProperty = newItem.FindPropertyRelative("itemShape");
        if (itemShapeProperty != null)
            itemShapeProperty.enumValueIndex = 0; // 默认形状

        SerializedProperty itemDirectionProperty = newItem.FindPropertyRelative("itemDirection");
        if (itemDirectionProperty != null)
            itemDirectionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加环绕物配置: {type}");
    }

    /// <summary>
    /// 添加新的开火触发器配置
    /// </summary>
    private void AddFireTriggerConfig(SerializedProperty fireTriggerAttributes, Assets.BagBattles.Types.FireTriggerType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        fireTriggerAttributes.arraySize++;
        SerializedProperty newItem = fireTriggerAttributes.GetArrayElementAtIndex(fireTriggerAttributes.arraySize - 1);

        // 设置触发器类型
        SerializedProperty attributeProperty = newItem.FindPropertyRelative("fireCountTriggerAttribute");
        SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("fireTriggerType");
        typeProperty.enumValueIndex = (int)type;

        // 设置其他默认属性
        SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
        if (rangeProperty != null)
            rangeProperty.enumValueIndex = 0; // 默认触发范围
            
        SerializedProperty countProperty = attributeProperty.FindPropertyRelative("fireCount");
        if (countProperty != null)
            countProperty.intValue = 1; // 默认开火次数
            
        SerializedProperty shapeProperty = newItem.FindPropertyRelative("fireTriggerShape");
        if (shapeProperty != null)
            shapeProperty.enumValueIndex = 0; // 默认形状
            
        SerializedProperty directionProperty = newItem.FindPropertyRelative("fireTriggerDirection");
        if (directionProperty != null)
            directionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加开火触发器配置: {type}");
    }

    /// <summary>
    /// 添加新的时间触发器配置
    /// </summary>
    private void AddTimeTriggerConfig(SerializedProperty timeTriggerAttributes, Assets.BagBattles.Types.TimeTriggerType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        timeTriggerAttributes.arraySize++;
        SerializedProperty newItem = timeTriggerAttributes.GetArrayElementAtIndex(timeTriggerAttributes.arraySize - 1);

        // 设置触发器类型
        SerializedProperty attributeProperty = newItem.FindPropertyRelative("timeTriggerAttribute");
        SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("timeTriggerType");
        typeProperty.enumValueIndex = (int)type;

        // 设置其他默认属性
        SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
        if (rangeProperty != null)
            rangeProperty.enumValueIndex = 0; // 默认触发范围
            
        SerializedProperty timeProperty = attributeProperty.FindPropertyRelative("triggerTime");
        if (timeProperty != null)
            timeProperty.floatValue = 1.0f; // 默认触发时间
            
        SerializedProperty shapeProperty = newItem.FindPropertyRelative("timeTriggerShape");
        if (shapeProperty != null)
            shapeProperty.enumValueIndex = 0; // 默认形状
            
        SerializedProperty directionProperty = newItem.FindPropertyRelative("timeTriggerDirection");
        if (directionProperty != null)
            directionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加时间触发器配置: {type}");
    }

    private void AddByOtherTriggerConfig(SerializedProperty byOtherTriggerAttributes, Assets.BagBattles.Types.ByOtherTriggerType type)
    {
        serializedObject.Update();
        
        // 增加数组元素
        byOtherTriggerAttributes.arraySize++;
        SerializedProperty newItem = byOtherTriggerAttributes.GetArrayElementAtIndex(byOtherTriggerAttributes.arraySize - 1);

        // 设置触发器类型
        SerializedProperty attributeProperty = newItem.FindPropertyRelative("byOtherTriggerAttribute");
        SerializedProperty typeProperty = attributeProperty.FindPropertyRelative("byOtherTriggerType");
        typeProperty.enumValueIndex = (int)type;

        // 设置其他默认属性
        SerializedProperty rangeProperty = attributeProperty.FindPropertyRelative("triggerRange");
        if (rangeProperty != null)
            rangeProperty.enumValueIndex = 0; // 默认触发范围
            
        SerializedProperty countProperty = attributeProperty.FindPropertyRelative("requiredTriggerCount");
        if (countProperty != null)
            countProperty.intValue = 1; // 默认次数
            
        SerializedProperty shapeProperty = newItem.FindPropertyRelative("byOtherTriggerShape");
        if (shapeProperty != null)
            shapeProperty.enumValueIndex = 0; // 默认形状
            
        SerializedProperty directionProperty = newItem.FindPropertyRelative("byOtherTriggerDirection");
        if (directionProperty != null)
            directionProperty.enumValueIndex = 0; // 默认方向

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"已添加触发器配置: {type}");
    }

    /// <summary>
    /// 通用方法：查找指定类型在属性数组中的索引
    /// </summary>
    private int FindTypeIndex(SerializedProperty arrayProperty, string attributePropertyName, string typePropertyName, int typeValue)
    {
        if (arrayProperty == null) return -1;

        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            SerializedProperty itemProperty = arrayProperty.GetArrayElementAtIndex(i);
            if (itemProperty == null) continue;

            SerializedProperty attributeProperty = itemProperty.FindPropertyRelative(attributePropertyName);
            if (attributeProperty == null) continue;

            SerializedProperty typeProperty = attributeProperty.FindPropertyRelative(typePropertyName);
            if (typeProperty == null) continue;

            if (typeProperty.enumValueIndex == typeValue)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 将ItemAttribute数据导出到JSON文件
    /// </summary>
    private void ExportToJson(ItemAttribute asset)
    {
        try
        {
            // 创建数据对象
            ItemAttributeData data = new ItemAttributeData
            {
                triggerAttribute = asset.triggerAttribute,
                bulletAttribute = asset.bulletAttribute,
                foodAttribute = asset.foodAttribute,
                surroundAttribute = asset.surroundAttribute,
                otherAttribute = asset.otherAttribute,
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

            AssetDatabase.Refresh();
            Debug.Log($"已成功导出道具属性到JSON文件: {filePath}");
            EditorUtility.DisplayDialog("导出成功", $"已成功导出道具属性到JSON文件:\n{filePath}", "确定");
        }
        catch (Exception ex)
        {
            Debug.LogError($"导出到JSON文件失败: {ex.Message}");
            EditorUtility.DisplayDialog("导出失败", $"导出到JSON文件失败:\n{ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 从JSON文件导入ItemAttribute数据
    /// </summary>
    private void ImportFromJson(ItemAttribute asset)
    {
        // 检查JSON文件是否存在
        string filePath = Path.Combine(Application.dataPath, "Resources/ItemAttributes/ItemAttributeConfig.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"找不到JSON文件: {filePath}");
            EditorUtility.DisplayDialog("导入失败", $"找不到JSON文件:\n{filePath}", "确定");
            return;
        }

        try
        {
            // 读取JSON文件
            string jsonData = File.ReadAllText(filePath);

            // 从JSON加载数据
            ItemAttributeData data = JsonUtility.FromJson<ItemAttributeData>(jsonData);

            // 更新数据
            asset.triggerAttribute = data.triggerAttribute;
            asset.bulletAttribute = data.bulletAttribute;
            asset.foodAttribute = data.foodAttribute;
            asset.surroundAttribute = data.surroundAttribute;
            asset.otherAttribute = data.otherAttribute;

            // 标记为已修改
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            Debug.Log($"已成功从JSON文件加载道具属性: {filePath}");
            EditorUtility.DisplayDialog("导入成功", "已成功从JSON文件加载道具属性", "确定");
        }
        catch (Exception ex)
        {
            Debug.LogError($"从JSON加载失败: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("导入失败", $"从JSON加载失败:\n{ex.Message}", "确定");
        }
    }
    
    #endregion
}