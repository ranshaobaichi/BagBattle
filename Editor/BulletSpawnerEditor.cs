using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(BulletSpawner))]
public class BulletSpawnerEditor : Editor
{
    private bool showDamageAddBonus = true;
    private bool showDamagePercentageBonus = true;
    private bool showAttackSpeedBonus = true;
    private bool showLoadSpeedBonus = true;
    
    // 上次检测到的各类加成数量，用于变化高亮
    private int lastDamageAddCount = 0;
    private int lastDamagePercentCount = 0;
    private int lastAttackSpeedCount = 0;
    private int lastLoadSpeedCount = 0;
    
    // 高亮计时器
    private float[] highlightTimers = new float[4];
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
            // 检测加成变化并更新高亮计时器
            BulletSpawner bulletSpawner = (BulletSpawner)target;
            CheckBonusChanges(bulletSpawner);
            
            // 递减高亮计时器
            for (int i = 0; i < highlightTimers.Length; i++)
            {
                if (highlightTimers[i] > 0)
                    highlightTimers[i] -= Time.deltaTime;
            }
            
            Repaint();
        }
    }
    
    // 检测加成变化
    private void CheckBonusChanges(BulletSpawner bulletSpawner)
    {
        var damageAddBonusField = typeof(BulletSpawner).GetField("temporary_damage_add_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var damagePercentBonusField = typeof(BulletSpawner).GetField("temporary_damage_percentage_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var attackSpeedBonusField = typeof(BulletSpawner).GetField("temporary_attack_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var loadSpeedBonusField = typeof(BulletSpawner).GetField("temporary_load_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
            
        int currentDamageAddCount = (damageAddBonusField?.GetValue(bulletSpawner) as LinkedList<Food.Bonus>)?.Count ?? 0;
        int currentDamagePercentCount = (damagePercentBonusField?.GetValue(bulletSpawner) as LinkedList<Food.Bonus>)?.Count ?? 0;
        int currentAttackSpeedCount = (attackSpeedBonusField?.GetValue(bulletSpawner) as LinkedList<Food.Bonus>)?.Count ?? 0;
        int currentLoadSpeedCount = (loadSpeedBonusField?.GetValue(bulletSpawner) as LinkedList<Food.Bonus>)?.Count ?? 0;
        
        // 检测变化并设置高亮
        if (currentDamageAddCount != lastDamageAddCount)
        {
            highlightTimers[0] = 3f; // 高亮3秒
            lastDamageAddCount = currentDamageAddCount;
            showDamageAddBonus = true; // 自动展开被修改的列表
        }
        
        if (currentDamagePercentCount != lastDamagePercentCount)
        {
            highlightTimers[1] = 3f;
            lastDamagePercentCount = currentDamagePercentCount;
            showDamagePercentageBonus = true;
        }
        
        if (currentAttackSpeedCount != lastAttackSpeedCount)
        {
            highlightTimers[2] = 3f;
            lastAttackSpeedCount = currentAttackSpeedCount;
            showAttackSpeedBonus = true;
        }
        
        if (currentLoadSpeedCount != lastLoadSpeedCount)
        {
            highlightTimers[3] = 3f;
            lastLoadSpeedCount = currentLoadSpeedCount;
            showLoadSpeedBonus = true;
        }
    }
    
    public override void OnInspectorGUI()
    {
        BulletSpawner bulletSpawner = (BulletSpawner)target;
        
        // 添加时间标签，便于验证更新是否实时
        if (Application.isPlaying)
        {
            // 添加触发器状态信息
            bool hasActiveTriggers = PlayerController.Instance != null && PlayerController.Instance.triggerItems.Count > 0;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("触发器状态:", GUILayout.Width(80));
            if (hasActiveTriggers)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField($"活跃 ({PlayerController.Instance.triggerItems.Count}个)");
            }
            else
            {
                GUI.color = Color.gray;
                EditorGUILayout.LabelField("无活跃触发器");
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        
        // 绘制原始Inspector
        DrawDefaultInspector();
        
        // 添加加成状态查看器
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("临时加成状态查看器", EditorStyles.boldLabel);
        
        // 使用反射获取私有字段
        var damageAddBonusField = typeof(BulletSpawner).GetField("temporary_damage_add_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var damagePercentBonusField = typeof(BulletSpawner).GetField("temporary_damage_percentage_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var attackSpeedBonusField = typeof(BulletSpawner).GetField("temporary_attack_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var loadSpeedBonusField = typeof(BulletSpawner).GetField("temporary_load_speed_bonus", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // 获取和显示加成总和
        var addDamageSumField = typeof(BulletSpawner).GetField("temporary_damage_add_bonus_sum", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var percentDamageSumField = typeof(BulletSpawner).GetField("temporary_damage_percentage_bonus_sum", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var attackSpeedSumField = typeof(BulletSpawner).GetField("temporary_attack_speed_bonus_sum", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var loadSpeedSumField = typeof(BulletSpawner).GetField("temporary_load_speed_bonus_sum", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 显示总加成信息
        EditorGUILayout.LabelField("当前临时加成总值:", EditorStyles.boldLabel);
        DrawBonusSummary("伤害加值:", addDamageSumField?.GetValue(bulletSpawner));
        DrawBonusSummary("伤害百分比:", percentDamageSumField?.GetValue(bulletSpawner));
        DrawBonusSummary("攻击速度:", attackSpeedSumField?.GetValue(bulletSpawner));
        DrawBonusSummary("装载速度:", loadSpeedSumField?.GetValue(bulletSpawner));
        
        EditorGUILayout.Space(5);
        
        // 显示伤害加法加成
        if (damageAddBonusField != null)
        {
            var bonusList = damageAddBonusField.GetValue(bulletSpawner) as LinkedList<Food.Bonus>;
            
            // 应用高亮
            if (highlightTimers[0] > 0)
                GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[0] / 3f);
                
            showDamageAddBonus = EditorGUILayout.Foldout(showDamageAddBonus, $"临时伤害加法加成 ({bonusList?.Count ?? 0}项)");
            GUI.backgroundColor = Color.white;
            
            if (showDamageAddBonus)
            {
                DrawBonusList(bonusList, "伤害加成值");
            }
        }
        
        // 显示伤害百分比加成
        if (damagePercentBonusField != null)
        {
            var bonusList = damagePercentBonusField.GetValue(bulletSpawner) as LinkedList<Food.Bonus>;
            
            if (highlightTimers[1] > 0)
                GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[1] / 3f);
                
            showDamagePercentageBonus = EditorGUILayout.Foldout(showDamagePercentageBonus, $"临时伤害百分比加成 ({bonusList?.Count ?? 0}项)");
            GUI.backgroundColor = Color.white;
            
            if (showDamagePercentageBonus)
            {
                DrawBonusList(bonusList, "伤害百分比");
            }
        }
        
        // 显示攻击速度加成
        if (attackSpeedBonusField != null)
        {
            var bonusList = attackSpeedBonusField.GetValue(bulletSpawner) as LinkedList<Food.Bonus>;
            
            if (highlightTimers[2] > 0)
                GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[2] / 3f);
                
            showAttackSpeedBonus = EditorGUILayout.Foldout(showAttackSpeedBonus, $"临时攻击速度加成 ({bonusList?.Count ?? 0}项)");
            GUI.backgroundColor = Color.white;
            
            if (showAttackSpeedBonus)
            {
                DrawBonusList(bonusList, "攻击速度加成");
            }
        }
        
        // 显示装载速度加成
        if (loadSpeedBonusField != null)
        {
            var bonusList = loadSpeedBonusField.GetValue(bulletSpawner) as LinkedList<Food.Bonus>;
            
            if (highlightTimers[3] > 0)
                GUI.backgroundColor = Color.Lerp(Color.white, highlightColor, highlightTimers[3] / 3f);
                
            showLoadSpeedBonus = EditorGUILayout.Foldout(showLoadSpeedBonus, $"临时装载速度加成 ({bonusList?.Count ?? 0}项)");
            GUI.backgroundColor = Color.white;
            
            if (showLoadSpeedBonus)
            {
                DrawBonusList(bonusList, "装载速度加成");
            }
        }
        
        EditorGUILayout.EndVertical();
        
        // 添加清除按钮
        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f); // 轻微的红色背景
        if (GUILayout.Button("清除所有临时加成", GUILayout.Height(30)))
        {
            bulletSpawner.ClearTemporaryBonus();
            EditorUtility.SetDirty(bulletSpawner);
        }
        
        GUI.backgroundColor = Color.white; // 恢复默认背景色
    }
    
    // 绘制加成摘要的辅助方法
    private void DrawBonusSummary(string label, object value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(80));
        EditorGUILayout.LabelField($"{value ?? 0}");
        EditorGUILayout.EndHorizontal();
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
}