using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSlotHeavSwordyImpact : AttackSlot
{
    [Header("Custom")]
    public float m_RadiusImpact = 5f;

    private List<Enemy> enemyList;
    public override Enemy[] GetEnemiesHit()
    {
        if (cols.Length == 0) return null;
        enemyList = new List<Enemy>();
        for (int i = 0; i < cols.Length; i++)
            if(cols[i].CompareTag("Enemy"))
            enemyList.Add(cols[i].GetComponent<Enemy>());
        return enemyList.ToArray();
    }
    Collider[] cols;
    public override void CustomComboCalling(int count)
    {
        switch (count)
        {
            case 0: //Open collider
                System.Array.Clear(cols, 0, cols.Length);
                Physics.OverlapSphereNonAlloc(PlayerManager.Position, m_RadiusImpact, cols, MathHelper.EnemiesLayerMask);
                break;
            default:
                break;
        }
    }
}
