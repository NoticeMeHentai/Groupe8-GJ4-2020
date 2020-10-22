using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [Tooltip("If there is no menu (in a separate scene) then the gameplay starts right away.")]
    public bool m_HasMenu = false;
    public bool m_Debug = false;
    [Header("Gameplay")]
    public float m_MaxPlayerHP = 100f;

    private float currentTime = 0;
    private float currentScore = 0;


    /// <summary>
    /// The game is actually being played, meaning the countdown is going on.
    /// The game may be stopped for a reason but the player could still move, but it doesn't count as playing
    /// </summary>
    private bool mCountsAsInPlay = false;
    /// <summary>
    /// Current time left for the gameplay
    /// </summary>
    private float mCurrentTimeLeft;
    /// <summary>
    /// Current lives left
    /// </summary>
    private float currentHPLeft = 0;
    /// <summary>
    /// Has the game truly started?
    /// </summary>
    private bool mGameHasStarted = false;
    /// <summary>
    /// Well, yeah, total score
    /// </summary>
    private float mTotalScore = 0;


    public static float sCurrentHP => sInstance.currentHPLeft;
    /// <summary>
    /// Has the game really started?
    /// </summary>
    public static bool sGameHasStarted => sInstance.mGameHasStarted;
    /// <summary>
    /// Current time left for gameplay
    /// </summary>
    public static float sTimeLeft => sInstance.mCurrentTimeLeft;

    /// <summary>
    /// The game is actually being played, meaning the countdown is going on.
    /// The game may be stopped for a reason but the player could still move, but it doesn't count as playing
    /// </summary>
    /// 
    public static float sCurrentRatioHP => sCurrentHP / sInstance.m_MaxPlayerHP;
    
    public static bool sCountsAsPlaying { get { if (sInstance != null) return sInstance.mCountsAsInPlay; else return false; } }
    public static float sCurrentScore => sInstance.currentScore;
    public static bool sIsDebug => sInstance.m_Debug;
    private static GameManager sInstance;



    /// <summary>
    /// Event to prepare the maps and other ressources
    /// </summary>
    public static Action OnGamePreparation;
    /// <summary>
    /// Event to start the game when the map and ressources are ready
    /// </summary>
    public static Action OnGameReady;
    /// <summary>
    /// When the player loses
    /// </summary>
    public static Action OnPlayerDeath;
    /// <summary>
    /// When the player wins
    /// </summary>
    public static Action OnGameOverTimeRanOut;
    /// <summary>
    /// When the player restarts the game
    /// </summary>
    public static Action OnRestart;

    public static Action OnPlayerHit;
    public static Action OnScoreChange;


    private void Awake()
    {
#if UNITY_EDITOR

#elif UNITY_STANDALONE
        UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
#endif

        mCountsAsInPlay = false;
        sInstance = this;
        
        OnGamePreparation += delegate
        {
            currentHPLeft = m_MaxPlayerHP;
        };

        OnGameReady += delegate { mCountsAsInPlay = true; mGameHasStarted = true; };

    }

    private void Start()
    {
        
        if (!m_HasMenu)
        {
            StartCoroutine(TemporaryStart());

        }
    }

    private IEnumerator TemporaryStart()
    {
        OnGamePreparation?.Invoke();
        yield return null;
        OnGameReady?.Invoke();
    }

    public static void DealPlayerDamage(float amount)
    {
        sInstance.currentHPLeft -= amount;
        if (sInstance.currentHPLeft < 0) OnPlayerDeath?.Invoke();
        else OnPlayerHit?.Invoke();
    }

    public static void HealPlayer(float amount)
    {
        sInstance.currentHPLeft = Mathf.Max(sInstance.currentHPLeft + amount, sInstance.m_MaxPlayerHP);
    }


}
