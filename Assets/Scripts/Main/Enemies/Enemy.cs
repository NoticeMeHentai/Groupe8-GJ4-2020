using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor; 
#endif

//This script was originaly by Sylvain but taken over by Kilian when he suddenly left then Calvin added the Detection method. 
public abstract class Enemy : MonoBehaviour
{
    #region Editor Variables
    public bool mDebug = false;
    #endregion

    #region Private Variables
    public enum EnemyState
    {
        /// <summary>
        /// The enemy is staying at his place
        /// </summary>
        Idle,
        /// <summary>
        /// The enemy will move in a zone
        /// </summary>
        Patrol,
        /// <summary>
        /// The enemy does is distant actions and attack
        /// </summary>
        RangedCombat,
        /// <summary>
        /// The enemy does is close combat actions and attack
        /// </summary>
        AoeCombat,
        /// <summary>
        /// The enemy is stunned and is unable to make anything
        /// </summary>
        Stun,
        /// <summary>
        /// The enemy is dead
        /// </summary>
        Dead,
    }
    private EnemyState mCurrentState = EnemyState.Idle;
    [BoxedHeader("Main, Idle parameters", order = 0), Header("Basic Variables", order =1)]

    /// Life of the enemy
    [SerializeField] private float mMaxLife = 100.0f;
    /// randomize a little the life of the enemy. take the life and add a random modificateur, in addition. Warning : the second value will never be taken, but the closest one to it.
    [SerializeField] private Vector2 mLifeRndGap = new Vector2(-10, 10);
    /// Damage done by the enemy to one player
    ///[SerializeField] private float mDamage = 50.0f;
    /// Speed of displacement
    [SerializeField] private float mSpeed = 3.0f;
    /// Radius of perception
    [SerializeField] private float mTriggerRadius = 10.0f;
    [SerializeField] private float mTriggerAngle = 25f;
    /// The radius of avoidance of the agent
    [SerializeField] private float mAvoidanceRadius = 0.5f;
    private float m_MaxDistanceAttackFromOrigin = 25f;

    /// The spawn zone behavior found in parent spawn zone
    private SpawnZoneBehavior mSZB = default;
    /// NavMeshAgent of the enemy
    private NavMeshAgent mAgent = default;
    /// has the enemy a spawn zone in parent
    private bool hasSZBParented = false;
    // A list of every object that the enemy has at Start so that we can unparent the other ones so that they won't be destroyed
    private Transform[] mChildrenAtAwake;

    [Header("Idle Variables")]
    /// The time the enemy will pass in Idle, before changing to patrol if anything else occur
    [SerializeField] private float mAverageIdleDuration = 2.0f;
    /// Change the time in Idle; 
    [SerializeField, Tooltip("Random time modifier(in seconds) to apply to the average duration at each Idle enter. " +
        "This means the actual idle duration will be the average duration + a random value between this minus value and plus value. " +
        "Do not input a value higher than the idle average lol")]
    private float mIdleModifier = 1.0f;


    [Header("Patrol Variables")]
    /// The time the enemy will stay in patrol before switching to Idle
    [SerializeField] private float mAveragePatrolDuration = 5.0f;
    /// Change the time in Idle; 
    [SerializeField, Tooltip("Random time modifier(in seconds) to apply to the average duration at each Patrol enter. " +
        "This means the actual patrol duration will be the average duration + a random value between this minus value and plus value. " +
        "Do not input a value higher than the patrol average lol")]
    private float mPatrolModifier = 1.0f;
    /// In case of no spawn zone, the max displacement the enemy can make in one movement
    [SerializeField][Tooltip("In case there's no spawner, the maximal patrolling distance this enemy will be able to walk during patrol")]
    private float m_MaxDistancePatrolFromOrigin = 10.0f;


    [BoxedHeader("Attack parameters", order =0), Header("Distance Combat Variables", order = 1)]
    [Tooltip("The minimal and maximal distance on which the enemy can do a ranged attack")]
    [SerializeField] private Vector2 mRangedAttackRange = new Vector2(2f, 5f);
    /// the delay between two attacks of an enemy
    [SerializeField] private float mRangedAttackCooldown = 0.5f;

