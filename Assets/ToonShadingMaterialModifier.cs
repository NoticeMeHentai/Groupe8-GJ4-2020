using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ToonShadingMaterialModifier : MonoBehaviour
{
    [Header("Materials")]

    public Material[] m_Materials;
    //public Texture2D m_DiffuseTexture;
    //public Texture2D m_GradientTexture;
    //public Texture2D m_FlatTexture;
    [Header("Parameters")]
    public Texture2D m_ToonGradientTexture;
    
    public Color m_ColorToChoose;
    [Range(0f,1f)]public float m_MinRemap = 0.01f;
    [Range(0f,1f)]public float m_MaxRemap = 0.99f;
    public bool m_Desaturate = false;

    private void Update()
    {
        for(int i=0; i < m_Materials.Length; i++)
        {
            m_Materials[i].SetFloat("_MinRemap", m_MinRemap);
            m_Materials[i].SetFloat("_MaxRemap", m_MaxRemap);
            m_Materials[i].SetColor("_GeneralColor", m_ColorToChoose);
            m_Materials[i].SetTexture("_ToonGradient", m_ToonGradientTexture);
            m_Materials[i].SetFloat("_Desaturate", m_Desaturate?1f:0f);
        }
    }

}
