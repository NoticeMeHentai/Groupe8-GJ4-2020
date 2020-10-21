using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemyDetection : MonoBehaviour
{
    private SphereCollider sphereCollider;
    TurretPlacement turretPlacement;
    private void Awake()
    {
        turretPlacement = GetComponentInParent<TurretPlacement>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            turretPlacement.DetectedEnemyEntering(other.GetComponent<Enemy>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            turretPlacement.DetectedEnemyLeaving(other.GetComponent<Enemy>());
    }
}