    /// The time the enemy is stopped  after activating its attack
    [Min(0.1f)] [SerializeField] private float mRangedAttackStopTime = 0.1f;
    ///  After using a ranged attack, the probablity to switch to aoe attack
    [Range(0, 1)] [SerializeField] private float mProbToSwitchToAoe = .5f;
    [Min(1)] [SerializeField] private int mMaxRangedAttacksBeforeSwitching = 3;

    [Header("Aoe Combat Variables")]
    [Tooltip("The minimal and maximal distance on which the enemy can do an aoe attack")]
    [SerializeField] private Vector2 mAOEAttackRange = new Vector2(1f, 3f);
    /// the time between two Aoe Attack
    [SerializeField] private float mAOEAttackCooldown = 1.0f;    
    /// The time the enemy is stopped  after activating its attack
    [Min(0.1f)] [SerializeField] private float mAOEAttackStopTime = 0.1f;
    /// After using an aoe attack, the probablity to switch to distant attack
    [Range(0, 1f)] [SerializeField] private float mProbToSwitchToRangedAttack = 0.5f;
    [Min(1)] [SerializeField] private int mMaxAOEAttacksBeforeSwitching = 3;



    [BoxedHeader("Other timings")]
    [SerializeField, Min(0.5f), Tooltip("When switching mode from distance to AOE, this is the time it takes before being able to try to switch again to distance"), Space]
    private float mMinimalTimeBeforeSwitchingAOEToDistance = 5f;
    [SerializeField, Min(0.5f), Tooltip("When switching mode from AOE to distance, this is the time it takes before being able to try to switch again to AOE")]
    private float mMinimalTimeBeforeSwitchingDistanceToAOE = 5f;

    [SerializeField, Tooltip("The minimal and maximal time it takes for the enemy to switch from AOE state to Distance state"), Space]
    private Vector2 mRandomTimeAOEToDistance = new Vector2(0.5f, 1f);
    [SerializeField, Tooltip("The minimal and maximal time it takes for the enemy to switch from Distance state to Attack state")]
    private Vector2 mRandomTimeDistanceToAOE = new Vector2(0.5f, 1f);
    [SerializeField, Min(0.5f), Tooltip("When the enemy dies, the time it takes before getting sent to the pool"), Space] private float mTimeUntilDeath = 3f;

    [SerializeField] private float mStunDuration = 2.0f;
    [SerializeField] private float mBlinkDuration = 0.05f;
    [SerializeField] private GameObject mEnemyMeshGO;
    [BoxedHeader("Slots")]
    public Renderer m_LifeBarRenderer;


    private bool _Debug { get => mDebug && GameManager.sIsDebug; }
#if UNITY_EDITOR
    [SerializeField, Disable, Multiline, HideLabel(order = 0)]
    public string mGizmosInfo = "GizmosInfo: Magenta: Trigger zone. Black: Agent collision radius. \n Red: Ranged range.  Blue: AOE range. \n White: Line towards next destination point."; 
#endif

    private Renderer mRend;
    protected Renderer _Rend { get { if (mRend == null) mRend = mEnemyMeshGO.GetComponent<Renderer>(); return mRend; } }

    private float mCurrentHP = 0f;
    private Material mLifeBarMat;

    private float mMaterialDissolveAlpha;
    private float mNextAOEAttack;
    private float mNextRangedAttack;
    private float mGoPatrolTime;
    private float mGoIdleTime;
    private float mNextSwitchAttackModeTime;
    private Vector3 mPathDestination = new Vector3();
    private Vector3 spawnOrigin = new Vector3();
    private int mCurrentAmountOfAttacks = 0;

    /// When in combat mode and not in range attack, the enemy will try to approach or flee from the target to be at range
    private float mSeekedDistanceToBeAtRange;
    private bool mSeekingToBeAtRange = false;

    private bool _IsIdleOver => Time.time > mGoPatrolTime;
    private bool _IsPatrolOver => Time.time > mGoIdleTime;


