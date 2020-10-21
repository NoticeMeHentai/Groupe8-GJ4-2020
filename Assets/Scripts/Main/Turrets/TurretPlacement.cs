using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TurretPlacement : MonoBehaviour
{
    public Renderer m_ActionTurretPopUp;
    public Renderer m_SelectionTurretPopUp;
    public Renderer m_HPBarRenderer;
    public ParticleSystem m_BuildUpgradeEffect;
    public GameObject m_TakeDamageEffectPrefab;
    public TextMeshPro m_CurrentLevelUpgradeTextPopUp;
    public TextMeshPro m_NextUpgradeCostTextPopUp;
    public Transform m_RangeDisplayTransform;
    public SphereCollider m_EnemyDetectionCollider;
    private bool hasPlacedTurret = false;
    private bool isBeingTriggered = false;
    private bool L2R2isReset = false;
    private int currentTurretIndex = 0;
    private float currentHP;


    private Turret[] turrets;
    private List<Enemy> enemies = new List<Enemy>();
    private Enemy currentEnemyTargeted;
    private BoxCollider boxCollider;
    private Transform duplicateDisplayTransform;
    private Material hpBarMat;
    #region Properties
    private Turret _CurrentTurret => turrets[currentTurretIndex];
    public bool IsAlive => currentHP > 0 && hasPlacedTurret;
    private Material rangeDisplayMaterial;
    private bool _CanPlaceTurret => !hasPlacedTurret && isBeingTriggered;
    private float _Range => _CurrentTurret.Range;
    private bool _SelectionLeft
    {
        get
        {
            float value = Input.GetAxis("Selection");
            if (!L2R2isReset && value == 0) L2R2isReset = true;
            else if (L2R2isReset && value < -0.5)
            {
                L2R2isReset = false;
                Debug.Log("Selected left!");
                return true;
            }
            return false;
        }
    }

    private bool _SelectionRight
    {
        get
        {
            float value = Input.GetAxis("Selection");
            if (!L2R2isReset && value == 0) L2R2isReset = true;
            else if (L2R2isReset && value > 0.5)
            {
                L2R2isReset = false;
                Debug.Log("Selected right!");
                return true;
            }
            return false;
        }
    }

    private bool _SelectionAccept => Input.GetButtonDown("Action");

    private bool CanBeUpgraded => hasPlacedTurret && _CurrentTurret.CanBeUpgraded;

    public bool HasATarget => currentEnemyTargeted != null;
    public Enemy Target => currentEnemyTargeted;
    public float CurrentHP => currentHP;

    public Action OnRepair;
    public Action OnBuild;
    public Action OnUpgrade;
    public Action OnDeath;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        hpBarMat = new Material(m_HPBarRenderer.material);
        m_HPBarRenderer.material = hpBarMat;
        hpBarMat.SetFloat("_Value", 1);
        m_HPBarRenderer.enabled = false;
        turrets = GetComponentsInChildren<Turret>();
        m_SelectionTurretPopUp.enabled = false;
        m_ActionTurretPopUp.enabled = false;
        m_NextUpgradeCostTextPopUp.gameObject.SetActive(false);
        m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(false);
        m_NextUpgradeCostTextPopUp.text = "UpgradeCost: " + _CurrentTurret.UpgradeCost;
        //m_RangeDisplayTransform.localScale = Vector3.one * _CurrentTurret.Range;
        m_RangeDisplayTransform.gameObject.SetActive(false);
        m_CurrentLevelUpgradeTextPopUp.text = "Level 1/" + _CurrentTurret.MaxUpgrades;

        duplicateDisplayTransform = Instantiate(m_RangeDisplayTransform.gameObject,
                m_RangeDisplayTransform.position,
                m_RangeDisplayTransform.rotation,
                transform).transform;

        boxCollider = GetComponent<BoxCollider>();

        OnUpgrade += delegate 
        {
            GameManager.DeductCurrency(_CurrentTurret.UpgradeCost); //The turret points the upgrade cost of the next upgrade level
            _CurrentTurret.Upgrade();
            m_CurrentLevelUpgradeTextPopUp.text="Level "+(_CurrentTurret.CurrentUpgrade + 1) + "/" + _CurrentTurret.MaxUpgrades;
            m_EnemyDetectionCollider.radius = _Range * 0.5f;
            m_RangeDisplayTransform.localScale = duplicateDisplayTransform.localScale= Vector3.one * _Range;
            if ((_CurrentTurret.CurrentUpgrade + 1) == _CurrentTurret.MaxUpgrades)
            {
                m_ActionTurretPopUp.enabled = false;
                m_NextUpgradeCostTextPopUp.gameObject.SetActive(false);
                //m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(false);
            }
            else
            {
                m_NextUpgradeCostTextPopUp.text = "UpgradeCost: " + _CurrentTurret.UpgradeCost;
            }
            currentHP = _CurrentTurret.MaxHP;
            m_BuildUpgradeEffect.Play();
            hpBarMat.SetFloat("_Value", 1);
        };


        OnBuild += delegate 
        { 
            GameManager.DeductCurrency(_CurrentTurret.UpgradeCost);
            _CurrentTurret.Appear();
            currentHP = _CurrentTurret.MaxHP;

            m_HPBarRenderer.enabled = true;
            hpBarMat.SetFloat("_Value", 1);
            m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(true);
            m_CurrentLevelUpgradeTextPopUp.text="Level 1/" + _CurrentTurret.MaxUpgrades;
            m_NextUpgradeCostTextPopUp.text = "UpgradeCost: " + _CurrentTurret.UpgradeCost;
            m_NextUpgradeCostTextPopUp.gameObject.SetActive(true);

            for (int i = 0; i < turrets.Length; i++)
                turrets[i].enabled = i == currentTurretIndex ? true : false;


            hasPlacedTurret = true;
            m_SelectionTurretPopUp.enabled = false;
            m_RangeDisplayTransform.localScale = duplicateDisplayTransform.localScale = Vector3.one * _Range;
            m_EnemyDetectionCollider.radius = _Range * 0.5f;
            m_BuildUpgradeEffect.Play();

        };
    }


    private void Update()
    {
        if (isBeingTriggered)
        {
            if (_CanPlaceTurret)
            {
                if (_SelectionLeft) SwitchTurret(true);
                else if (_SelectionRight) SwitchTurret(false);
                else if (_SelectionAccept && _CurrentTurret.CanBeUpgraded) OnBuild?.Invoke();
            }
            else if (CanBeUpgraded && _SelectionAccept) OnUpgrade?.Invoke();
        }

    }




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasPlacedTurret)
            {
                m_RangeDisplayTransform.gameObject.SetActive(true);
                m_ActionTurretPopUp.enabled = true;
                m_NextUpgradeCostTextPopUp.gameObject.SetActive(true);
                m_SelectionTurretPopUp.enabled = true;

                m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(true);
                _CurrentTurret.ShowHologram(true);
            }
            else if(CanBeUpgraded)
            {
                m_ActionTurretPopUp.enabled = true;
                m_NextUpgradeCostTextPopUp.gameObject.SetActive(true);
            }
            isBeingTriggered = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasPlacedTurret)
            {
                m_RangeDisplayTransform.gameObject.SetActive(false);
                m_SelectionTurretPopUp.enabled = false;
                m_ActionTurretPopUp.enabled = false;
                m_NextUpgradeCostTextPopUp.gameObject.SetActive(false);
                _CurrentTurret.ShowHologram(false);

                m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(false);

            }
            else 
            {
                m_ActionTurretPopUp.enabled = false;
                if(!CanBeUpgraded) m_NextUpgradeCostTextPopUp.gameObject.SetActive(false);
            }
            isBeingTriggered = false;
        }
    } 
    #endregion

    private void SwitchTurret(bool nextOrPrevious)
    {
        _CurrentTurret.ShowHologram(false);
        currentTurretIndex = nextOrPrevious 
            ? (currentTurretIndex - 1 + turrets.Length) % turrets.Length 
            : (currentTurretIndex + 1) % turrets.Length;
        _CurrentTurret.ShowHologram(true);
        m_RangeDisplayTransform.localScale = Vector3.one * _CurrentTurret.Range;
        m_NextUpgradeCostTextPopUp.text = "UpgradeCost: " + _CurrentTurret.UpgradeCost;
        m_CurrentLevelUpgradeTextPopUp.text = "Level 1/" + _CurrentTurret.MaxUpgrades;
    }


    private void SelectNewTarget()
    {
        currentEnemyTargeted = null;
        if (enemies.Count > 0)
            currentEnemyTargeted = enemies[enemies.Count - 1]; //Last one to be added, having a higher chance to stay within range for a while
    }

    public void DetectedEnemyEntering(Enemy enemy)
    {
        if(hasPlacedTurret)
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                enemy.OnDeath += delegate { if (enemies.Contains(enemy)) enemies.Remove(enemy); SelectNewTarget(); };
                if (enemies.Count == 1) SelectNewTarget();
            }
            else Debug.LogWarning("Turret at pos " + transform.position + " couldn't add enemy because it was already on the list");
    }
    

    public void DetectedEnemyLeaving(Enemy enemy)
    {
        if(hasPlacedTurret) 
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            SelectNewTarget();
        }
        else Debug.LogWarning("Turret at pos " + transform.position + " couldn't remove enemy because it wasn't on the list");
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        Instantiate(m_TakeDamageEffectPrefab, transform.position, Quaternion.identity);
        if (currentHP < 0)
        {
            hasPlacedTurret = false;
            OnDeath?.Invoke();
            m_HPBarRenderer.enabled = false;
            if (!isBeingTriggered)
            {
                m_CurrentLevelUpgradeTextPopUp.gameObject.SetActive(false);
                m_NextUpgradeCostTextPopUp.gameObject.SetActive(false);
            }
            else
            {
                m_CurrentLevelUpgradeTextPopUp.text = "1/" + _CurrentTurret.MaxUpgrades;
                m_NextUpgradeCostTextPopUp.text = _CurrentTurret.UpgradeCost.ToString();
                m_ActionTurretPopUp.enabled = true;
                m_SelectionTurretPopUp.enabled = true;
                _CurrentTurret.ShowHologram(true);
            }
        }
        hpBarMat.SetFloat("_Value", currentHP / _CurrentTurret.MaxHP);
    }


    private void OnDrawGizmos()
    {
        //if (EditorApplication.isPlaying )
        //{
        //    Handles.color = Color.red;
        //    Handles.DrawWireDisc(transform.position, Vector3.up, _Range);
        //}
    }
}
