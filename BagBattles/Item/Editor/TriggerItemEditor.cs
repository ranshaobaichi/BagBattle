using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerItem))]
public class TriggerItemEditor : Editor
{
    private SerializedProperty itemTypeProp;
    private SerializedProperty triggerRangeProp;
    private SerializedProperty triggerTypeProp;
    private SerializedProperty triggerTimeProp;
    private SerializedProperty triggerFireCountProp;
    private SerializedProperty inventoryItemProp;
    private SerializedProperty triggerItemsProp;

    private void OnEnable()
    {
        // 基础Item属性
        itemTypeProp = serializedObject.FindProperty("itemType");
        
        // TriggerItem特有属性
        triggerRangeProp = serializedObject.FindProperty("triggerRange");
        triggerTypeProp = serializedObject.FindProperty("triggerType");
        triggerTimeProp = serializedObject.FindProperty("triggerTime");
        triggerFireCountProp = serializedObject.FindProperty("triggerFireCount");
        inventoryItemProp = serializedObject.FindProperty("inventoryItem");
        triggerItemsProp = serializedObject.FindProperty("triggerItems");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制基础Item属性
        GUI.enabled = false; // 禁用物品类型字段，因为对于TriggerItem它总是固定的
        itemTypeProp.enumValueIndex = (int)Item.ItemType.TriggerItem;
        EditorGUILayout.PropertyField(itemTypeProp, new GUIContent("物品类型"));
        GUI.enabled = true;

        EditorGUILayout.Space();
        
        // 使用Box分组显示触发器配置
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("触发器配置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(triggerRangeProp, new GUIContent("触发范围"));
        EditorGUILayout.PropertyField(triggerTypeProp, new GUIContent("触发方式"));
        
        // 根据选择的触发类型显示相应的属性
        TriggerItem.TriggerType selectedType = (TriggerItem.TriggerType)triggerTypeProp.enumValueIndex;
        switch (selectedType)
        {
            case TriggerItem.TriggerType.ByTime:
                EditorGUILayout.PropertyField(triggerTimeProp, new GUIContent("触发时间 (秒)"));
                break;
            
            case TriggerItem.TriggerType.ByFireTimes:
                EditorGUILayout.PropertyField(triggerFireCountProp, new GUIContent("触发次数"));
                break;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        
        // 显示库存物品引用
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("物品引用", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(inventoryItemProp, new GUIContent("库存物品"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 显示可触发物品列表
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("触发物品列表", EditorStyles.boldLabel);
        
        // 只读显示触发物品列表
        if (triggerItemsProp.arraySize > 0)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < triggerItemsProp.arraySize; i++)
            {
                SerializedProperty itemProp = triggerItemsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"物品 {i+1}");
                GUI.enabled = false; // 设为只读
                EditorGUILayout.ObjectField(itemProp.objectReferenceValue, typeof(Item), true);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            
            if (GUILayout.Button("清空列表"))
            {
                if (EditorUtility.DisplayDialog("确认清空", "确定要清空触发物品列表吗?", "确定", "取消"))
                {
                    TriggerItem triggerItem = (TriggerItem)target;
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

        EditorGUILayout.Space();
        
        // 添加触发检测按钮
        if (GUILayout.Button("检测可触发物品", GUILayout.Height(30)))
        {
            TriggerItem triggerItem = (TriggerItem)target;
            triggerItem.DetectItems();
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
