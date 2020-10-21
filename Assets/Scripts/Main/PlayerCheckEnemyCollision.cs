using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckEnemyCollision : MonoBehaviour
{
    private Collider col;
    public PlayerMovement m_Papa;
    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
        
    }

    public void Enabled(bool value)
    {
        col.enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            m_Papa.DealDamage(other.GetComponent<Enemy>());
        }
    }
}
