using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSlot360Slash : AttackSlot
{
    [Header("Custom")]
    public PlayerCheckEnemyCollision m_WideCollider;
    public ParticleSystem m_SlashImpact;

   
    public override Enemy[] GetEnemiesHit()
    {
        return m_WideCollider.EnemyList.ToArray();
    }

    public override void CustomComboCalling(int count)
    {
        switch (count)
        {
            case 0: //Open collider
                m_WideCollider.Enabled(true);
                break;
            //Close collider
            case 1:
                m_WideCollider.Enabled(false);
                break;
            case 2:
                m_SlashImpact?.Play();
                break;
            default:
                break;
        }
    }
}
