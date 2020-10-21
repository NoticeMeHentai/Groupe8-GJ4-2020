using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CALVIN
//TO DO : 
//THERE SEEMS TO BE A BUG WHEN USING ADD SIZE : THE QUEUE WILL ENQUEUE THE LAST GAMEOBJECT TWICE - ?
public class PoolingSystem
{

    #region private variables
    private class Pool
    {
        public string poolName;
        public List<GameObject> objectsInPoolList;
        public Queue<GameObject> objectsAvailable;
        public GameObject gameObjectInThisPool;
        public Transform parentGameObject;
    }
    private static Dictionary<string, Pool> poolsDictionnary = new Dictionary<string, Pool>();

    #endregion
    
    #region Pooling Methods

    #region Preload
    /// <summary>
    /// Loads a pool with the given obj, amount and optional transform to parent to. Adds every obj created to the array 
    /// objectsInPoolArray and to objectsAvailable to act as a queue of availableObj.
    /// </summary>
    /// <param name="gameObjectToSpawn"> Your gameObject that needs a pool </param>
    /// <param name="howManyToLoad"> How many of said gameObject copies do you need to preload, 5 by default </param>
    /// <param name="optionalParentTransform"> Optional transform to set as parent of every copy of spawned gameObject </param>
    public static void Preload(GameObject gameObjectToSpawn, int howManyToLoad = 5, Transform optionalParentTransform = null)
    {
        if (poolsDictionnary.ContainsKey(gameObjectToSpawn.name))
        {
            //Debug.LogWarning(gameObjectToSpawn.name + "already has a pool but your tried to preload it, wtf bro");
            return;
        }

        if (optionalParentTransform == null)
        {
            GameObject optionalParentObject = new GameObject(gameObjectToSpawn.name + " Holder"); //sets up a parent to keep pool's objs in one place
            optionalParentTransform = optionalParentObject.transform;
        }

        Pool pool = new Pool();
        pool.objectsAvailable = new Queue<GameObject>();
        pool.objectsInPoolList = new List<GameObject>();
        pool.parentGameObject = optionalParentTransform;
        pool.poolName = gameObjectToSpawn.name;
        pool.gameObjectInThisPool = gameObjectToSpawn;
        poolsDictionnary.Add(gameObjectToSpawn.name, pool);

        PoolAddSize(pool, howManyToLoad, optionalParentTransform);
    }
    #endregion

    #region GetFromPool & GetFromPoolMultiple
    /// <summary>
    /// first checks if the pool exists, if it does then :
    /// Returns an available GameObject from the pool and optionaly sets its parent to optionalParentTransform
    /// if the pool for this object does not exist, Preloads one and then returns an available object from the new pool
    /// </summary>
    /// <param name="gameObjectToGet">The gameObject you are searching for</param>
    /// <param name="optionalParentTransform">The optional tranform you wish the object to be a child of</param>
    /// <returns></returns>
    public static GameObject GetFromPool(GameObject gameObjectToGet, bool setActivate = true, Transform optionalParentTransform = null)
    {
        if (poolsDictionnary.ContainsKey(gameObjectToGet.name))
        {
            if(poolsDictionnary[gameObjectToGet.name].objectsAvailable.Count > 0)
            {
                if(poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek() != null)
                {
                    if (setActivate)
                        poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().SetActive(true);

                    if(optionalParentTransform != null)
                        poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().transform.parent = optionalParentTransform;

                    return poolsDictionnary[gameObjectToGet.name].objectsAvailable.Dequeue();                    
                }
                else
                {
                    poolsDictionnary[gameObjectToGet.name].objectsAvailable.Dequeue();
                    return GetFromPool(gameObjectToGet, setActivate, optionalParentTransform);
                }
            }
            else
            {
                Debug.LogWarning("WOA, THE POOL FOR " + gameObjectToGet.name + " WAS EMPTY SO I RESIZED IT, MAKE SURE TO PRELOAD IT BIGGER");
                PoolAddSize(poolsDictionnary[gameObjectToGet.name], Mathf.CeilToInt(poolsDictionnary[gameObjectToGet.name].objectsInPoolList.Count * 0.3f), optionalParentTransform);
                return GetFromPool(gameObjectToGet, setActivate, optionalParentTransform);
            }
        }
        else
        {
            Debug.LogWarning("WOA, THERE IS NO POOL FOR " + gameObjectToGet.name + " SO I CREATED ONE. (make sure you use Preload to avoid fps drops and unwanted behavior)");
            Preload(gameObjectToGet, 5, optionalParentTransform);
            return GetFromPool(gameObjectToGet, setActivate, optionalParentTransform);
        }
    }

