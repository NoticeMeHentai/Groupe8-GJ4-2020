using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Renderer m_HPBarRenderer;
    public float m_Speed = 10f;
    public float m_Damage = 15f;
    public float m_AttackInterval = 1.25f;
    public float m_MaxLife = 35f;
    public float m_CurrencyGiven = 50f;
    public float m_RotationLerpSpeed = 10f;
    private EnemyFollowPoint[] followPoints;

    private float currentHP;
    private Material hpBarMat;
    private bool isAttackingTurret = false;
    private float currentAttackTime;

    public Action OnDeath;

    private TurretPlacement turretTarget;

    private int currentPointToFollowIndex;
    private bool _IsFollowingAPoint => currentPointToFollowIndex < followPoints.Length;
    private EnemyFollowPoint _CurrentFollowPoint => followPoints[currentPointToFollowIndex];
    private Vector3 _CurrentDirection
    {
        get
        {
            return ((currentPointToFollowIndex == followPoints.Length ? GameManager.sObjectiveToDefendPosition : followPoints[currentPointToFollowIndex])
                - transform.position).normalized;
        }
    }

    private float _CurrentDistance
    {
        get
        {
            return Vector3.Distance(transform.position, (currentPointToFollowIndex == followPoints.Length ? GameManager.sObjectiveToDefendPosition : followPoints[currentPointToFollowIndex]));
        }
    }

    public float HPRatio => currentHP / m_MaxLife;
    public Vector3 Position => transform.position;

    public void Initialize(EnemyFollowPoint[] follow)
    {
        followPoints = follow;
        currentHP = m_MaxLife;
        hpBarMat = new Material(m_HPBarRenderer.material);
        m_HPBarRenderer.material = hpBarMat;
        hpBarMat.SetFloat("_Value", HPRatio);

    }

    private void FixedUpdate()
    {
        if (!isAttackingTurret)
        {
            Vector3 dir = _CurrentDirection;
            transform.position += dir * Time.fixedDeltaTime * m_Speed;

            transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(dir), m_RotationLerpSpeed * Time.fixedDeltaTime);
            if (_IsFollowingAPoint && _CurrentFollowPoint.IsInsideRange(transform.position))
            {
                currentPointToFollowIndex++;
                Debug.Log("Going to the next point!");
            }
        }
        else
        {
            currentAttackTime += Time.fixedDeltaTime;
            if (currentAttackTime > m_AttackInterval)
            {
                Attack();
                currentAttackTime -= m_AttackInterval;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core"))
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
                col.enabled = false;
            Debug.Log("Kaboom");
            GameManager.HitCore(m_Damage);
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Turret"))
        {
            turretTarget = other.GetComponent<TurretPlacement>();
            if (turretTarget.IsAlive)
            {
                isAttackingTurret = true;
                currentAttackTime = m_AttackInterval*0.5f;
                turretTarget.OnDeath += delegate{ isAttackingTurret = false; };
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log("Watashi wa... shinda. F");
            GameObject.Destroy(gameObject);
        }
        hpBarMat.SetFloat("_Value", HPRatio);
    }

    public void Attack()
    {
        turretTarget.TakeDamage(m_Damage);

    }


}
