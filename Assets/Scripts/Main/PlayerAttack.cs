using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackInfo
{
    public float m_Damage = 20f;
    public float m_InstantCooldown = 0.2f;
    public float m_MaxWindowTime = 0.5f;
    private float comboTime = 0;
    public float ComboTime { get => comboTime; set { comboTime = value; } }
    public bool _CanKeepCombo => (Time.time > (ComboTime + m_InstantCooldown)) && (Time.time < (ComboTime + m_MaxWindowTime));
    public bool _ComboIsOver => Time.time > (ComboTime + m_MaxWindowTime);
}

public partial class PlayerManager
{
    [Header("Attacks")]
    public AttackInfo[] m_NormalAttackCombo = new AttackInfo[3];
    public PlayerCheckEnemyCollision m_SwordCollider;
    public PlayerCheckEnemyCollision m_FootCollider;
    public PlayerCheckEnemyCollision m_HeavyCollider;
    public float m_HeavyAttackDamage = 30f;
    public float m_HeavyAttackRadius = 2.5f;

    private bool isHeavyAttack = false;
    private bool isInCombo = false;
    private int currentNormalComboCount = 0;
    private bool canDoSomethingElseAfterAttacking = false;
    private bool isNormalAttack = false;

    private AttackInfo _CurrentNormalComboAttack => m_NormalAttackCombo[currentNormalComboCount];

    private void AttackCheck()
    {

        if (canDoSomethingElseAfterAttacking) //Meaning he's comboing
        {
            if (Input.GetAxis("HorizontalMovement") != 0
                || Input.GetAxis("VerticalMovement") != 0
                || Input.GetButtonDown("Dash"))
            {
                ChangeState(PlayerState.Idle);
                _Animator?.SetTrigger("Fine");
                isInCombo = false;
                canDoSomethingElseAfterAttacking = false;
                isChainingCombo = false;
                return;
            }
        }
        bool hasPressedNormalAttack = Input.GetButtonDown("NormalAttack");
        bool hasPressedHeavyAttack = Input.GetButtonDown("HeavyAttack");

        bool hasPressedAnyAttack = hasPressedHeavyAttack || hasPressedNormalAttack;
        if (_CanAttack && hasPressedAnyAttack && !isInCombo)
        {
            isNormalAttack = hasPressedNormalAttack;
            isChainingCombo = false;
            currentNormalComboCount = 0;
            _Animator?.SetInteger("CurrentCombo", 0);
            _Animator?.SetTrigger(hasPressedNormalAttack ? "NormalAttack" : "HeavyAttack");
            ChangeState(PlayerState.Attacking);
            isInCombo = true;

        }
        else if (isInCombo)
        {
            if (isNormalAttack && _CurrentNormalComboAttack._CanKeepCombo)
            {
                canDoSomethingElseAfterAttacking = true;
                if (hasPressedNormalAttack)
                {
                    isChainingCombo = true;
                    currentNormalComboCount = (currentNormalComboCount + 1) % m_NormalAttackCombo.Length;
                    _Animator?.SetInteger("CurrentCombo", currentNormalComboCount);
                    _Animator?.SetTrigger("NormalAttack");
                    canDoSomethingElseAfterAttacking = false;
                }

            }
            else if (isNormalAttack && _CurrentNormalComboAttack._ComboIsOver) isChainingCombo = false;

        }
        else if (!isNormalAttack && !isInCombo && _IsAttacking)
        {
            if (Input.GetAxis("HorizontalMovement") != 0
                || Input.GetAxis("VerticalMovement") != 0
                || Input.GetButtonDown("Dash"))
            {
                ChangeState(PlayerState.Idle);
                _Animator?.SetTrigger("Fine");
                isInCombo = false;
                canDoSomethingElseAfterAttacking = false;
                isChainingCombo = false;
                return;
            }
        }


    }
    private bool isChainingCombo = false;
    public void EnableSwordCollider(int value)
    {
        if (value == 1) _CurrentNormalComboAttack.ComboTime = Time.time;
        m_SwordCollider.Enabled(value == 1 ? true : false);
    }

    public void EnableFootCollider(int value)
    {
        if (value == 1) _CurrentNormalComboAttack.ComboTime = Time.time;
        m_FootCollider.Enabled(value == 1 ? true : false);
    }
    public void EnableHeavyCollider(int value)
    {
        if (value == 0) isInCombo = false;
        m_HeavyCollider.Enabled(value == 1 ? true : false);
    }
    public void DealDamage(Enemy enemy)
    {
        enemy.AddDamage(isHeavyAttack ? m_HeavyAttackDamage : _CurrentNormalComboAttack.m_Damage);
        Debug.Log("Hit an enemy!");
    }
    public void ChangeToIdle()
    {
        if ((isNormalAttack && !isChainingCombo) || (!isNormalAttack && !isInCombo))
        {
            _Animator?.SetTrigger("Fine");
            Debug.Log("r");
            ChangeState(PlayerState.Idle);
            canDoSomethingElseAfterAttacking = false;
            isInCombo = false;
        }
    }
}