    /// Can we switch attack mode? (Cooldown)
    private bool _CanSwitchAttackMode => Time.time > mNextSwitchAttackModeTime;
    /// Can we attack aoe? (Cooldown)
    private bool _IsAOECooldownOver => Time.time > mNextAOEAttack;
    /// Can we ranged attack? (Cooldown)
    private bool _IsRangedCooldownOver => Time.time > mNextRangedAttack;
    /// The direction towards the player
    protected Vector3 _DirectionToPlayer => _VectorToPlayer.normalized;
    protected Vector3 _VectorToPlayer => (_TargetPosition - transform.position);
    /// The distance between the enemy and the player targeted
    private float _DistanceToTarget => _VectorToPlayer.magnitude;
    /// Is the target within the aoe range?
    private bool _TargetWithinAOERange => mAOEAttackRange.WithinRange(_DistanceToTarget);
    /// Is the target withing the ranged range and not within aoe range?
    private bool _TargetWithinRangedRange => mRangedAttackRange.WithinRange(_DistanceToTarget);
    private float mDetectionAngleInRadians;

    

    #endregion

    #region Properties
    /// Is the enemy idle or patrolling?
    public bool IsPatrolling => (int)mCurrentState <= 1;
    /// Is the enemy stunned, in ranged or aoe combat?
    public bool _IsAttacking => (int)mCurrentState > 1 && mCurrentState != EnemyState.Dead;
    public float AgentRadius => mAvoidanceRadius;
    public bool IsDead => mCurrentState == EnemyState.Dead;
    public delegate void EnemyWantToDie();
    public event EnemyWantToDie EnemyWillDie;


    protected float _DistanceToSpawnOrigin => Vector3.Distance(spawnOrigin.FlatOneAxis(Vector3Extensions.Axis.y, false), transform.position.FlatOneAxis(Vector3Extensions.Axis.y, false));
    protected Vector3 _TargetPosition => PlayerMovement.Position;

    public Bounds Bounds => _Rend.bounds;
    private bool mBusyCasting = false;

    #endregion

    #region BehaviorState Methods
    private void OnEnterState()
    {
        switch (mCurrentState)
        {
            case EnemyState.Idle:
                EnterIdle();
                break;
            case EnemyState.Patrol:
                EnterPatrol();
                break;
            case EnemyState.RangedCombat:
                EnterRangedCombat();
                break;
            case EnemyState.AoeCombat:
                EnterAoeCombat();
                break;
            case EnemyState.Stun:
                EnterStun();
                break;
            case EnemyState.Dead:
                EnterDead();
                break;
        }
    }

    private void OnUpdateState()
    {
        switch (mCurrentState)
        {
            case EnemyState.Idle:
                IdleUpdate();
                break;
            case EnemyState.Patrol:
                PatrolUpdate();
                break;
            case EnemyState.RangedCombat:
                RangedCombatUpdate();
                break;
            case EnemyState.AoeCombat:
                AoeCombatUpdate();
                break;
            case EnemyState.Stun:
                StunUpdate();
                break;
            case EnemyState.Dead:
                DeadUpdate();
                break;
        }
    }

