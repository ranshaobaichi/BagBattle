using UnityEditor;
using UnityEngine;
using Assets.BagBattles.Types;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor
{
    private Item.ItemType selectedItemType = Item.ItemType.None;

    // 编辑器中的临时变量
    private Trigger.TriggerType selectedTriggerType;
    private FireTriggerType selectedFireTriggerType;
    private TimeTriggerType selectedTimeTriggerType;
    private bool showTriggerOptions = false;

    // 子弹类型
    private BulletType selectedBulletType = BulletType.None;
    private bool showBulletOptions = false;
    
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 获取目标对象
        InventoryManager inventoryManager = (InventoryManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("测试掉落物品", EditorStyles.boldLabel);

        // 物品类型选择
        selectedItemType = (Item.ItemType)EditorGUILayout.EnumPopup("物品类型", selectedItemType);

        // 根据物品类型显示不同选项
        switch (selectedItemType)
        {
            case Item.ItemType.TriggerItem:
                showTriggerOptions = EditorGUILayout.Foldout(showTriggerOptions, "触发器选项");
                if (showTriggerOptions)
                {
                    EditorGUI.indentLevel++;
                    selectedTriggerType = (Trigger.TriggerType)EditorGUILayout.EnumPopup("触发器类型", selectedTriggerType);

                    // 根据触发器类型显示不同选项
                    switch (selectedTriggerType)
                    {
                        case Trigger.TriggerType.ByFireTimes:
                            selectedFireTriggerType = (FireTriggerType)EditorGUILayout.EnumPopup("开火触发器类型", selectedFireTriggerType);
                            break;
                        case Trigger.TriggerType.ByTime:
                            // 添加其他触发器类型的选项
                            selectedTimeTriggerType = (TimeTriggerType)EditorGUILayout.EnumPopup("时间触发器类型", selectedTimeTriggerType);
                            break;
                        default:
                            EditorGUILayout.HelpBox("请选择有效的触发器类型", MessageType.Warning);
                            break;
                    }
                    EditorGUI.indentLevel--;
                }
                break;
            case Item.ItemType.BulletItem:
                showBulletOptions = EditorGUILayout.Foldout(showBulletOptions, "子弹选项");
                if (showBulletOptions)
                {
                    EditorGUI.indentLevel++;
                    selectedBulletType = (BulletType)EditorGUILayout.EnumPopup("子弹类型", selectedBulletType);

                    // 通用处理方式
                    if (selectedBulletType == BulletType.None)
                    {
                        EditorGUILayout.HelpBox("请选择子弹类型", MessageType.Warning);
                    }
                    EditorGUI.indentLevel--;
                }
                break;
            case Item.ItemType.FoodItem:
                EditorGUILayout.HelpBox("此类型物品尚未实现掉落功能", MessageType.Info);
                break;
        }

        EditorGUILayout.Space(5);

        // 创建测试按钮
        GUI.enabled = showBulletOptions || showTriggerOptions;
        if (GUILayout.Button("生成选定物品", GUILayout.Height(30)))
        {
            // 根据选择的类型调用 DropItems
            switch (selectedItemType)
            {
                case Item.ItemType.TriggerItem:
                    object functionType = null;
                    object specificType = null;
                    functionType = selectedTriggerType;
                    specificType = selectedTriggerType switch
                    {
                        Trigger.TriggerType.ByFireTimes => selectedFireTriggerType,
                        Trigger.TriggerType.ByTime => selectedTimeTriggerType,
                        _ => null,
                    };
                    Debug.Log("生成触发器物品, functionType: " + functionType + ", specificType: " + specificType);
                    if (specificType == null)
                    {
                        Debug.LogError("请选择触发器类型");
                        break;
                    }
                    inventoryManager.DropItem(selectedItemType, functionType, specificType);
                    break;
                case Item.ItemType.BulletItem:
                    if(selectedBulletType != BulletType.None)
                    {
                        inventoryManager.DropItem(selectedItemType, selectedBulletType);
                    }
                    else
                    {
                        Debug.LogError("请选择子弹类型");
                    }
                    break;
                case Item.ItemType.FoodItem:
                    EditorGUILayout.HelpBox("Food物品尚未实现掉落功能", MessageType.Info);
                    break;
                default:
                    Debug.LogError("未实现的物品类型");
                    break;
            }

        }

        GUI.enabled = true;
    }
}