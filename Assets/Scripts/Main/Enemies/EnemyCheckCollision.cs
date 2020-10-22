using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCheckCollision : MonoBehaviour
{
    private Collider col;
    private SimpleEnemy enemy;

    private bool alreadyChecked = false;
    private void Awake()
    {
        enemy = GetComponentInParent<SimpleEnemy>();
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void EnableCollider(bool value)
    {
        col.enabled = value;
        alreadyChecked = !value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !alreadyChecked)
        {
            enemy.DealDamage();
            alreadyChecked = true;
        }
    }


}
