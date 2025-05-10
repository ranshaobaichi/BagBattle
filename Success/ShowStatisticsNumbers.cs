using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowStatisticsNumbers : MonoBehaviour
{
    public StatisticsScript.StatisticsData statisticsData; // 统计数据
    void Start()
    {
        statisticsData = StatisticsScript.Instance.GetStatisticsData(); // 获取统计数据

        // 显示统计数据
        // 总伤害
        GameObject totalDamageCausedText = new GameObject("TotalDamageCausedText");
        totalDamageCausedText.transform.SetParent(transform); // 设置父物体
        TextMeshProUGUI damageText = totalDamageCausedText.AddComponent<TextMeshProUGUI>();
        damageText.SetText("Total Damage Caused: " + statisticsData.totalDamageCaused);
        var contentSizeFitter_Damage = totalDamageCausedText.AddComponent<ContentSizeFitter>();
        contentSizeFitter_Damage.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // 自动调整大小
        contentSizeFitter_Damage.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // 自动调整大小

        // 总击杀敌人数量
        GameObject totalEnemiesKilledText = new GameObject("TotalEnemiesKilledText");
        totalEnemiesKilledText.transform.SetParent(transform); // 设置父物体
        TextMeshProUGUI enemiesKilledText = totalEnemiesKilledText.AddComponent<TextMeshProUGUI>();
        enemiesKilledText.SetText("Total Enemies Killed: " + statisticsData.totalEnemiesKilled);
        var contentSizeFitter_Enemies = totalEnemiesKilledText.AddComponent<ContentSizeFitter>();
        contentSizeFitter_Enemies.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // 自动调整大小
        contentSizeFitter_Enemies.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // 自动调整大小
    }
}