    private void OnExitState()
    {
        switch (mCurrentState)
        {
            case EnemyState.Idle:
                ExitIdle();
                break;
            case EnemyState.Patrol:
                ExitPatrol();
                break;
            case EnemyState.RangedCombat:
                ExitRangedCombat();
                break;
            case EnemyState.AoeCombat:
                ExitAoeCombat();
                break;
            case EnemyState.Stun:
                ExitStun();
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (mCurrentState!= EnemyState.Dead)
        {
            if (mCurrentState == newState) return;
            OnExitState();
            mCurrentState = newState;
            OnEnterState(); 
        }
    }
    #endregion

    #region Private Methods

    protected void Awake()
    {
        mLifeBarMat = new Material(m_LifeBarRenderer.material);
        m_LifeBarRenderer.material = mLifeBarMat;
        mLifeBarMat.SetFloat("_Value", 1f);
        mMaxLife = mCurrentHP = mMaxLife + mLifeRndGap.Lerp(Random.value);
        mAgent = GetComponent<NavMeshAgent>();
        if (mAgent == null)
        {
            if (_Debug) Debug.LogWarning("No NavMeshAgent has been found on the enemy !");
            mAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        mAgent.radius = mAvoidanceRadius * 0.5f;
        mDetectionAngleInRadians = mTriggerAngle * Mathf.Deg2Rad;
        GameManager.OnPlayerDeath += delegate { if(_IsAttacking && mCurrentHP>0) ChangeState(EnemyState.Patrol); };
        CustomAwake(); 
    }

    protected void Start()
    {
        mAgent.Warp(transform.position);
        spawnOrigin = transform.position;
        if (transform.parent == null)
        {
            if (_Debug) Debug.Log("Enemy don't have parent spawn zone");
            SetDestination(NewRandomPoint());
        }
        else
        {
            mSZB = GetComponentInParent<SpawnZoneBehavior>();
            if (mSZB == null)
            {
                if (_Debug) Debug.LogWarning("Parent doesn't have a SpawnZoneBehaviour component!");
                hasSZBParented = false;
            }
            else
            {
                hasSZBParented = true;
                SetDestination(mSZB.CreateRandomPoint());
            }

        }
        EnemyManager.AddNewEnemy(this);
        //EnemyManager.AddToList(gameObject, transform);
        mAgent.autoRepath = true;
        CustomStart();

        //Getting Every children of this object to unparent what need to be at death
        mChildrenAtAwake = new Transform[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            mChildrenAtAwake[i] = child;
            i++;
        }
        
    }



    protected void Update()
    {
            OnUpdateState();
            Detection();
            CustomUpdate(); 
    }

    protected void FixedUpdate()
    {
        if (_IsAttacking && _DistanceToSpawnOrigin > m_MaxDistanceAttackFromOrigin)
            ChangeState(EnemyState.Patrol);
    }

    //private void StopSpeed(bool value)
    //{
    //    mAgent.speed = value ? mSpeed : 0;
    //}

    #endregion

    #region Idle
    private void EnterIdle()
    {
        mGoPatrolTime = Time.time + Mathf.Max(mAverageIdleDuration + Random.Range(-mIdleModifier, mIdleModifier), 0.25f);
        EnterIdleCustom();
        mAgent.isStopped = true;
    }
    private void IdleUpdate()
    {
        if (_IsIdleOver) ChangeState(EnemyState.Patrol);
        IdleUpdateCustom();
    }
    private void ExitIdle()
    {
        mAgent.isStopped = false;
        ExitIdleCustom(); 
    }
    #endregion

    #region Patrol
    private void EnterPatrol()
    {
        mGoIdleTime = Time.time + Mathf.Max(mAveragePatrolDuration + Random.Range(-mPatrolModifier, mPatrolModifier), 0.25f);
        if (hasSZBParented) SetDestination(mSZB.CreateRandomPoint());
        else SetDestination(NewRandomPoint());
        EnterPatrolCustom();
        mAgent.isStopped = false;
    }
    private void PatrolUpdate()
    {
        if (mAgent.pathStatus >= NavMeshPathStatus.PathPartial || mAgent.remainingDistance<0.1f || _IsPatrolOver) //If cannot reach destination or it's already completed or the patrol time allowed is over
            ChangeState(EnemyState.Idle);
        PatrolUpdateCustom();
    }
    private void ExitPatrol()
    {
        ExitPatrolCustom();
    }
    #endregion

    #region Distance
    private void RangedAttack()
    {
        mBusyCasting = true;
        mCurrentAmountOfAttacks++;
        Invoke(nameof(ResetIsStoppedToFalse), mRangedAttackStopTime);
        mNextRangedAttack = Time.time + mRangedAttackCooldown;
        RangedAttackCustom();
    }
    private void EnterRangedCombat()
    {
        mCurrentAmountOfAttacks = 0;
        mNextSwitchAttackModeTime = Time.time + mMinimalTimeBeforeSwitchingDistanceToAOE;
        EnterRangedCombatCustom();
    }

    private void RangedCombatUpdate()
    {
        if (!mAgent.isStopped)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_DirectionToPlayer), 45 * Time.deltaTime);

        if (_TargetWithinRangedRange)
        {
            if (mSeekingToBeAtRange == true)
            {
                mSeekingToBeAtRange = false;
                SetDestination(transform.position);
                ReachedDesiredRangeCustom();
            }
            if (_IsRangedCooldownOver) RangedAttack();
        }
        else if (!mAgent.isStopped) SeekMoveToRange();
        
        RangedCombatUpdateCustom();
    }
    private void ExitRangedCombat()
    {
        CancelInvoke(nameof(TryToSwitchAttackMethod));
        ExitRangedCombatCustom();
    }
    #endregion

