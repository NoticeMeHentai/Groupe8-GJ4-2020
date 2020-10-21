using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowPoint : MonoBehaviour
{
    public float m_ShowRadius = 3f;
    public bool m_SnapToGround = true;
    public Vector3 Position => transform.position;

    private float squareRadius = 0;
    private void Awake()
    {
        squareRadius = m_ShowRadius * m_ShowRadius;
    }

    public bool IsInsideRange(Vector3 pos)
    {
        return Vector3.SqrMagnitude(pos - transform.position) < squareRadius;
    }


    public static implicit operator Vector3(EnemyFollowPoint point)
    {
        return point.Position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_ShowRadius);
    }
}
