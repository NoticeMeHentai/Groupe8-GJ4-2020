using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AttackSlot : MonoBehaviour 
{
    public enum AttackType { Light, Heavy}
    [Header("Main")]
    [SerializeField] private AttackType m_AttackType;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private AnimationClip m_AnimationToPlay;
    private AttackSlot lightAttackComboContinuation;
    private AttackSlot heavyAttackComboContinuation;

    protected bool _IsBeingUsed { get; private set; }

    private void Start()
    {
        if (transform.childCount > 0)
        {
            AttackSlot firstChild = transform.GetChild(0).GetComponent<AttackSlot>();
            if (firstChild.IsLightAttack && lightAttackComboContinuation == null) lightAttackComboContinuation = firstChild;
            else if (firstChild.IsHeavyAttack && heavyAttackComboContinuation == null) heavyAttackComboContinuation = firstChild;
            if (transform.childCount > 1)
            {
                AttackSlot secondChild = transform.GetChild(1).GetComponent<AttackSlot>();
                if (secondChild.IsLightAttack && lightAttackComboContinuation == null) lightAttackComboContinuation = secondChild;
                else if (secondChild.IsHeavyAttack && heavyAttackComboContinuation == null) heavyAttackComboContinuation = secondChild;
            }
        }
    }

    public float Damage => m_Damage;
    public AnimationClip AnimationToPlay => m_AnimationToPlay;
    public AttackSlot LightComboContinuation => lightAttackComboContinuation;
    public AttackSlot HeavyComboContinuation => heavyAttackComboContinuation;
    public bool HasLightCombo => lightAttackComboContinuation != null;
    public bool HasHeavyCombo => heavyAttackComboContinuation != null;
    public bool IsLightAttack => m_AttackType == AttackType.Light;
    public bool IsHeavyAttack => m_AttackType == AttackType.Heavy;

    public virtual void StartAnimation()
    {
        _IsBeingUsed = true;
        OnStartAnimation();
    }

    public virtual void EndAnimation()
    {
        _IsBeingUsed = false;
        OnEndAnimation();
    }
    #region Virtual or Abstracts
    protected virtual void OnStartAnimation()
    {
        Debug.Log("Combo " + name + " started animation!");
    }

    public virtual void CustomComboCalling(int count)
    {
        Debug.Log("Combo " + name + " called custom method numer " + count + " !");
    }

    public abstract Enemy[] GetEnemiesHit();

    protected virtual void OnEndAnimation()
    {
        Debug.Log("Combo " + name + " ended animation!");
    } 
    #endregion

}
