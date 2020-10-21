using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; 
#endif

public class SpawnZoneBehavior : MonoBehaviour
{
    IEnumerator SpreadTrigger(Enemy enemy)
    {
        //Calculates a random delay time, waits for it, then triggers the enemy.
        yield return new WaitForSeconds(mMinMaxTriggerSpreadDelay.Random());
        enemy.Trigger();
    }
    #region Public Properties
    /// <summary>
    /// The radius of spawn
    /// </summary>
    public float mRadiusOfSpawn = 5.0f;
    /// <summary>
    /// The total number of different enemies that have to be spawned to this location
    /// </summary>
    public int mTotalNumberOfSpawn = 5;
    /// <summary>
    /// List of differents prefabs of enemies stored for this spawn zone
    /// </summary>
    [ReorderableList(ListStyle.Boxed, fixedSize: true)]public GameObject[] mEnemyPrefabList = new GameObject[3];
    [MinMaxSlider(0f,1f)]public Vector2 mEnemyTypeRepartition = new Vector2(0.33f, 0.66f);
    /// <summary>
    /// The radius that will create the patrol zone. All spawned enemies will stay in this zone, in patrol state
    /// </summary>
    public float mRadiusOfPatrol = 10.0f;
    /// <summary>
    /// List of the enemies spawned by this spawn zone
    /// </summary>
    [HideInInspector] public List<Enemy> mEnemiesList;
    public Vector2 mMinMaxTriggerSpreadDelay = new Vector2(0.5f, 2f);
    #endregion
    #region Private Properties
    private bool mHasBeenTriggered = false;
    #endregion
    #region Public Methods
    void Start()
    {
        Spawn();
    }

    public Vector3 CreateRandomPoint()
    {
        Vector2 rndPoint = Random.insideUnitCircle * mRadiusOfPatrol;
        Vector3 rndPointScaled = new Vector3((rndPoint.x * transform.localScale.x) + transform.position.x,
                                               transform.position.y,
                                               (rndPoint.y * transform.localScale.z) + transform.position.z);
        return rndPointScaled;
    }

    public void GetTriggered()
    {
        if (!mHasBeenTriggered)
        {
            mHasBeenTriggered = true;
            for (int i = 0; i < mEnemiesList.Count; i++)
            {
                StartCoroutine(SpreadTrigger(mEnemiesList[i].GetComponent<Enemy>()));
            } 
        }
    }
    public void SortOutOfList(Enemy enemyScript)
    {
        mEnemiesList.Remove(enemyScript);
    }
    #endregion

    #region Private Methods
    private void Spawn()
    {

        if (mEnemyPrefabList.Length>0)
        {
            //Get a randomisation of the prefab spawn.
            for (int i = 0; i < mTotalNumberOfSpawn; i++)
            {
                GameObject prefabToSpawn = mEnemyPrefabList[0];
                float randomValue = Random.value;
                //if the rnd is in the percentage number of the prefab, it will instantiate the prefab. E.g. if rnd = 25 and  percentagelist = 83, it will instantiate.
                if (mEnemyTypeRepartition.WithinRange(randomValue) && mEnemyPrefabList[1] != null) prefabToSpawn = mEnemyPrefabList[1];
                else if (mEnemyTypeRepartition.y <= randomValue && mEnemyPrefabList[2] != null) prefabToSpawn = mEnemyPrefabList[2];

                Vector3 positionToSpawn = transform.position;

                bool canSpawn = true;
                for(int counter = 0; counter < 5; counter++)
                {
                    //Check if no other prefab is already at the place
                    Vector2 placeRandom = Random.insideUnitCircle * mRadiusOfSpawn;
                    positionToSpawn = new Vector3(transform.position.x + placeRandom.x, transform.position.y, transform.position.z + placeRandom.y);
                    bool canUsePosition = true;
                    if (mEnemiesList != null)
                    {
                        for (int k = 0; k < mEnemiesList.Count; k++)
                        {
                            float distanceBetween = (mEnemiesList[k].transform.position - positionToSpawn).magnitude + mEnemiesList[k].AgentRadius;
                            if (distanceBetween < prefabToSpawn.GetComponent<Enemy>().AgentRadius)
                            {
                                canUsePosition = false;
                                break;
                            }
                        }
                    }
                    if (canUsePosition)
                    {
                        canSpawn = true;
                        break; //Else look for another point
                    }
                }
                if (canSpawn == true)
                {
                    GameObject enemy = Instantiate(prefabToSpawn, positionToSpawn, Quaternion.identity, transform);
                    mEnemiesList.Add(enemy.GetComponent<Enemy>());
                    //Debug.Log("Spawn Zone added enemy to its list");
                }

            } 
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, mRadiusOfSpawn);
        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, Vector3.up, mRadiusOfPatrol);
    } 
#endif
    #endregion
}