    #region AOE
    private void AOEAttack()
    {
        mCurrentAmountOfAttacks++;
        mNextAOEAttack = Time.time + mAOEAttackCooldown;
        mAgent.isStopped = true;
        Invoke(nameof(ResetIsStoppedToFalse), mAOEAttackStopTime);
        AoeAttackCustom();
    }

    private void EnterAoeCombat()
    {
        mCurrentAmountOfAttacks = 0;
        mNextSwitchAttackModeTime = Time.time + mMinimalTimeBeforeSwitchingAOEToDistance; //Cannot switch mode before X seconds have passed
        EnterAoeCombatCustom();
    }
    private void AoeCombatUpdate()
    {
        if (!mAgent.isStopped)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_DirectionToPlayer), 45 * Time.deltaTime);

        if ((_TargetWithinAOERange))
        {
            if(mSeekingToBeAtRange == true)
            {
                mSeekingToBeAtRange = false;
                SetDestination(transform.position);
                ReachedDesiredRangeCustom();
            }
            if (_IsAOECooldownOver)
                AOEAttack();
        }
        else if (!mAgent.isStopped) SeekMoveToRange();
        AoeCombatUpdateCustom();
    }
    private void ExitAoeCombat()
    {
        CancelInvoke(nameof(TryToSwitchAttackMethod));
        ExitAoeCombatCustom();
    }
    #endregion

    #region Stun
    public void DoStun(float duration)
    {
        ChangeState(EnemyState.Stun);
        Invoke(nameof(Unstun), duration);
    }
    private void Unstun()
    {
        Trigger();
        if (mSZB != null) mSZB.GetTriggered();
        
    }
    private void EnterStun()
    {
        mAgent.isStopped = true;
        InvokeStunEnd(mStunDuration);
        EnterStunCustom();
    }
    private void StunUpdate()
    {
        StunUpdateCustom();
    }
    private void ExitStun()
    {
        mAgent.isStopped = false;
        ExitStunCustom();
    }
    #endregion

    #region Dead
    bool mChildShouldBeUnparented;
    private void EnterDead()
    {
        mAgent.isStopped = true;
        if(EnemyWillDie!= null) EnemyWillDie.Invoke();
        if (mSZB != null) mSZB.SortOutOfList(this);
        EnterDeadCustom();
        mMaterialDissolveAlpha = 0;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(DeathDissolve());
        int i = 0;
        foreach(Transform child in transform)
        {
            mChildShouldBeUnparented = true;
            while (i < mChildrenAtAwake.Length)
            {
                if (mChildrenAtAwake[i] == child)
                {
                    mChildShouldBeUnparented = false;
                    break;
                }
                i++;
            }
            if (mChildShouldBeUnparented)
            {
                child.transform.parent = null;
            }
        }
        Destroy(gameObject, mTimeUntilDeath);
    }
    private void DeadUpdate()
    {
        DeadUpdateCustom();
    }
    private void ExitDead()
    {
        ExitDeadCustom();
    }
    private void InvokeDeath()
    {
        Destroy(gameObject);
    }

    IEnumerator DeathDissolve()
    {
        while(mMaterialDissolveAlpha < mTimeUntilDeath)
        {
            mMaterialDissolveAlpha += Time.deltaTime/ mTimeUntilDeath;
            _Rend.material.SetFloat("_DissolveAmount", mMaterialDissolveAlpha);
            yield return null;
        }
        yield return null;
    }
    #endregion

    #region Damage
    /// <summary>
    /// Add damage to the enemy
    /// </summary>
    /// <param name="damage">the amount of damage dealt to the enemy</param>
    public void AddDamage(float damage)
    {   
        StartCoroutine(HitBlink());
        if (_Debug) Debug.Log("Enemy Add damage has been called");
        mCurrentHP -= damage;
        if (mCurrentHP <= 0) ChangeState(EnemyState.Dead);
        else AddDamageCustom();
        mLifeBarMat.SetFloat("_Value", Mathf.Clamp01( mCurrentHP / mMaxLife));
        if (!_IsAttacking)
        {
            DetectedEnemy();
            //PlayerMovement.OnDeath += IfPlayerDies;
            if (_TargetWithinAOERange) ChangeState(EnemyState.AoeCombat);
            else ChangeState(EnemyState.RangedCombat);
            if (_Debug) Debug.Log("[Enemy] Found target!");
        }
        
    }

    IEnumerator HitBlink()
    {
        _Rend.material.SetFloat("_HitBlinkValue", 1);
        yield return new WaitForSeconds(mBlinkDuration);

        _Rend.material.SetFloat("_HitBlinkValue", 0);
    }

    #region Slow
    /// <summary>
    /// Slows down the enemy by the given percent [0,1]
    /// </summary>
    /// <param name="percentage"></param>
    public void Slow(float percentage)
    {
        mAgent.speed = mSpeed * (1 - Mathf.Clamp01(percentage));
        SlowCustom();
    }
    #endregion


    #endregion

    #region Custom Methods
    #region Generals Custom
    protected virtual void CustomAwake()
    {

    }
    protected virtual void CustomStart()
    {

    }
    protected virtual void CustomUpdate()
    {

    }
    #endregion

    #region Idle Custom
    /// <summary>
    /// The custom method for a certan type of enemy
    /// </summary>
    protected virtual void EnterIdleCustom()
    {
        //Debug.Log("EnterIdleCustom has been called");
    }
    /// <summary>
    /// The custom method for a certan type of enemy
    /// </summary>
    protected virtual void IdleUpdateCustom()
    {
        //Debug.Log("IdleUpdateCustom has been called");
    }
    /// <summary>
    /// The custom method for a certan type of enemy
    /// </summary>
    protected virtual void ExitIdleCustom()
    {
        //Debug.Log("ExitIdleCustom has been called");
    }
    #endregion

    #region Patrol Custom

    protected virtual void EnterPatrolCustom()
    {
        //Debug.Log("EnterPatrolCustom has been called");
    }
    protected virtual void PatrolUpdateCustom()
    {
        //Debug.Log("PatrolUpdateCustom has been called");
    }
    protected virtual void ExitPatrolCustom()
    {
        //Debug.Log("ExitPatrolCustom has been called");
    }
    #endregion

    #region Range Custom
    /// <summary>
    /// Called when the enemy needs to go away or approach the player
    /// </summary>
    protected virtual void SeekingToBeAtRangeCustom()
    {

    }

    /// <summary>
    /// Called when the enemy needed to go away or approach the player and can attack again
    /// </summary>
    protected virtual void ReachedDesiredRangeCustom()
    {

    }

    /// <summary>
    /// Called when the enemy has found a target
    /// </summary>
    protected virtual void DetectedEnemy()
    {

    }
    #endregion

    #region Distance Custom
    protected virtual void RangedAttackCustom()
    {
        //Do the distant attack
    }
    protected virtual void EnterRangedCombatCustom()
    {
        if (_Debug) Debug.Log("EnterDistanceCombatCustom has been called");
    }
    protected virtual void RangedCombatUpdateCustom()
    {
        if (_Debug) Debug.Log("DistanceCombatUpdateCustom has been called");
    }
    protected virtual void ExitRangedCombatCustom()
    {
        if (_Debug) Debug.Log("ExitDistanceCombatCustom has been called");
    }
    #endregion

    #region Aoe Custom
    protected virtual void AoeAttackCustom()
    {
        //do AoeAttack
    }
    protected virtual void EnterAoeCombatCustom()
    {
        if (_Debug) Debug.Log("EnterAoeCombatCustom has been called");
    }
    protected virtual void AoeCombatUpdateCustom()
    {
        if (_Debug) Debug.Log("AoeCombatUpdateCustom has been called");
    }
    protected virtual void ExitAoeCombatCustom()
    {
        if (_Debug) Debug.Log("ExitAoeCombatCustom has been called");
    }
    #endregion

    #region Stun Custom
    protected virtual void EnterStunCustom()
    {
        if (_Debug) Debug.Log("EnterStunCustom has been called");
    }
    protected virtual void StunUpdateCustom()
    {
        if (_Debug) Debug.Log("StunUpdateCustom has been called");
    }
    protected virtual void ExitStunCustom()
    {
        if (_Debug) Debug.Log("ExitStunCustom has been called");
    }
    #endregion

    #region Dead Custom
    protected virtual void EnterDeadCustom()
    {
        if (_Debug) Debug.Log("EnterDeadCustom has been called");
    }
    protected virtual void DeadUpdateCustom()
    {
        if (_Debug) Debug.Log("DeadUpdateCustom has been called");
    }
    protected virtual void ExitDeadCustom()
    {
        if (_Debug) Debug.Log("ExitDeadCustom has been called");
    }
    #endregion

    #region Damage Custom
    protected virtual void AddDamageCustom()
    {
        if (_Debug) Debug.Log("enemy has taken damage");
    }
    #endregion

    #region Slow Custom
    protected virtual void SlowCustom()
    {
        if (_Debug) Debug.Log("Enemy has been slow down");
    }
    #endregion

    #endregion

    #region Infos
    /// <summary>
    /// return true if the agent has reached a destination or is unable to pursue is journey
    /// </summary>
    /// <returns></returns>
    private bool IsPathComplete()
    {
        float distance = (Vector3.Distance(mAgent.destination, mAgent.transform.position));
        bool value = distance <= mAgent.stoppingDistance || (!mAgent.hasPath || mAgent.velocity.sqrMagnitude == 0f);
        if (value && _Debug) Debug.Log("[Enemy] Completed path!");
        return value;

        //if (Vector3.Distance(mAgent.destination, mAgent.transform.position) <= mAgent.stoppingDistance
        //    || (!mAgent.hasPath || mAgent.velocity.sqrMagnitude == 0)) return true;
        //else if (Vector3.Distance(mAgent.destination, mAgent.transform.position) <= mDistanceToStop) return true;
        //return false;
    }

    /// <summary>
    /// Moves the agent to a certain range in order to use its skills (in combat mode)
    /// </summary>
    private void SeekMoveToRange()
    {
        if (!mBusyCasting)
        {
            if (_Debug) Debug.Log("[Enemy] Seeking a new point to be at range!");
            if (mCurrentState == EnemyState.RangedCombat) mSeekedDistanceToBeAtRange = mRangedAttackRange.Lerp(0.5f);
            else mSeekedDistanceToBeAtRange = mAOEAttackRange.Lerp(0.5f);
            float distanceLeft = _DistanceToTarget - mSeekedDistanceToBeAtRange;
            mSeekingToBeAtRange = true;
            SetDestination(transform.position + _DirectionToPlayer * distanceLeft);
            SeekingToBeAtRangeCustom();
        }
    }


    /// <summary>
    /// Create a random point in a certain range around the enemy. Is called if no spawn zone is detected.
    /// </summary>
    /// <returns></returns>
    public Vector3 NewRandomPoint()
    {
        Vector2 rndpoint = Random.insideUnitCircle * m_MaxDistancePatrolFromOrigin;
        Vector3 target = spawnOrigin + new Vector3(rndpoint.x , 0, rndpoint.y);
        //Debug.DrawLine()
        return target;
    }

    /// <summary>
    /// Detect if a player come in the trigger zone, and push the ennemy in distant combat state
    /// </summary>
    void Detection()
    {
        if (!_IsAttacking) //No reason to look for a target if there is none or we already have a target
        {
                Vector3 dir = _TargetPosition - transform.position;
                float distanceToPlayer = dir.magnitude;

                //If the player is within radius range and angle range
                if (mCurrentState!= EnemyState.Dead && distanceToPlayer <= mTriggerRadius 
                &&(PlayerMovement.Position - spawnOrigin).magnitude<m_MaxDistanceAttackFromOrigin
                    && Mathf.Acos(Vector3.Dot(dir.normalized, transform.forward))< mDetectionAngleInRadians)
                {
                    DetectedEnemy();
                    //PlayerMovement.OnDeath += IfPlayerDies;
                    if (_TargetWithinAOERange) ChangeState(EnemyState.AoeCombat);
                    else ChangeState(EnemyState.RangedCombat);
                    if (_Debug) Debug.Log("[Enemy] Found target!");
                }
        }
    }

    private void SetDestination(Vector3 newDestination)
    {
        
        mAgent.SetDestination(newDestination);
        if (_Debug) Debug.Log("[Enemy] Got a new destination!");
    }
    /// <summary>
    /// Method to activate if we're in the middle of a combat and the player dies. If there's another player alive,then seek it, else go to patrol
    /// </summary>
    private void IfPlayerDies()
    {
        ChangeState(EnemyState.Patrol);
        if (_Debug) Debug.Log("[Enemy] No player alive left, so boring...");
    }

    /// <summary>
    /// If an enemy of the same spawner sees an enemy, the entire group will also be triggered over time
    /// </summary>
    public void Trigger()
    {
        if (_TargetWithinAOERange) ChangeState(EnemyState.AoeCombat);
        else ChangeState(EnemyState.RangedCombat);
        DetectedEnemy();
    }


    private IEnumerator InvokeSwitchBool(bool value, float delay)
    {
        yield return new WaitForSeconds(delay);
        value = !value;
    }
    /// <summary>
    /// When doing a ranged or aoe attack, the enemy won't be able to move, and after the given amount of stop time, it'll be able to pursue the player again if needed
    /// </summary>
    private void ResetIsStoppedToFalse()
    {
        mAgent.isStopped = false;
        mBusyCasting = false;
        if (_CanSwitchAttackMode)
        {
            if ((mCurrentState == EnemyState.RangedCombat && Random.value <= mProbToSwitchToAoe) || mCurrentAmountOfAttacks == mMaxRangedAttacksBeforeSwitching)
            {
                mNextAOEAttack = Time.time + mAOEAttackCooldown;
                ChangeState(EnemyState.AoeCombat); //Give a cooldown in order to not execute aoe attack just after switching from ranged
            }
            else if (mCurrentState == EnemyState.AoeCombat && Random.value <= mProbToSwitchToRangedAttack || mCurrentAmountOfAttacks == mMaxAOEAttacksBeforeSwitching)
            {
                mNextRangedAttack = Time.time + mRangedAttackCooldown;
                ChangeState(EnemyState.RangedCombat);
            }

        }
        else if(_Debug) Debug.Log("[Enemy] is no longer stopped!");
    }
    /// <summary>
    /// After using a skill, there's a chance it'll change the skill after finishing it
    /// </summary>
    private void TryToSwitchAttackMethod()
    {
        if (_CanSwitchAttackMode)
        {
            if ((mCurrentState == EnemyState.RangedCombat && Random.value <= mProbToSwitchToAoe)) ChangeState(EnemyState.AoeCombat);
            else if (mCurrentState == EnemyState.AoeCombat && Random.value <= mProbToSwitchToRangedAttack) ChangeState(EnemyState.RangedCombat);

        }
    }

    private IEnumerator InvokeStunEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        Detection();
    }

