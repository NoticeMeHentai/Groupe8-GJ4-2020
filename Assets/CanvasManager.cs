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
        GameManager.OnCurrencyChange += delegate { M_ScoreText.text = GameManager.sCurrentCurrency.ToString();};
        GameManager.OnCoreHit += delegate { m_HPBarMaterial.SetFloat("_Value", GameManager.sCurrentRatioHP); };
    }
}
