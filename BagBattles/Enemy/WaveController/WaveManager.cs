using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EnemyTypeMapping
{
    public Enemy.EnemyType enemyType;
    public GameObject prefab;
}

public class WaveManager : MonoBehaviour
{
    // private const string CURRENT_WAVE_KEY = "CurrentWaveIndex";
    public static WaveManager Instance { get; set; }

    // 波次配置文件
    [SerializeField] private TextAsset wavesConfigFile;

    // 在编辑器中显示当前加载的波次信息（只读）
    [SerializeField, HideInInspector] private int totalWaves;
    [SerializeField] private GameObject spawnerParent;

    // 预制体字典，键为敌人类型名称
    [SerializeField] private List<EnemyTypeMapping> enemyPrefabs;

    private WavesConfig wavesConfig;
    private int currentWaveIndex;

    public void Awake()
    {
        Debug.Log("WaveManager Awake");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 初始化敌人预制体字典
        InitializeEnemyPrefabDictionary();
        // 加载波次配置
        LoadWavesConfig();
        currentWaveIndex = 0;
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            StartWaves();
        }
        else
        {
            EndWaveSequence();
        }
    }

    private void InitializeEnemyPrefabDictionary()
    {
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab.prefab == null)
            {
                Debug.LogError("Enemy_type : " + prefab.enemyType + " prefab is null!");
            }
        }
    }

    private void LoadWavesConfig()
    {
        if (wavesConfigFile != null)
        {
            wavesConfig = JsonUtility.FromJson<WavesConfig>(wavesConfigFile.text);
            Debug.Log($"已加载{wavesConfig.waves.Count}个波次的敌人生成配置");

            // 更新编辑器显示信息
            totalWaves = wavesConfig.waves.Count;
        }
        else
        {
            Debug.LogError("未找到波次配置文件！");
            wavesConfig = new WavesConfig { waves = new List<WaveInfo>() };
            totalWaves = 0;
        }
    }
    public void EndWaveSequence()
    {
        StopAllCoroutines();
        foreach (var spawners in spawnerParent.GetComponents<EnemySpawner>())
        {
            Destroy(spawners);
        }
        currentWaveIndex++;
        PlayerPrefs.SetInt(PlayerPrefsKeys.CURRENT_WAVE_KEY, currentWaveIndex);
        PlayerPrefs.Save();
    }

    private void StartWaves()
    {
        if (wavesConfig == null)
        {
            // 初始化敌人预制体字典
            InitializeEnemyPrefabDictionary();
            // 加载波次配置
            LoadWavesConfig();
            currentWaveIndex = 0;
        }
        currentWaveIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.CURRENT_WAVE_KEY);
        var wave = wavesConfig.waves[currentWaveIndex];
        Debug.Log($"开始生成第{wave.waveId}波敌人");

        // 为每种敌人类型创建生成器
        foreach (var enemyData in wave.enemies)
        {
            if (!enemyPrefabs.Exists(e => e.enemyType == enemyData.enemyType))
            {
                Debug.LogError($"未找到敌人类型: {enemyData.enemyType}");
                continue;
            }

            var enemyPrefab = enemyPrefabs.Find(e => e.enemyType == enemyData.enemyType).prefab;
            // // 创建对应类型的Spawner
            // string spawnerName = $"{enemyData.enemyType}_Spawner";
            // GameObject spawnerObject = new GameObject(spawnerName);
            // spawnerObject.transform.parent = spawnerParent != null ? spawnerParent : transform;
            Debug.Log("创建敌人生成器: " + enemyData.enemyType);
            // 添加相应类型的Spawner组件
            EnemySpawner spawner = spawnerParent.AddComponent<EnemySpawner>();

            // 设置预制体
            spawner.enemyPrefab = enemyPrefab;
            // 从配置初始化生成器
            spawner.Initialize(enemyData);
        }
    }

    // 加载当前设置的配置文件
    public void ReloadConfig()
    {
        if (wavesConfigFile != null)
        {
            wavesConfig = JsonUtility.FromJson<WavesConfig>(wavesConfigFile.text);
            Debug.Log($"已重新加载{wavesConfig.waves.Count}个波次的敌人生成配置");
        }
    }

    // 重置当前波次索引
    public void ResetWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = 0;
    }

    // 开始从特定波次播放
    public void StartFromWave(int waveIndex)
    {
        if (wavesConfig != null && waveIndex < wavesConfig.waves.Count)
        {
            StopAllCoroutines();
            currentWaveIndex = waveIndex;
            StartWaves();
        }
    }

    // 获取当前配置
    public WavesConfig GetCurrentConfig()
    {
        return wavesConfig;
    }
}