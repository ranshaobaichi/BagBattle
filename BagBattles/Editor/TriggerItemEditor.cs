using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryTriggerItem))]
public class TriggerItemEditor : Editor
{
    // 当前选择的触发器类型
    private Trigger.TriggerType currentTriggerType = Trigger.TriggerType.ByTime;
    
    // 当前选择的触发范围
    private Trigger.TriggerRange currentTriggerRange = Trigger.TriggerRange.SingleCell;
    
    // 时间触发属性
    private float triggerTime = 1.0f;
    private float duration = 0f;
    
    // 开火次数触发属性
    private int triggerFireCount = 3;
    private bool resetAfterTrigger = true;
    
    // UI展示状态
    private bool showTriggerConfigFoldout = true;
    private bool showItemReferenceFoldout = true;
    private bool showTriggerListFoldout = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        InventoryTriggerItem triggerItem = (InventoryTriggerItem)target;
        
        // 初始化当前值
        InitializeCurrentValues(triggerItem);
        
        // ===== 触发器配置部分 =====
        EditorGUILayout.Space(5);
        showTriggerConfigFoldout = EditorGUILayout.Foldout(showTriggerConfigFoldout, "触发器配置", true);
        if (showTriggerConfigFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 触发器类型选择
            EditorGUI.BeginChangeCheck();
            currentTriggerType = (Trigger.TriggerType)EditorGUILayout.EnumPopup(
                new GUIContent("触发器类型", "决定该触发器如何被激活"), 
                currentTriggerType);
            bool triggerTypeChanged = EditorGUI.EndChangeCheck();
            
            // 触发器范围选择
            EditorGUI.BeginChangeCheck();
            currentTriggerRange = (Trigger.TriggerRange)EditorGUILayout.EnumPopup(
                new GUIContent("触发范围", "触发器影响的网格范围"), 
                currentTriggerRange);
            bool triggerRangeChanged = EditorGUI.EndChangeCheck();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("类型特定属性", EditorStyles.boldLabel);
            
            bool specificPropertyChanged = false;
            
            // 根据触发器类型显示不同属性
            switch (currentTriggerType)
            {
                case Trigger.TriggerType.ByTime:
                    EditorGUI.BeginChangeCheck();
                    
                    triggerTime = EditorGUILayout.FloatField(
                        new GUIContent("触发间隔(秒)", "触发器每隔多少秒触发一次"), 
                        triggerTime);
                    
                    duration = EditorGUILayout.FloatField(
                        new GUIContent("持续时间(秒)", "触发器持续多长时间，0表示永久"), 
                        duration);
                    
                    specificPropertyChanged = EditorGUI.EndChangeCheck();
                    break;
                    
                case Trigger.TriggerType.ByFireTimes:
                    EditorGUI.BeginChangeCheck();
                    
                    triggerFireCount = EditorGUILayout.IntField(
                        new GUIContent("触发所需开火次数", "需要多少次开火才会触发"), 
                        triggerFireCount);
                    
                    resetAfterTrigger = EditorGUILayout.Toggle(
                        new GUIContent("触发后重置计数", "触发后是否重置开火计数"), 
                        resetAfterTrigger);
                    
                    specificPropertyChanged = EditorGUI.EndChangeCheck();
                    break;
            }
            
            // 如果有值变化，更新触发器属性
            if (triggerTypeChanged || triggerRangeChanged || specificPropertyChanged)
            {
                UpdateTriggerAttribute(triggerItem);
            }
            
            EditorGUILayout.EndVertical();
        }

        // ===== 物品引用部分 =====
        EditorGUILayout.Space(5);
        showItemReferenceFoldout = EditorGUILayout.Foldout(showItemReferenceFoldout, "物品引用", true);
        if (showItemReferenceFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SerializedProperty inventoryItemProp = serializedObject.FindProperty("inventoryItem");
            EditorGUILayout.PropertyField(inventoryItemProp, new GUIContent("库存物品"));
            EditorGUILayout.EndVertical();
        }

