using UnityEditor;
using UnityEngine;

/*
添加新属性：
// 在类的顶部添加新属性
private float yourNewProperty = 0f;
private int anotherProperty = 0;

// 在switch语句中添加新case
case Trigger.TriggerType.YourNewType:
    EditorGUI.BeginChangeCheck();
    yourNewProperty = EditorGUILayout.FloatField("你的新属性", yourNewProperty);
    anotherProperty = EditorGUILayout.IntField("另一个属性", anotherProperty);
    specificPropertyChanged = EditorGUI.EndChangeCheck();
    break;

// 在UpdateTriggerAttribute中添加新case
case Trigger.TriggerType.YourNewType:
    var newTypeAttr = new Trigger.YourNewTriggerAttribute();
    newTypeAttr.yourNewProperty = yourNewProperty;
    newTypeAttr.anotherProperty = anotherProperty;
    newAttribute = newTypeAttr;
    break;

// 在InitializeCurrentValues中添加新类型处理
else if (baseAttr is Trigger.YourNewTriggerAttribute newTypeAttr)
{
    yourNewProperty = newTypeAttr.yourNewProperty;
    anotherProperty = newTypeAttr.anotherProperty;
}
*/

[CustomEditor(typeof(InventoryTriggerItem))]
public class InventoryTriggerItemEditor : Editor
{
    private SerializedProperty triggerItemAttributeProp;
    private SerializedProperty inventoryItemProp;
    private SerializedProperty triggerItemsProp;

    // 当前选择的触发器类型
    private Trigger.TriggerType currentTriggerType;
    // 当前选择的触发范围
    private Trigger.TriggerRange currentTriggerRange;
    
    // 用于时间触发器的属性
    private float triggerTime = 1.0f;
    
    // 用于开火次数触发器的属性
    private int triggerFireCount = 3;

    // 触发器属性折叠状态
    private bool showTriggerConfigFoldout = true;
    private bool showItemReferenceFoldout = true;
    private bool showTriggerListFoldout = true;

    private void OnEnable()
    {
        triggerItemAttributeProp = serializedObject.FindProperty("triggerItemAttribute");
        inventoryItemProp = serializedObject.FindProperty("inventoryItem");
        triggerItemsProp = serializedObject.FindProperty("triggerItems");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        InventoryTriggerItem triggerItem = (InventoryTriggerItem)target;
        
        // 初始化当前值
        InitializeCurrentValues(triggerItem);
        
        // 触发器属性部分
        showTriggerConfigFoldout = EditorGUILayout.Foldout(showTriggerConfigFoldout, "触发器配置", true, EditorStyles.foldoutHeader);
        if (showTriggerConfigFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 触发器类型选择
            EditorGUI.BeginChangeCheck();
            currentTriggerType = (Trigger.TriggerType)EditorGUILayout.EnumPopup("触发器类型", currentTriggerType);
            bool triggerTypeChanged = EditorGUI.EndChangeCheck();
            
            // 触发器范围选择
            EditorGUI.BeginChangeCheck();
            currentTriggerRange = (Trigger.TriggerRange)EditorGUILayout.EnumPopup("触发范围", currentTriggerRange);
            bool triggerRangeChanged = EditorGUI.EndChangeCheck();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("类型特定属性", EditorStyles.boldLabel);
            
            // 根据选择的触发器类型显示不同的属性
            bool specificPropertyChanged = false;
            switch (currentTriggerType)
            {
                case Trigger.TriggerType.ByTime:
                    EditorGUI.BeginChangeCheck();
                    triggerTime = EditorGUILayout.FloatField("触发时间(秒)", triggerTime);
                    specificPropertyChanged = EditorGUI.EndChangeCheck();
                    break;
                    
                case Trigger.TriggerType.ByFireTimes:
                    EditorGUI.BeginChangeCheck();
                    triggerFireCount = EditorGUILayout.IntField("触发开火次数", triggerFireCount);
                    specificPropertyChanged = EditorGUI.EndChangeCheck();
                    break;
            }
            
            // 如果有任何值发生变化，创建新的属性对象
            if (triggerTypeChanged || triggerRangeChanged || specificPropertyChanged)
            {
                UpdateTriggerAttribute(triggerItem);
            }
            
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        
        // 物品引用部分
        showItemReferenceFoldout = EditorGUILayout.Foldout(showItemReferenceFoldout, "物品引用", true, EditorStyles.foldoutHeader);
        if (showItemReferenceFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(inventoryItemProp, new GUIContent("库存物品"));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        // 触发物品列表
        showTriggerListFoldout = EditorGUILayout.Foldout(showTriggerListFoldout, "触发物品列表", true, EditorStyles.foldoutHeader);
        if (showTriggerListFoldout)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 显示触发物品信息（只读）
            if (triggerItem.triggerItems != null && triggerItem.triggerItems.Count > 0)
            {
                EditorGUI.indentLevel++;
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
                EditorGUI.indentLevel--;
                
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
                EditorGUILayout.HelpBox("列表为空。使用DetectItems()方法可更新列表。", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        
        // 检测按钮
        if (GUILayout.Button("检测可触发物品", GUILayout.Height(30)))
        {
            triggerItem.DetectItems();
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void InitializeCurrentValues(InventoryTriggerItem triggerItem)
    {
        if (triggerItem.triggerItemAttribute == null)
        {
            // 如果没有设置触发器属性，创建默认值
            currentTriggerType = Trigger.TriggerType.ByTime;
            currentTriggerRange = Trigger.TriggerRange.SingleCell;
            triggerTime = 1.0f;
            triggerFireCount = 3;
            
            // 创建默认属性
            UpdateTriggerAttribute(triggerItem);
        }
        else
        {
            // 读取当前值
            if (triggerItem.triggerItemAttribute is Trigger.BaseTriggerAttribute baseAttr)
            {
                currentTriggerType = baseAttr.triggerType;
                currentTriggerRange = baseAttr.triggerRange;
                
                // 基于特定类型读取特殊属性
                if (baseAttr is Trigger.TimeTriggerAttribute timeAttr)
                {
                    triggerTime = timeAttr.triggerTime;
                }
                else if (baseAttr is Trigger.FireCountTriggerAttribute fireAttr)
                {
                    triggerFireCount = fireAttr.triggerFireCount;
                }
            }
        }
    }
    
    private void UpdateTriggerAttribute(InventoryTriggerItem triggerItem)
    {
        // 根据当前类型创建适当的属性对象
        Trigger.BaseTriggerAttribute newAttribute = null;
        
        switch (currentTriggerType)
        {
            case Trigger.TriggerType.ByTime:
                var timeAttr = new Trigger.TimeTriggerAttribute();
                timeAttr.triggerTime = triggerTime;
                newAttribute = timeAttr;
                break;
                
            case Trigger.TriggerType.ByFireTimes:
                var fireAttr = new Trigger.FireCountTriggerAttribute();
                fireAttr.triggerFireCount = triggerFireCount;
                newAttribute = fireAttr;
                break;
        }
        
        // 设置共同属性
        if (newAttribute != null)
        {
            newAttribute.triggerType = currentTriggerType;
            newAttribute.triggerRange = currentTriggerRange;
            
            // 更新对象
            triggerItem.triggerItemAttribute = newAttribute;
            EditorUtility.SetDirty(target);
        }
    }
}
