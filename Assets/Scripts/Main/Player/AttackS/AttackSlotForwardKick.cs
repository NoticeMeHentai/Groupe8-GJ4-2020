using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSlotForwardKick : AttackSlot
{
    [Header("Custom")]
    public PlayerCheckEnemyCollision m_FootCollider;
    public ParticleSystem m_FootImpact;
    public override Enemy[] GetEnemiesHit()
    {
        return m_FootCollider.EnemyList.ToArray();
    }

    public override void CustomComboCalling(int count)
    {
        switch (count)
        {
            case 0: //Open collider
                m_FootCollider.Enabled(true);
                break;
            //Close collider
            case 1:
                m_FootCollider.Enabled(false);
                break;
            case 2:
                m_FootImpact?.Play();
                break;
            default:
                break;
        }
    }
}
