using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TriggerItem), true)]
public class TriggerItemEditor : Editor
{
    private bool showBulletItems = true;
    private bool showFoodItems = true;

    public override void OnInspectorGUI()
    {
        // 获取当前编辑的TriggerItem对象
        TriggerItem triggerItem = (TriggerItem)target;

        // 绘制原有的Inspector内容
        DrawDefaultInspector();

        // 添加分割线
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("触发器物品列表", EditorStyles.boldLabel);

        // 检查字典是否初始化
        if (triggerItem.items == null)
        {
            triggerItem.items = new Dictionary<Item.ItemType, List<Item>>();
            triggerItem.items[Item.ItemType.BulletItem] = new List<Item>();
            triggerItem.items[Item.ItemType.FoodItem] = new List<Item>();
        }

        // 显示子弹物品列表
        if (triggerItem.items.TryGetValue(Item.ItemType.BulletItem, out List<Item> bulletItems))
        {
            showBulletItems = EditorGUILayout.Foldout(showBulletItems, $"子弹物品 ({bulletItems.Count})");
            if (showBulletItems)
            {
                EditorGUI.indentLevel++;
                if (bulletItems.Count == 0)
                {
                    EditorGUILayout.LabelField("没有子弹物品");
                }
                else
                {
                    for (int i = 0; i < bulletItems.Count; i++)
                    {
                        if (bulletItems[i] is BulletItem bulletItem)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField($"子弹 {i + 1}:", EditorStyles.boldLabel);
                            EditorGUILayout.LabelField($"类型: {bulletItem.bulletAttribute.bulletType}");
                            EditorGUILayout.LabelField($"每次发射数量: {bulletItem.bulletAttribute.bulletCount}");
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // 显示食物物品列表
        if (triggerItem.items.TryGetValue(Item.ItemType.FoodItem, out List<Item> foodItems))
        {
            showFoodItems = EditorGUILayout.Foldout(showFoodItems, $"食物物品 ({foodItems.Count})");
            if (showFoodItems)
            {
                EditorGUI.indentLevel++;
                if (foodItems.Count == 0)
                {
                    EditorGUILayout.LabelField("没有食物物品");
                }
                else
                {
                    for (int i = 0; i < foodItems.Count; i++)
                    {
                        if (foodItems[i] is FoodItem foodItem)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField($"食物 {i + 1}:", EditorStyles.boldLabel);
                            
                            // if (foodItem.foodItemAttribute != null && 
                            //     foodItem.foodItemAttribute.foodItemAttributes != null)
                            // {
                            //     foreach (var attr in foodItem.foodItemAttribute.foodItemAttributes)
                            //     {
                            //         EditorGUILayout.LabelField($"加成类型: {attr.foodBonusType}");
                            //         EditorGUILayout.LabelField($"加成值: {attr.foodBonusValue}");
                            //         EditorGUILayout.LabelField($"持续类型: {attr.foodDurationType}");
                            //         EditorGUILayout.LabelField($"持续回合: {attr.timeLeft}");
                            //     }
                            // }
                            
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // 如果在编辑器中修改了数据，标记为已修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}