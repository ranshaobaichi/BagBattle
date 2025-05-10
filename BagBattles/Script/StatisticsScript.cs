using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StatisticsScript
{
    public class StatisticsData
    {
        public double totalDamageCaused = 0; // 总伤害
        public ulong totalEnemiesKilled = 0; // 总击杀敌人数量
    }

    private const string STATISTICS_DATA_PATH = "statisticsData.json"; // 统计数据文件路径
    private static StatisticsScript instance;
    public static StatisticsScript Instance
    { 
        get
        {
            if (instance == null)
            {
                instance = new StatisticsScript();
                instance.LoadStatisticsData(); // 加载统计数据
            }
            return instance;
        }
        private set { instance = value; }
    }
    private StatisticsData statisticsData;

    public void AddTotalDamageCaused(float damage) => statisticsData.totalDamageCaused += damage;
    public void AddTotalEnemiesKilled(ulong count = 1) => statisticsData.totalEnemiesKilled += count;
    public StatisticsData GetStatisticsData() => statisticsData;
    
    private void LoadStatisticsData()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 1)
        {
            statisticsData = new StatisticsData(); // 新游戏时初始化统计数据
            Debug.Log("New game started, initializing statistics data.");
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, STATISTICS_DATA_PATH);
            string json = File.ReadAllText(path); // 从文件加载数据
            statisticsData = JsonUtility.FromJson<StatisticsData>(json); // 反序列化数据
            Debug.Log("Loaded statistics data from path: " + path);
        }
    }

    public void StoreStatisticsData()
    {
        string path = Path.Combine(Application.persistentDataPath, STATISTICS_DATA_PATH);
        string json = JsonUtility.ToJson(statisticsData, true); // 序列化数据
        File.WriteAllText(path, json); // 保存数据到文件
        Debug.Log("Saved statistics data to path: " + path);
    }
}
