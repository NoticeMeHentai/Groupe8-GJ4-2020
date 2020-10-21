using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public Color m_GizmosColor = Color.red;
    public Transform m_SpawnPoint;
    public GameObject m_EnemyPrefab;
    public EnemyWave[] m_EnemyWaves;

    private List<EnemyWave> currentWaves = new List<EnemyWave>();

    private float currentTime = 0;


    private void Update()
    {
        currentTime += Time.deltaTime;
        for (int i = 0; i < m_EnemyWaves.Length; i++)
        {
            EnemyWave wave = m_EnemyWaves[i];
            if (currentTime > wave.StartSpawnAtTime && wave.CanSpawn)
            {
                wave.IsSpawning = true;
                currentWaves.Add(wave);
            }
        }
        for(int i=0; i < currentWaves.Count; i++)
        {
            EnemyWave wave = currentWaves[i];        
            if (wave.CanKeepSpawning(currentTime))
            {
                wave.TimeUntilNextSpawn -= Time.deltaTime;
                if (wave.TimeUntilNextSpawn < 0)
                {
                    Spawn(wave);
                    wave.AlreadySpawnedAmount++;
                    if(wave.AlreadySpawnedAmount == wave.SpawnAmount)
                    {
                        wave.HasFinishedSpawning = true;
                        currentWaves.Remove(wave);
                        Debug.Log("Spawned all enemies of this wave");
                    }
                    else
                    {
                        wave.TimeUntilNextSpawn += wave.TimeBetweenEachSpawn;
                    }
                }
            }
        }
    }

    private void Spawn(EnemyWave wave)
    {
        Debug.Log("Spawned new enemy");
        Enemy enemy =  Instantiate(m_EnemyPrefab, m_SpawnPoint.position, transform.rotation).GetComponent<Enemy>();
        enemy.Initialize(wave.m_PointsToFollow);
        EnemyManager.AddNewEnemy(enemy);
        enemy.OnDeath += delegate { EnemyManager.RemoveEnemy(enemy); };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = m_GizmosColor;
        for(int i=0; i < m_EnemyWaves.Length; i++)
        {
            for(int j = 0; j < m_EnemyWaves[i].m_PointsToFollow.Length; j++)
            {
                if (j != 0)
                {
                    Gizmos.DrawLine(m_EnemyWaves[i].m_PointsToFollow[j].Position, m_EnemyWaves[i].m_PointsToFollow[j - 1].Position);
                }
            }
        }
    }
}
