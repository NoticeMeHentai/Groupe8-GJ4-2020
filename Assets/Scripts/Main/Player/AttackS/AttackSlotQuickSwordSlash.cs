using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSlotQuickSwordSlash : AttackSlot
{
    [Header("Custom")]
    public PlayerCheckEnemyCollision m_SwordCollider;
    public override Enemy[] GetEnemiesHit()
    {
        return m_SwordCollider.EnemyList.ToArray();
    }

    public override void CustomComboCalling(int count)
    {
        switch (count)
        {
            case 0: //Open collider
                m_SwordCollider.Enabled(true);
                break;
                //Close collider
            case 1:
                m_SwordCollider.Enabled(false);
                break;
            default:
                break;
        }
    }
}
