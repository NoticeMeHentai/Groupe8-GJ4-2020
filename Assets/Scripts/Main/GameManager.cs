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

    
    //public static bool sCountsAsPlaying { get { if (sInstance != null) return sInstance.mCountsAsInPlay; else return false; } }
    public static float sCurrentScore => sInstance.currentScore;
    public static bool sIsDebug => sInstance.m_Debug;

    public static bool sHasMenu => sInstance.m_HasMenu;

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
    /// When the player restarts the whole game
    /// </summary>
    public static Action EventRestartGame;
    public static Action EventRestartCheckpoint;

    public static Action OnScoreChange;

    public static Action EventPauseEnter;
    public static Action EventPauseLeave;
    public static Action EventGameVictory;


    private void Awake()
    {
#if UNITY_EDITOR

#elif UNITY_STANDALONE
        UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
#endif

        mCountsAsInPlay = false;
        sInstance = this;

        OnGameReady += delegate { mCountsAsInPlay = true; mGameHasStarted = true; };

    }

    private void Start()
    {
        
        if (!m_HasMenu)
        {
            StartCoroutine(TemporaryStart());

        }
    }

    private bool isInPause = false;
    private void Update()
    {
        if(sGameHasStarted && Input.GetButtonDown("Pause"))
        {
            if (isInPause)
            {
                EventPauseEnter?.Invoke();
            }
            else
            {
                EventPauseLeave?.Invoke();
            }
            isInPause = !isInPause;
        }
    }

    public static void StartGame()
    {
        sInstance.StartCoroutine(sInstance.TemporaryStart());
    }
    public static void RestartGame()
    {
        EventRestartGame?.Invoke();
    }
    public static void RestartCheckpoint()
    {
        EventRestartCheckpoint?.Invoke();
    }

    private IEnumerator TemporaryStart()
    {
        OnGamePreparation?.Invoke();
        yield return null;
        OnGameReady?.Invoke();
    }

    


}