    /// <summary>
    /// first checks if the pool exists, if it does then :
    /// Returns an available GameObject from the pool and optionaly sets its parent to optionalParentTransform
    /// if the pool for this object does not exist, Preloads one and then returns an available object from the new pool
    /// </summary>
    /// <param name="gameObjectToGet">The gameObject you are searching for</param>
    /// <param name="position">Your gameObject's starting world position</param>
    /// <param name="rotation">Your gameObject's starting orientation</param>
    /// <param name="optionalParentTransform">The optional tranform you wish the object to be a child of</param>
    /// <returns></returns>
    public static GameObject GetFromPool(GameObject gameObjectToGet, Vector3 position, Quaternion orientation, bool setActive = true, Transform optionalParentTransform = null)
    {
        if (poolsDictionnary.ContainsKey(gameObjectToGet.name))
        {
            if (poolsDictionnary[gameObjectToGet.name].objectsAvailable.Count > 0)
            {
                if (poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek() != null)
                {
                    if (optionalParentTransform != null)
                        poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().transform.parent = optionalParentTransform;

                    poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().transform.position = position;
                    poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().transform.rotation = orientation;

                    if (setActive)
                        poolsDictionnary[gameObjectToGet.name].objectsAvailable.Peek().SetActive(true);

                    return poolsDictionnary[gameObjectToGet.name].objectsAvailable.Dequeue();
                }
                else
                {
                    poolsDictionnary[gameObjectToGet.name].objectsAvailable.Dequeue();
                    return GetFromPool(gameObjectToGet, position, orientation, setActive, optionalParentTransform);
                }
            }
            else
            {
                //Debug.LogWarning("WOA, THE POOL FOR " + gameObjectToGet.name + " WAS EMPTY SO I RESIZED IT, MAKE SURE TO PRELOAD IT BIGGER");
                PoolAddSize(poolsDictionnary[gameObjectToGet.name], Mathf.CeilToInt(poolsDictionnary[gameObjectToGet.name].objectsInPoolList.Count * 0.3f), optionalParentTransform);
                return GetFromPool(gameObjectToGet, position, orientation, setActive, optionalParentTransform);
            }
        }
        else
        {
            //Debug.LogWarning("WOA, THERE IS NO POOL FOR " + gameObjectToGet.name + " SO I CREATED ONE. (make sure you use Preload to avoid fps drops and unwanted behavior)");
            Preload(gameObjectToGet, 5, optionalParentTransform);
            return GetFromPool(gameObjectToGet, position, orientation, setActive, optionalParentTransform);
        }
    }

    /// <summary>
    /// Returns an array of size "howManyObjects" from a pool of "gameObjectToGet" using GetFromPool() and filling the given array up until it is full
    /// </summary>
    /// <param name="gameObjectToGet">The gameObject you wish to get</param>
    /// <param name="howManyObjects">How many clones of that gameObject do you need</param>
    /// <param name="optionalParentTransform"></param>
    /// <param name="gameObjectsArray">The array that will be filled up with gameObjects until it has reached capacity</param>
    /// <returns></returns>
    public static GameObject[] GetFromPoolMultiple(GameObject gameObjectToGet, GameObject[] gameObjectsArray, int howManyObjects, Transform optionalParentTransform = null)
    {
        for (int i = 0; i < gameObjectsArray.Length; i++)
        {
            gameObjectsArray[i] = GetFromPool(gameObjectToGet, optionalParentTransform);
        }
        return gameObjectsArray;
    }
    #endregion

    #region SendToPool
    /// <summary>
    /// Sends the gameObject given back to it's pool and deactivates it. 
    /// </summary>
    /// <param name="gameObjectToSend">Your gameObject that needs to be put back in the pool</param>
    public static void SendToPool(GameObject gameObjectToSend)
    {
        if (poolsDictionnary.ContainsKey(gameObjectToSend.name))
        {
            poolsDictionnary[gameObjectToSend.name].objectsAvailable.Enqueue(gameObjectToSend);
            gameObjectToSend.SetActive(false);
        }
        else
        {
            Debug.LogWarning("WOA, THERE IS NO POOL FOR " + gameObjectToSend.name + " SO I CREATED ONE. (make sure you use Preload to avoid fps drops and unwanted behavior)");
            Preload(gameObjectToSend, 5);
            SendToPool(gameObjectToSend);
        }
    }

    /// <summary>
    /// Instantiates the given number of clones of a given pool's gameObject and feeds it to it's list
    /// </summary>
    /// <param name="pool">Your pool that you wish to resize</param>
    /// <param name="howManyItemsToAdd">by how much do you wish to resize</param>
    /// <param name="parentTransform">Where to attach the clones, by default fetches the pool's parent</param>
    private static void PoolAddSize(Pool pool, int howManyItemsToAdd, Transform parentTransform)
    {
        if(parentTransform == null)
            parentTransform = pool.parentGameObject;

        pool.objectsInPoolList.Capacity += howManyItemsToAdd;
        int poolCurrentCount = pool.objectsInPoolList.Count;

        for (int i = poolCurrentCount; i < pool.objectsInPoolList.Capacity; i++)
        {
            pool.objectsInPoolList.Add(GameObject.Instantiate(pool.gameObjectInThisPool, parentTransform));
            pool.objectsInPoolList[i].name = pool.gameObjectInThisPool.name; //Because otherwise Unity will name it gameObjectName + "(clone)"
            SendToPool(pool.objectsInPoolList[i]);
        }
    }
    #endregion

    #endregion
}
