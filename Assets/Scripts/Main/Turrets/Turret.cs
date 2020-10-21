using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Turret : MonoBehaviour
{
    public GameObject m_Bullet;
    public Transform m_ShootPoint;
    public TurretUpgradeSO[] m_Upgrades;



    public GameObject m_ActualTurret;
    public GameObject m_HologramTurret;

    private bool hasAppeared = false;
    private int currentUpgradeTier = 0;
    private float timeBeforeShoot;
    private float timeBetweenShoots;

    private TurretPlacement turretPlacement;
    private bool _IsAlive => turretPlacement.CurrentHP>0 && hasAppeared;
    private bool _HasATarget => turretPlacement.HasATarget;
    private Enemy _CurrentTarget => turretPlacement.Target;



    public float Range => m_Upgrades[currentUpgradeTier].Range;
    public float MaxHP => m_Upgrades[currentUpgradeTier].MaxHP;
    public float Damage => m_Upgrades[currentUpgradeTier].Damage;
    public float BulletSpeed => m_Upgrades[currentUpgradeTier].BulletSpeed;
    public float FireRate => m_Upgrades[currentUpgradeTier].FireRate;
    public float UpgradeCost => !hasAppeared ? m_Upgrades[0].UpgradeCost : m_Upgrades[currentUpgradeTier + 1].UpgradeCost;
    public bool CanBeUpgraded => (!hasAppeared || (currentUpgradeTier+1<MaxUpgrades)) && GameManager.sCurrentCurrency > UpgradeCost;
    public int CurrentUpgrade => currentUpgradeTier;
    public int MaxUpgrades => m_Upgrades.Length;


    private void Awake()
    {
        m_ActualTurret.SetActive(false);
        m_HologramTurret.SetActive(false);
        turretPlacement = GetComponentInParent<TurretPlacement>();
        turretPlacement.OnDeath += delegate { hasAppeared = false; currentUpgradeTier = 0; m_ActualTurret.SetActive(false); };
    }

    private void FixedUpdate()
    {
        if (_IsAlive)
        {
            timeBeforeShoot -= Time.fixedDeltaTime;
            if (_HasATarget)
            {
                Vector3 direction = (_CurrentTarget.transform.position - transform.position).normalized.FlatOneAxis(Vector3Extensions.Axis.y, true);
                Quaternion newRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 20f * Time.fixedDeltaTime);
                if(timeBeforeShoot<0)
                {
                    timeBeforeShoot = (Mathf.Abs(timeBeforeShoot)%timeBetweenShoots)*(-1f);
                    timeBeforeShoot += timeBetweenShoots;
                    Shoot();
                }
            }
        }
    }

    private void Shoot()
    {
        
        m_ShootPoint.LookAt(_CurrentTarget.transform.position + Vector3.up);
        Instantiate(m_Bullet, m_ShootPoint.position, m_ShootPoint.rotation).GetComponent<Bullet>().Initialize(Damage, BulletSpeed);
    }

    public void ShowHologram(bool show)
    {
        m_ActualTurret.SetActive(false);
        m_HologramTurret.SetActive(show?true:false);
    }
    public void Appear()
    {

        timeBetweenShoots = 1f / FireRate;
        m_ActualTurret.SetActive(true);
        m_HologramTurret.SetActive(false);
        hasAppeared = true;
    }


    public void Upgrade()
    {
        timeBetweenShoots = 1f / FireRate;
        currentUpgradeTier++;
        //Some effect when upgrading
    }
}
