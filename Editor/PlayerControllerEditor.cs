using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private bool showTemporarySpeedBonus = true;
    private bool showTriggerItems = true;
    
    // 上次检测到的加成和触发器数量，用于变化高亮
    private int lastSpeedBonusCount = 0;
    private int lastTriggerItemsCount = 0;
    
    // 高亮计时器
    private float[] highlightTimers = new float[2]; // [0]速度加成，[1]触发器
    private Color highlightColor = new Color(1f, 0.8f, 0.2f);
    
    // 添加实时更新支持
    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }
    
    // 编辑器更新函数，用于实时刷新
    private void OnEditorUpdate()
    {
        if (Application.isPlaying && target != null)
        {
            // 检测加成和触发器变化并更新高亮计时器
            PlayerController playerController = (PlayerController)target;
            CheckChanges(playerController);
            
            // 递减高亮计时器
            for (int i = 0; i < highlightTimers.Length; i++)
            {
                if (highlightTimers[i] > 0)
                    highlightTimers[i] -= Time.deltaTime;
            }
            
            Repaint();
        }
    }
    
    // 检测变化
    private void CheckChanges(PlayerController playerController)
    {
        // 使用反射获取私有字段
        var speedBonusField = typeof(PlayerController).GetField("temporary_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
            
        int currentSpeedBonusCount = (speedBonusField?.GetValue(playerController) as LinkedList<Food.Bonus>)?.Count ?? 0;
        int currentTriggerItemsCount = playerController.triggerItems?.Count ?? 0;
        
        // 检测变化并设置高亮
        if (currentSpeedBonusCount != lastSpeedBonusCount)
        {
            highlightTimers[0] = 3f; // 高亮3秒
            lastSpeedBonusCount = currentSpeedBonusCount;
            showTemporarySpeedBonus = true; // 自动展开被修改的列表
        }
        
        if (currentTriggerItemsCount != lastTriggerItemsCount)
        {
            highlightTimers[1] = 3f;
            lastTriggerItemsCount = currentTriggerItemsCount;
            showTriggerItems = true;
        }
    }
    
    public override void OnInspectorGUI()
    {
        PlayerController playerController = (PlayerController)target;
        
        // 角色状态信息
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("角色状态", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("存活状态:", GUILayout.Width(80));
        if (Application.isPlaying)
        {
            bool isAlive = playerController.Live();
            GUI.color = isAlive ? Color.green : Color.red;
            EditorGUILayout.LabelField(isAlive ? "存活" : "死亡");
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.LabelField("未在运行");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // 绘制原始Inspector
        DrawDefaultInspector();
        
        // 添加触发器状态查看器
        EditorGUILayout.Space(10);
        
        // 使用反射获取私有字段
        var speedBonusField = typeof(PlayerController).GetField("temporary_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var speedBonusSumField = typeof(PlayerController).GetField("temporary_speed_bonus_sum", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // 触发器状态显示
        if (highlightTimers[1] > 0)
            GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[1] / 3f);
            
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showTriggerItems = EditorGUILayout.Foldout(showTriggerItems, $"触发器 ({playerController.triggerItems?.Count ?? 0}项)");
        GUI.backgroundColor = Color.white;
        
        if (showTriggerItems && playerController.triggerItems != null && playerController.triggerItems.Count > 0)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < playerController.triggerItems.Count; i++)
            {
                TriggerItem trigger = playerController.triggerItems[i];
                if (trigger != null)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"#{i}", GUILayout.Width(30));
                    EditorGUILayout.LabelField($"类型: {trigger.GetType().Name}");
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }
        else if (showTriggerItems)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("(没有触发器)");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        // 临时加成状态显示
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("临时加成状态查看器", EditorStyles.boldLabel);
        
        // 显示总加成信息
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("速度加成总和:", GUILayout.Width(80));
        EditorGUILayout.LabelField($"{speedBonusSumField?.GetValue(playerController) ?? 0}");
        EditorGUILayout.EndHorizontal();
        
        // 显示速度加成列表
        if (speedBonusField != null)
        {
            var bonusList = speedBonusField.GetValue(playerController) as LinkedList<Food.Bonus>;
            
            if (highlightTimers[0] > 0)
                GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[0] / 3f);
                
            showTemporarySpeedBonus = EditorGUILayout.Foldout(showTemporarySpeedBonus, $"临时速度加成 ({bonusList?.Count ?? 0}项)");
            GUI.backgroundColor = Color.white;
            
            if (showTemporarySpeedBonus)
            {
                DrawBonusList(bonusList, "速度加成");
            }
        }
        
        EditorGUILayout.EndVertical();
        
        // 添加清除按钮
        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f);
        if (GUILayout.Button("清除所有临时速度加成", GUILayout.Height(30)))
        {
            // 需要在 PlayerController 中添加清除方法
            InvokePlayerControllerMethod(playerController, "DecreaseTemporaryBonus");
            EditorUtility.SetDirty(playerController);
        }
        
        // 清除所有触发器按钮
        if (playerController.triggerItems != null && playerController.triggerItems.Count > 0)
        {
            if (GUILayout.Button("清除所有触发器", GUILayout.Height(30)))
            {
                playerController.DestroyAllTriggers();
                EditorUtility.SetDirty(playerController);
            }
        }
        
        GUI.backgroundColor = Color.white;
    }
    
    // 绘制加成列表的辅助方法
    private void DrawBonusList(LinkedList<Food.Bonus> bonusList, string label)
    {
        if (bonusList == null || bonusList.Count == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("(没有加成)");
            EditorGUI.indentLevel--;
            return;
        }
        
        EditorGUI.indentLevel++;
        
        int index = 0;
        foreach (var bonus in bonusList)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"#{index}", GUILayout.Width(30));
            EditorGUILayout.LabelField($"{label}: {bonus.bonusValue}", GUILayout.Width(150));
            
            // 用颜色标记剩余回合数
            if (bonus.timeLeft <= 1)
                GUI.color = Color.red;
            else if (bonus.timeLeft <= 2)
                GUI.color = Color.yellow;
                
            EditorGUILayout.LabelField($"剩余回合: {bonus.timeLeft}");
            GUI.color = Color.white;
            
            EditorGUILayout.EndHorizontal();
            index++;
        }
        
        EditorGUI.indentLevel--;
    }
    
    // 通过反射调用 PlayerController 的方法
    private void InvokePlayerControllerMethod(PlayerController playerController, string methodName)
    {
        var method = typeof(PlayerController).GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
        if (method != null)
            method.Invoke(playerController, null);
        else
            Debug.LogError($"无法找到方法 {methodName} 在 PlayerController 类中");
    }
}