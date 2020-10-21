using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
    public float m_Radius = 5f;
    public Renderer m_BarRenderer;
    private Material hpBarMat;

    


    private void Awake()
    {
        transform.localScale = Vector3.one * m_Radius;
        hpBarMat = new Material(m_BarRenderer.material);
        m_BarRenderer.material = hpBarMat;
    }

    private void Update()
    {
        hpBarMat.SetFloat("_Value", GameManager.sCurrentRatioHP);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_Radius * 0.5f);
    }
}
