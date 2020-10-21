using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public int SpawnAmount = 5;
    [Min(0f)] public float StartSpawnAtTime = 0f;
    [Min(0f)] public float TimeBetweenEachSpawn = 1f;
    public bool HasFinishedSpawning { get; set; } = false;
    public bool IsSpawning { get; set; } = false;

    public bool CanSpawn => !IsSpawning && !HasFinishedSpawning;
    public float TimeUntilNextSpawn { get; set; } = 0;
    public int AlreadySpawnedAmount { get; set; } = 0;

    private float totalSpawnTime = 0;
    private float _TotalSpawnTime { get { if (totalSpawnTime == 0f && TimeBetweenEachSpawn != 0) totalSpawnTime = StartSpawnAtTime * SpawnAmount; return totalSpawnTime; } }

    public bool CanKeepSpawning(float currentTime)
    {
        return currentTime < (_TotalSpawnTime + StartSpawnAtTime);
    }
    public float LocalCurrentTime(float currentTime)
    {
        return currentTime - StartSpawnAtTime;
    }

    public EnemyFollowPoint[] m_PointsToFollow;
}
