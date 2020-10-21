using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="TFA/TurretUpgrade", menuName ="TFA/TurretUpgrade")]
public class TurretUpgradeSO : ScriptableObject
{
    [SerializeField]private float m_Range = 10f;
    [SerializeField] private float m_FireRate = 1f;
    [SerializeField] private float m_MaxHP = 10f;
    [SerializeField] private float m_BulletSpeed = 10f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private float m_UpgradeCost = 100f;

    public float Range => m_Range;
    public float FireRate => m_FireRate;
    public float MaxHP => m_MaxHP;
    public float BulletSpeed => m_BulletSpeed;
    public float Damage => m_Damage;
    public float UpgradeCost => m_UpgradeCost;
}
