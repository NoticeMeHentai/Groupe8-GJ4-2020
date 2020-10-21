using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class MeshScatterer : MonoBehaviour
{
    #region public variables
    [HideInInspector]
    public bool cooldownBool, placeOn, firstClick;
    [HideInInspector]
    public Vector2 mousePosStore, currentMousePos;
    [HideInInspector]
    public List<GameObject> paintedGameObjectsList = new List<GameObject>(); //list of all game objects painted during the current stroke to be fed in VertexPainterEditor's Stack for Undo

    [Header("Mesh Settings"), Tooltip("tick only if your object's local pivot is not at the base of the object")]
    public bool pivotPointIsAtCenter = false;
    [Tooltip("Hides Unity's tranform/rotation/... handles for easier use of tool")]
    public bool hideUnityGizmos = false;
    [Tooltip("works as if you put this number in XYZ of the transform's scale")]
    public float objectScaleMultiplier = 1;
    [Tooltip("the minimum scale for the object to be spawned (doesn't apply to Scale Random)")]
    public float minObjectScale = 0.2f;
    [Tooltip("multiplies the object's final scale within a random range of -this to +this")]
    public float scaleRandom = 0.1f;
    [Tooltip("rotates the gameobjects randomly in degrees within this range")]
    public float rotationRandom = 180;

    [Header("Brush Settings"), Tooltip("Off : place textures one at a time. On : place multiple objects following brush texture")]
    public bool useTexture = true;
    [Tooltip("brush scale")]
    public float spawnPatchLength = 3;
    [Tooltip("multiplies how many objects spawned by stroke")]
    public float density = 0.5f;
    [Tooltip("introduces some randomness in each objects position")]
    public float positionJitter = 0f;

    [Header("Mesh")]
    public GameObject yourGameObject;

    [Header("Optional Brush Texture")]
    public Texture2D brushAlphaTex;

    #endregion

    #region private variables
    GameObject yourGameObjectsHolder;
    GameObject yourObjectClone;
    float maxMouseDistance = 10;
    Event e;
    #endregion

    #region CustomUpdate
    void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnGUI()
    {
        e = Event.current;
    }

    //This works with EditorApplication.update in OnEnable() to call scene update, it is then calmed down here with the i counter to have a fake update by calling CustomUpdate()
    int i = 0;
    private void Update()
    {
        i++;
        if(i > 10)
        {
            CustomUpdate();
            //DrawHandles();
            i = 0;
        }
    }

    private void CustomUpdate()
    {
        if(cooldownBool == false && placeOn == true)
        {
            if (Vector2.Distance(mousePosStore, currentMousePos) > maxMouseDistance || firstClick == true) //firstclick is only true on the frame the mouse key is pressed, so that it will spawn a GO regardless of distance
            {
                mousePosStore = currentMousePos; //set mousePosStore with current mouse pos
                SpawnGameObjectManager();
                firstClick = false;
            }
        }
    }
    #endregion

    #region Object Spawning Methods

    /// <summary>
    /// Called from the CustomUpdate() when mousebutton is pressed, will cast a ray 
    /// at current mouse position and then call the appropriate object spawning functions
    /// </summary>
    private void SpawnGameObjectManager()
    {
        Vector3 mousePosV3 = currentMousePos;
        mousePosV3.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosV3.y;

        Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosV3);

        RaycastHit raycast;
        if (Physics.Raycast(ray, out raycast))
        {
            if (!yourGameObjectsHolder)
                yourGameObjectsHolder = new GameObject("YourGameObjectsHolder");

            if(raycast.collider.gameObject == transform.gameObject && yourGameObject.GetComponent<MeshFilter>() != null)
            {
                if (useTexture)
                    SpawnPatchByTexture(raycast);
                else
                    paintedGameObjectsList.Add(SpawnObject(raycast, 1f)); //Since we're only spawning one, adding it directly to the list
            }

        }
    }

    /// <summary>
    /// Takes the original raycast from mouse position and creates a grid that will spawn objects periodically
    /// </summary>
    /// <param name="raycast">The original raycast hit information from the mouse raycast</param>
    private void SpawnPatchByTexture(RaycastHit raycast)
    {
        if (brushAlphaTex.isReadable)
        {
            Vector2 startingPoint = new Vector2(raycast.point.x - spawnPatchLength / 2, raycast.point.z - spawnPatchLength / 2); //feeds raycast point original information in a V2

            int pointsNumberOnX = Mathf.CeilToInt(spawnPatchLength * density); //how many points are there on x axis
            int pointsNumberOnY = Mathf.CeilToInt(spawnPatchLength * density);

            int pixelYOffsetAmount = brushAlphaTex.height / pointsNumberOnY;
            int pixelXOffsetAmount = brushAlphaTex.width / pointsNumberOnY;

            Vector2 newPoint = startingPoint;

            GameObject[] objectsToCombine = new GameObject[pointsNumberOnX * pointsNumberOnY]; //creating the array that will be given to CombineMeshes()
            int currentObjectNumber = 0;

            //Loops through the axis by a grid like behavior to spawn object periodically by raycasting and calling SpawnObject()
            for (int i = 0; i < pointsNumberOnX; i++)
            {
                for (int ib = 0; ib < pointsNumberOnY; ib++)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3((newPoint.x + Random.Range(-positionJitter, positionJitter)), 10000f, (newPoint.y + Random.Range(-positionJitter, positionJitter))), Vector3.down, out hit, Mathf.Infinity))
                    {
                        if(raycast.collider.gameObject == transform.gameObject)
                        {
                            float getPixelOnBrush = brushAlphaTex.GetPixel((pixelYOffsetAmount * ib), pixelXOffsetAmount * i).r; // gets the red channel's intensity at given pixel coordinates

                            if(getPixelOnBrush > minObjectScale)
                            {
                                objectsToCombine[currentObjectNumber] = SpawnObject(hit, getPixelOnBrush);
                            }
                            if(objectsToCombine[currentObjectNumber] != null)
                                currentObjectNumber++;
                        }
                    }
                    newPoint.y += spawnPatchLength / pointsNumberOnY;
                }
                newPoint.y = startingPoint.y;
                newPoint.x += spawnPatchLength / pointsNumberOnX;
            }
            if(objectsToCombine != null)
            {
                CombineMeshes(objectsToCombine, currentObjectNumber);
            }
        }
        else
            Debug.LogError("Attention : your " + brushAlphaTex.name + " is not set to readable, please check the 'Read/Write enabled' CheckBox on the texture import page");
    }

    /// <summary>
    /// the method that will instantiate the actual mesh, at mouse position and adds jitter such as random rotation
    /// it's override is for spawning objects from texture data and can be found below
    /// </summary>
    /// <param name="raycast"></param>
    private GameObject SpawnObject(RaycastHit raycast, float scale)
    {
        if (scale > minObjectScale)
        {
            // Instantiate gameObject at raycast point, lookRotation with raycast's normal, and parents it to the gameHolderOBJ
            yourObjectClone = Instantiate(yourGameObject, raycast.point, Quaternion.LookRotation(raycast.normal), yourGameObjectsHolder.transform);

            yourObjectClone.transform.rotation = Quaternion.AngleAxis(90, yourObjectClone.transform.right) * yourObjectClone.transform.rotation;
            yourObjectClone.transform.rotation = Quaternion.AngleAxis(Random.Range(-rotationRandom, rotationRandom), yourObjectClone.transform.up) * yourObjectClone.transform.rotation;

            if (scaleRandom > 0)
                yourObjectClone.transform.localScale *= (scale * (Random.Range(-scaleRandom, scale) + 1)); // takes into account the scale sent from spawnPatch's texture
            else
                yourObjectClone.transform.localScale *= scale;

            yourObjectClone.transform.localScale *= objectScaleMultiplier;

            //recenters object by taking localscale into account in case the pivot point is at the center of the object
            if (pivotPointIsAtCenter == true)
                yourObjectClone.transform.Translate(0, ((yourObjectClone.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * yourObjectClone.transform.localScale.y) / 2), 0);

            return yourObjectClone;
        }
        else
            return null;
    }

    private void CombineMeshes(GameObject[] objectsToCombine, int numberOfObjectsToSpawn)
    {
        CombineInstance[] combinedMeshes = new CombineInstance[numberOfObjectsToSpawn]; //creates an array of combinedMeshes of length = total of points on the grid

        //populating the meshCombiner to unify the mesh
        for(int objectNumber = 0; objectNumber < numberOfObjectsToSpawn; objectNumber++)
        {
            if (objectNumber == (numberOfObjectsToSpawn -1))
            {
                combinedMeshes[objectNumber].mesh = objectsToCombine[objectNumber].GetComponent<MeshFilter>().sharedMesh;
                combinedMeshes[objectNumber].transform = objectsToCombine[objectNumber].transform.localToWorldMatrix;

                Mesh combinedMeshFinal = new Mesh(); 
                combinedMeshFinal.CombineMeshes(combinedMeshes);
                objectsToCombine[objectNumber].GetComponent<MeshFilter>().sharedMesh = combinedMeshFinal; //combinedSharedMesh.CombineMeshes(meshCombiner);
                objectsToCombine[objectNumber].transform.position = Vector3.zero;
                objectsToCombine[objectNumber].transform.localScale = Vector3.one;
                objectsToCombine[objectNumber].transform.rotation = Quaternion.identity;

                paintedGameObjectsList.Add(objectsToCombine[objectNumber]); //Feeding the final gameObject to the list
            }
            else
            {
                combinedMeshes[objectNumber].mesh = objectsToCombine[objectNumber].GetComponent<MeshFilter>().sharedMesh;
                combinedMeshes[objectNumber].transform = objectsToCombine[objectNumber].transform.localToWorldMatrix;
                DestroyImmediate(objectsToCombine[objectNumber].gameObject);
            }
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Vector3 mousePosV3 = currentMousePos;
        mousePosV3.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosV3.y;

        Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosV3);

        Quaternion rot = Quaternion.identity;

        RaycastHit raycast;
        if (Physics.Raycast(ray, out raycast))
        {
            rot = Quaternion.LookRotation(raycast.normal);
            Gizmos.DrawRay(raycast.point, raycast.normal);
            Handles.Disc(rot, raycast.point, raycast.normal, spawnPatchLength * 0.5f, false, 0.5f);
        }
    }
}
#endif
