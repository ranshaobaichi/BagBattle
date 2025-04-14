using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

[System.Serializable]
public class WaveData {
    public int enemy_type;
    public float start_time;
    public float hold_time;
    public bool _default;
    public bool random;
    public bool group;
    public float intensity;
    public uint spawn_nums;
    public uint spawn_time;
}
 
[System.Serializable]
public class RootWrapper {
    public WaveData[] waves;
}
 
public class WaveController : MonoBehaviour {
    public GameObject[] EnemySpawners;
 
    void Start() {
        TextAsset wavesJson = Resources.Load<TextAsset>("Wave_Data");
        if(wavesJson == null) {
            Debug.LogError("JSON文件未找到");
            return;
        }
 
        RootWrapper wrapper = JsonUtility.FromJson<RootWrapper>(wavesJson.text); 
        WaveData[] waves = wrapper.waves; 
 
        foreach (WaveData wave_data in waves) {
            // 添加数组越界保护 
            if(wave_data.enemy_type >= EnemySpawners.Length) {
                Debug.LogError($"无效的敌人类型索引: {wave_data.enemy_type}"); 
                continue;
            }
 
            GameObject spawnerObj = Instantiate(EnemySpawners[wave_data.enemy_type]); 
            EnemySpawner wave = spawnerObj.GetComponent<EnemySpawner>();

            // 使用属性拷贝替代逐个赋值 
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(wave_data), wave);
        }
    }
}