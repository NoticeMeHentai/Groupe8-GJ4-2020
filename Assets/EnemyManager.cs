using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemiesAlive = new List<Enemy>();

    public static int sEnemyCount => sInstance.enemiesAlive.Count;

    public static Action OnEnemyCountChange;
    private static EnemyManager sInstance;
    private void Awake()
    {
        sInstance = this;
    }
    public static void AddNewEnemy(Enemy enemy)
    {
        sInstance.enemiesAlive.Add(enemy);
        OnEnemyCountChange?.Invoke();
    }

    public static void RemoveEnemy(Enemy enemy)
    {
        sInstance.enemiesAlive.Remove(enemy);
        OnEnemyCountChange?.Invoke();
    }
}
