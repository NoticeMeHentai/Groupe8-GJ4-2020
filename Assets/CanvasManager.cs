using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Material m_HPBarMaterial;
    public Text M_ScoreText;
    public Text M_EnemyCountText;

    private void Awake()
    {
        M_EnemyCountText.text = "0";
        m_HPBarMaterial.SetFloat("_Value", 1);
        EnemyManager.OnEnemyCountChange += delegate { M_EnemyCountText.text = EnemyManager.sEnemyCount.ToString(); };
        GameManager.OnScoreChange += delegate { M_ScoreText.text = GameManager.sCurrentScore.ToString();};
        PlayerManager.OnHit += delegate { m_HPBarMaterial.SetFloat("_Value", PlayerManager.sCurrentRatioHP); };
        PlayerManager.OnHeal+= delegate { m_HPBarMaterial.SetFloat("_Value", PlayerManager.sCurrentRatioHP); };
    }
}