        // ===== 触发物品列表部分 =====
        EditorGUILayout.Space(5);
        showTriggerListFoldout = EditorGUILayout.Foldout(showTriggerListFoldout, "触发物品列表", true);
        if (showTriggerListFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (triggerItem.triggerItems != null && triggerItem.triggerItems.Count > 0)
            {
                foreach (var itemType in triggerItem.triggerItems.Keys)
                {
                    EditorGUILayout.LabelField($"物品类型: {itemType} (数量: {triggerItem.triggerItems[itemType].Count})");
                    
                    EditorGUI.indentLevel++;
                    foreach (var item in triggerItem.triggerItems[itemType])
                    {
                        EditorGUILayout.LabelField($"- {item?.ToString() ?? "null"}");
                    }
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.Space(5);
                }
                
                if (GUILayout.Button("清空列表"))
                {
                    if (EditorUtility.DisplayDialog("确认清空", "确定要清空触发物品列表吗?", "确定", "取消"))
                    {
                        triggerItem.triggerItems.Clear();
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("列表为空。使用检测按钮可更新可触发物品列表。", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        // ===== 操作按钮 =====
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("检测可触发物品", GUILayout.Height(30)))
        {
            triggerItem.DetectItems();
            EditorUtility.SetDirty(target);
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }

    private void InitializeCurrentValues(InventoryTriggerItem triggerItem)
    {
        if (triggerItem.triggerItemAttribute == null)
        {
            // 设置默认值
            currentTriggerType = Trigger.TriggerType.ByTime;
            currentTriggerRange = Trigger.TriggerRange.SingleCell;
            triggerTime = 1.0f;
            duration = 0f;
            triggerFireCount = 3;
            resetAfterTrigger = true;
            
            // 创建默认属性
            UpdateTriggerAttribute(triggerItem);
        }
        else
        {
            // 读取现有值
            currentTriggerType = triggerItem.triggerItemAttribute.triggerType;
            currentTriggerRange = triggerItem.triggerItemAttribute.triggerRange;
            
            // 读取特定类型属性
            if (triggerItem.triggerItemAttribute is Trigger.TimeTriggerAttribute timeAttr)
            {
                triggerTime = timeAttr.triggerTime;
                duration = timeAttr.duration;
            }
            else if (triggerItem.triggerItemAttribute is Trigger.FireCountTriggerAttribute fireAttr)
            {
                triggerFireCount = fireAttr.triggerFireCount;
                resetAfterTrigger = fireAttr.resetAfterTrigger;
            }
            else if (triggerItem.triggerItemAttribute is Trigger.TriggerItemAttribute legacyAttr)
            {
                // 处理兼容旧数据
                triggerTime = legacyAttr.triggerTime;
                triggerFireCount = legacyAttr.triggerFireCount;
            }
        }
    }
    
    private void UpdateTriggerAttribute(InventoryTriggerItem triggerItem)
    {
        // 创建对应类型的属性对象
        Trigger.BaseTriggerAttribute newAttribute = null;
        
        switch (currentTriggerType)
        {
            case Trigger.TriggerType.ByTime:
                var timeAttr = new Trigger.TimeTriggerAttribute();
                timeAttr.triggerTime = triggerTime;
                timeAttr.duration = duration;
                newAttribute = timeAttr;
                break;
                
            case Trigger.TriggerType.ByFireTimes:
                var fireAttr = new Trigger.FireCountTriggerAttribute();
                fireAttr.triggerFireCount = triggerFireCount;
                fireAttr.resetAfterTrigger = resetAfterTrigger;
                newAttribute = fireAttr;
                break;
        }
        
        // 设置通用属性
        if (newAttribute != null)
        {
            newAttribute.triggerRange = currentTriggerRange;
            
            // 更新组件
            triggerItem.triggerItemAttribute = newAttribute;
            EditorUtility.SetDirty(target);
        }
    }
}