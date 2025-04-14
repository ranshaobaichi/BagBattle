#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    private bool[] wavesFoldout;
    private bool[] enemiesFoldout;
    private WavesConfig editorWavesConfig;
    private bool showRawJson = false;
    private Vector2 jsonScrollPosition;
    
    private SerializedProperty wavesConfigFileProperty;
    private SerializedProperty spawnerParentProperty;
    private SerializedProperty enemyPrefabsProperty;
    
    private void OnEnable()
    {
        wavesConfigFileProperty = serializedObject.FindProperty("wavesConfigFile");
        spawnerParentProperty = serializedObject.FindProperty("spawnerParent");
        enemyPrefabsProperty = serializedObject.FindProperty("enemyPrefabs");
        
        InitializeWavesConfig();
    }
    
    private void InitializeWavesConfig()
    {
        if (editorWavesConfig == null)
        {
            WaveManager waveManager = (WaveManager)target;
            TextAsset configFile = wavesConfigFileProperty.objectReferenceValue as TextAsset;
            
            if (configFile != null)
            {
                editorWavesConfig = JsonUtility.FromJson<WavesConfig>(configFile.text);
            }
            else
            {
                editorWavesConfig = new WavesConfig { waves = new List<WaveInfo>() };
            }
            
            InitializeFoldouts();
        }
    }
    
    private void InitializeFoldouts()
    {
        wavesFoldout = new bool[editorWavesConfig.waves.Count];
        enemiesFoldout = new bool[editorWavesConfig.waves.Count];
        
        for (int i = 0; i < wavesFoldout.Length; i++)
        {
            wavesFoldout[i] = false;
            enemiesFoldout[i] = false;
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(wavesConfigFileProperty);
        EditorGUILayout.PropertyField(spawnerParentProperty);
        
        // 增强敌人映射列表的显示
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("敌人类型映射配置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 添加按钮用于添加新映射
        if (GUILayout.Button("添加敌人映射"))
        {
            int index = enemyPrefabsProperty.arraySize;
            enemyPrefabsProperty.InsertArrayElementAtIndex(index);
        }
        
        // 显示当前映射列表
        EditorGUILayout.PropertyField(enemyPrefabsProperty, new GUIContent("敌人类型-预制体映射"), true);
        
        EditorGUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
        
        // 配置文件更改时重新加载
        if (GUI.changed && wavesConfigFileProperty.objectReferenceValue != null)
        {
            TextAsset configFile = wavesConfigFileProperty.objectReferenceValue as TextAsset;
            if (configFile != null)
            {
                editorWavesConfig = JsonUtility.FromJson<WavesConfig>(configFile.text);
                InitializeFoldouts();
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("波次配置编辑器", EditorStyles.boldLabel);
        
        if (editorWavesConfig != null)
        {
            DrawWavesEditor();
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("加载波次配置"))
        {
            LoadWavesConfig();
        }
        
        if (GUILayout.Button("保存波次配置"))
        {
            SaveWavesConfig();
        }
        
        if (GUILayout.Button("新建空配置"))
        {
            CreateEmptyConfig();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        showRawJson = EditorGUILayout.Foldout(showRawJson, "显示/隐藏JSON预览");
        if (showRawJson && editorWavesConfig != null)
        {
            string json = JsonUtility.ToJson(editorWavesConfig, true);
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            jsonScrollPosition = EditorGUILayout.BeginScrollView(jsonScrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(json);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawWavesEditor()
    {
        // 添加新波次按钮
        if (GUILayout.Button("添加新波次"))
        {
            AddNewWave();
        }
        
        EditorGUILayout.Space();
        
        // 显示所有波次
        for (int i = 0; i < editorWavesConfig.waves.Count; i++)
        {
            WaveInfo wave = editorWavesConfig.waves[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 波次标题栏 (带折叠功能)
            EditorGUILayout.BeginHorizontal();
            wavesFoldout[i] = EditorGUILayout.Foldout(wavesFoldout[i], $"波次 {wave.waveId}", true, EditorStyles.foldoutHeader);
            
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("删除波次", $"确定要删除波次 {wave.waveId} 吗?", "确定", "取消"))
                {
                    editorWavesConfig.waves.RemoveAt(i);
                    InitializeFoldouts();
                    GUIUtility.ExitGUI();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 波次详细内容
            if (wavesFoldout[i])
            {
                EditorGUI.indentLevel++;
                
                wave.waveId = EditorGUILayout.IntField("波次ID", wave.waveId);
                wave.startTime = (uint)EditorGUILayout.FloatField("开始时间(秒)", wave.startTime);
                wave.holdTime = EditorGUILayout.FloatField("持续时间(秒)", wave.holdTime);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("敌人配置", EditorStyles.boldLabel);
                
                if (wave.enemies == null)
                {
                    wave.enemies = new List<EnemySpawnData>();
                }
                
                // 添加新敌人配置按钮
                if (GUILayout.Button("添加敌人类型"))
                {
                    AddNewEnemyToWave(wave);
                }
                
                EditorGUILayout.Space();
                
                // 显示波次中的所有敌人配置
                for (int j = 0; j < wave.enemies.Count; j++)
                {
                    EnemySpawnData enemy = wave.enemies[j];
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    string enemyLabel = enemy.enemyType switch
                    {
                        Enemy.EnemyType.NormalCloseCombatEnemy => "普通近战敌人",
                        Enemy.EnemyType.NormalFarCombatEnemy => "普通远程敌人",
                        Enemy.EnemyType.ArmorEnemy => "重甲敌人",
                        Enemy.EnemyType.ExplosionEnemy => "爆炸敌人",
                        Enemy.EnemyType.DivisionEnemy => "分裂敌人",
                        Enemy.EnemyType.DashEnemy => "冲刺敌人",
                        _ => "未知敌人"
                    };
                    EditorGUILayout.LabelField($"敌人: {enemyLabel}", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        wave.enemies.RemoveAt(j);
                        GUIUtility.ExitGUI();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    enemy.enemyType = (Enemy.EnemyType)EditorGUILayout.EnumPopup("敌人类型", enemy.enemyType);
                    enemy.spawnCount = (uint)EditorGUILayout.IntField("生成数量", (int)enemy.spawnCount);
                    enemy.spawnTime = EditorGUILayout.FloatField("生成间隔(秒)", enemy.spawnTime);
                    
                    // 添加新的时间控制字段
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("时间控制", EditorStyles.boldLabel);
                    enemy.startTime = EditorGUILayout.FloatField("开始生成时间(秒)", enemy.startTime);
                    enemy.holdTime = EditorGUILayout.FloatField("持续生成时间(秒)", enemy.holdTime);
                    EditorGUILayout.HelpBox("开始时间为相对当前波次开始的时间。持续时间为0或负值表示无限生成直到波次结束。", MessageType.Info);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("生成方式", EditorStyles.boldLabel);
                    enemy.isGroup = EditorGUILayout.Toggle("群组生成", enemy.isGroup);
                    
                    if (enemy.isGroup)
                    {
                        enemy.intensity = EditorGUILayout.Slider("群组生成间距", enemy.intensity, 0.1f, 5f);
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("生成位置", EditorStyles.boldLabel);
                    enemy.random = EditorGUILayout.Toggle("随机位置", enemy.random);
                    enemy.isDefault = EditorGUILayout.Toggle("使用默认区域", enemy.isDefault);

                    EditorGUILayout.EndVertical();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
    
    private void AddNewWave()
    {
        int newId = editorWavesConfig.waves.Count > 0 ? 
                    editorWavesConfig.waves[editorWavesConfig.waves.Count - 1].waveId + 1 : 1;
        
        WaveInfo newWave = new WaveInfo
        {
            waveId = newId,
            startTime = 0,
            holdTime = 30,
            enemies = new List<EnemySpawnData>()
        };
        
        editorWavesConfig.waves.Add(newWave);
        
        // 更新折叠状态数组
        InitializeFoldouts();
        
        // 展开新添加的波次
        wavesFoldout[wavesFoldout.Length - 1] = true;
    }
    
    private void AddNewEnemyToWave(WaveInfo wave)
    {
        EnemySpawnData newEnemy = new EnemySpawnData
        {
            enemyType = Enemy.EnemyType.NormalCloseCombatEnemy,
            spawnCount = 5,
            spawnTime = 2.0f,
            isGroup = false,
            intensity = 1.0f,
            random = true,
            isDefault = true,
            // 添加新字段的默认值
            startTime = 0f,  // 默认立即开始生成
            holdTime = 0f    // 默认持续整个波次时间
        };
        
        wave.enemies.Add(newEnemy);
    }
    
    private void LoadWavesConfig()
    {
        string path = EditorUtility.OpenFilePanel("加载波次配置", Application.dataPath + "/Resources", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            editorWavesConfig = JsonUtility.FromJson<WavesConfig>(json);
            
            // 创建或更新TextAsset
            string assetPath = CreateOrUpdateTextAsset(path, json);
            
            // 将创建的TextAsset分配给WaveManager
            TextAsset configAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            wavesConfigFileProperty.objectReferenceValue = configAsset;
            serializedObject.ApplyModifiedProperties();
            
            InitializeFoldouts();
            EditorUtility.SetDirty(target);
        }
    }
    
    private void SaveWavesConfig()
    {
        string path = EditorUtility.SaveFilePanel("保存波次配置", Application.dataPath + "/Resources", "waves_config", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(editorWavesConfig, true);
            File.WriteAllText(path, json);
            
            // 创建或更新TextAsset
            string assetPath = CreateOrUpdateTextAsset(path, json);
            
            // 将创建的TextAsset分配给WaveManager
            TextAsset configAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            wavesConfigFileProperty.objectReferenceValue = configAsset;
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"波次配置已保存至: {path}");
            EditorUtility.SetDirty(target);
        }
    }
    
    private void CreateEmptyConfig()
    {
        editorWavesConfig = new WavesConfig { waves = new List<WaveInfo>() };
        InitializeFoldouts();
        
        // 移除当前配置文件引用
        wavesConfigFileProperty.objectReferenceValue = null;
        serializedObject.ApplyModifiedProperties();
    }
    
    private string CreateOrUpdateTextAsset(string filePath, string content)
    {
        // 确保路径是相对于Assets文件夹的
        string relativePath;
        if (filePath.StartsWith(Application.dataPath))
        {
            relativePath = "Assets" + filePath.Substring(Application.dataPath.Length);
        }
        else
        {
            // 如果文件在Assets文件夹外，复制到Resources文件夹
            string fileName = Path.GetFileName(filePath);
            string resourcesDir = "Assets/Resources";
            
            // 确保Resources文件夹存在
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources"));
            }
            
            relativePath = Path.Combine(resourcesDir, fileName);
            
            // 写入文件到Resources文件夹
            File.WriteAllText(Path.Combine(Application.dataPath, "Resources", fileName), content);
        }
        
        AssetDatabase.Refresh();
        return relativePath;
    }
}
#endif