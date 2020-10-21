using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [Tooltip("If there is no menu (in a separate scene) then the gameplay starts right away.")]
    public bool mHasMenu = false;
    [Header("Gameplay")]
    public float m_MaxCoreHP = 100f;
    public float m_StartingCurrency = 150f;
    public float m_CurrencyGainRate = 8f;
    public float mMaxTime = 120f;
    public Transform m_ObjectiveToDefend;


    private float mCurrentPoints = 0;
    private EnemyFollowPoint[] enemyFollowPoints;
    private float currentCurrency = 0;
    private float currentTime = 0;


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
    private float mCurrentHPLeft = 0;
    /// <summary>
    /// Has the game truly started?
    /// </summary>
    private bool mGameHasStarted = false;
    /// <summary>
    /// Well, yeah, total score
    /// </summary>
    private float mTotalScore = 0;


    public static float sCurrentHP => sInstance.mCurrentHPLeft;
    /// <summary>
    /// Has the game really started?
    /// </summary>
    public static bool sGameHasStarted => sInstance.mGameHasStarted;
    /// <summary>
    /// Current time left for gameplay
    /// </summary>
    public static float sTimeLeft => sInstance.mCurrentTimeLeft;
    /// <summary>
    /// Current points won
    /// </summary>
    public static float sCurrentPoints { get { return sInstance.mCurrentPoints; } }
    /// <summary>
    /// The game is actually being played, meaning the countdown is going on.
    /// The game may be stopped for a reason but the player could still move, but it doesn't count as playing
    /// </summary>
    /// 
    public static float sCurrentRatioHP => sCurrentHP / sInstance.m_MaxCoreHP;
    
    public static EnemyFollowPoint[] sEnemyFollowPoints { get
        {
            if (sInstance.enemyFollowPoints == null) sInstance.enemyFollowPoints = GameObject.FindObjectsOfType<EnemyFollowPoint>();
            return sInstance.enemyFollowPoints;
        } }

    public static bool sCountsAsPlaying { get { if (sInstance != null) return sInstance.mCountsAsInPlay; else return false; } }
    public static Vector3 sObjectiveToDefendPosition => sInstance.m_ObjectiveToDefend.position;
    public static float sCurrentCurrency { get { return sInstance.currentCurrency; }
    private set
        {
            sInstance.currentCurrency = value;
            OnCurrencyChange?.Invoke();
        }
    }

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
    public static Action OnGameOverNoLivesLeft;
    /// <summary>
    /// When the player wins
    /// </summary>
    public static Action OnGameOverTimeRanOut;
    /// <summary>
    /// When the player restarts the game
    /// </summary>
    public static Action OnRestart;

    public static Action OnCoreHit;
    public static Action OnCurrencyChange;

    public static void AddCurrency(float currencyAmount)
    {
        sInstance.currentCurrency += currencyAmount;
        if (OnCurrencyChange != null) OnCurrencyChange();
    }

    public static void DeductCurrency(float amount)
    {
        sInstance.currentCurrency -= amount;
        if (OnCurrencyChange != null) OnCurrencyChange();
    }

    private void Awake()
    {
#if UNITY_STANDALONE
        UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
#endif
        mCountsAsInPlay = false;
        sInstance = this;
        
        OnGamePreparation += delegate
        {
            //HUDManager.Enable(true);
            mCurrentTimeLeft = mMaxTime;
            mCurrentHPLeft = m_MaxCoreHP;
            currentCurrency = m_StartingCurrency;
            OnCurrencyChange?.Invoke();
            Debug.Log("hurr durr");
        };

        OnGameReady += delegate { mCountsAsInPlay = true; mGameHasStarted = true; };

    }

    private void Start()
    {
        
        if (!mHasMenu)
        {
            StartCoroutine(TemporaryStart());

        }
    }


    private void Update()
    {

        if (mCountsAsInPlay)
        {
            mCurrentTimeLeft -= Time.deltaTime;
            currentTime += Time.deltaTime;
            if (currentTime > 1)
            {
                currentTime -= 1;
                currentCurrency += m_CurrencyGainRate;
                OnCurrencyChange?.Invoke();
            }
            if (mCurrentTimeLeft < 0) OnGameOverTimeRanOut();
        }
    }

    private IEnumerator TemporaryStart()
    {
        OnGamePreparation?.Invoke();
        yield return null;
        OnGameReady?.Invoke();
    }

    public static void HitCore(float damage)
    {
        sInstance.mCurrentHPLeft = Mathf.Max(sCurrentHP - damage, 0);
        if (sCurrentHP < 0)
        {
            if (OnGameOverNoLivesLeft != null) OnGameOverNoLivesLeft();
            Debug.Log("No more hp!");
        }
        OnCoreHit?.Invoke();

    }
}
