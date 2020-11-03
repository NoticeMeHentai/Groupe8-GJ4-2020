using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckEnemyCollision : MonoBehaviour
{
    private Collider col;
    private List<Enemy> enemyList = new List<Enemy>();

    public List<Enemy> EnemyList => enemyList;
    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
        
    }

    public void Enabled(bool value)
    {if (value) enemyList = new List<Enemy>();
        col.enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyList.Add(other.GetComponent<Enemy>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemyList.Contains(enemy)) enemyList.Remove(enemy);
        }
    }
}
