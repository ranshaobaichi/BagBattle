using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveInfo
{
    public int waveId;
    public uint startTime;
    public float holdTime;
    public List<EnemySpawnData> enemies;
}

[Serializable]
public class WavesConfig
{
    public List<WaveInfo> waves;
}