#if UNITY_EDITOR

    private void DrawDisc(float radius, Color color)
    {
        Handles.color = color;
        Handles.DrawWireDisc(transform.position, Vector3.up, radius);
    }
    /// <summary>
    /// Draw some helper gizmos in the editor
    /// </summary>
    void OnDrawGizmos()
    {
        Handles.color = Color.magenta;
        float halfAngle = mTriggerAngle * 0.5f;
        Vector3 forwardRight = Quaternion.AngleAxis(halfAngle, Vector3.up) * transform.forward*mTriggerRadius;
        Handles.DrawWireArc(transform.position, Vector3.up, forwardRight, -mTriggerAngle, mTriggerRadius);
        Vector3 forwardLeft = Quaternion.AngleAxis(-halfAngle, Vector3.up) * transform.forward * mTriggerRadius;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + forwardRight);
        Gizmos.DrawLine(transform.position, transform.position + forwardLeft);

        DrawDisc(mAvoidanceRadius, Color.black);

        DrawDisc(mAOEAttackRange.y, Color.blue);
        DrawDisc(mAOEAttackRange.x, Color.blue * 0.75f);

        DrawDisc(mRangedAttackRange.y, Color.red);
        DrawDisc(mRangedAttackRange.x, Color.red*0.75f);

        DrawDisc(m_MaxDistancePatrolFromOrigin, Color.black);
        DrawDisc(m_MaxDistanceAttackFromOrigin, Color.green);

        Gizmos.color = Color.white;
        if(EditorApplication.isPlaying)
        if (mAgent != null && mAgent.hasPath) Gizmos.DrawLine(transform.position, mAgent.destination);
    }
#endif
    #endregion
}
