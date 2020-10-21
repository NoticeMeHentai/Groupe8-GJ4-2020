//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ElementHider : MonoBehaviour
//{
//    [Tooltip("How many frames does it take to check if the player is behind a surface or not?")]
//    [Min(3)]public float mCheckFrequency = 5; //Every X frames we check if the player is visible
//    [Tooltip("Speed at which a material to turn opaque or transparent")]
//    [Min(1f)] public float mTransitionSpeed = 5f;
//    [Tooltip("Distance behind the camera from which the raycast will be thrown")]
//    [Min(5f)] public float mMinDistance = 10f;
//    public bool mDebug = false;

//    private bool _Debug => mDebug && GameManager.IsDebug;


//    private float mCounter = 0f; //Frame counter
    
//    [System.Serializable]
//    private class MaterialID : Object
//    {
//        public enum MaterialState { Opaque, Transparent}
//        /// <summary>
//        /// The state of the current material. If it's opaque, we'll reduce opacity until it's transparent, and viceversa (if needed)
//        /// </summary>
//        private MaterialState MatState;
//        /// <summary>
//        /// The ID of the transform of this MaterialID
//        /// </summary>
//        public int ID;
//        /// <summary>
//        /// The Material attached to this object
//        /// </summary>
//        public Material MaterialRef;
//        /// <summary>
//        /// Has this material ID been seen during the Check?
//        /// </summary>
//        public bool AlreadyChecked;
//        /// <summary>
//        /// Transparency of the material [0,1]
//        /// </summary>
//        public float OpacityValue;

//        public MaterialID(int id, Material mat)
//        {
//            ID = id;
//            MaterialRef = mat;
//            AlreadyChecked = true;
//            MatState = MaterialState.Opaque;
//            MaterialTypeString = "Opaque";
//            OpacityValue = 1;
//        }
//        public MaterialID() { } //Default Find constructor

//        /// <summary>
//        /// Is this material turning opaque? Changing this value will set the opposite on IsTransparent.
//        /// </summary>
//        public bool IsOpaquing { get => MatState == MaterialState.Opaque; set
//            {
//                MatState = value ? MaterialState.Opaque : MaterialState.Transparent;
//                MaterialTypeString = value ? "Opaque" : "Transparent";
//            } }
//        /// <summary>
//        /// Is material turning transparent? Changing this value will set the opposite on IsOpaque.
//        /// </summary>
//        public bool IsTransparenting { get => !IsOpaquing; set => IsOpaquing = !value; }


//        /// <summary>
//        /// The name of the state of the material, either Opaque or Transparent
//        /// </summary>
//        public string MaterialTypeString { get; private set; } = "Opaque";

//        public override string ToString()
//        {
//            return ID.ToString();
//        }

//        public override bool Equals(object obj)
//        {
//            return (obj as MaterialID).ID == this.ID;
//        }

//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }

//        public static bool operator ==(MaterialID lhs, MaterialID rhs)
//        {
//            //If the first object is null, returns wether the second object is also null or not
//            if (object.ReferenceEquals(lhs, null))
//            {
//                return (object.ReferenceEquals(rhs, null));
//            }

//            //Else compare them (without fear of infinite recursion)
//            return lhs.Equals(rhs);
//        }

//        public static bool operator !=(MaterialID lhs, MaterialID rhs)
//        {

//            if (object.ReferenceEquals(lhs, null))
//            {
//                return !object.ReferenceEquals(rhs, null);
//            }

//            return !lhs.Equals(rhs);
//        }
//    }
//    private List<MaterialID> mMaterialArray = new List<MaterialID>();
//    /*
//     * Every X frames, check if there's an object between the camera and the player.
//     * If there is, check that it sn't already added to the material array. If it is already added, set the check bool to true. Else, check that none of the materials already stored isn't the same as the one in the object.
//     * If it isn't added and none of the objects stored reference the same material, then create a new materialID, store it, and start
//     * After checking every player, if there's an object that hasn't been checked, and has finished transitioning between materials, transition it back.
//     */

//    /// <summary>
//    /// Switch the shader of the given material.
//    /// Note that there should be two different shaders for each material type, one ending in "/Opaque" and another one ending in "/Transparent"
//    /// Also note, it does NOT change the IsOpaque or IsTransparent, and only acts accordingly
//    /// </summary>
//    /// <param name="matID"></param>
//    private void SwitchShader(MaterialID matID)
//    {
//        string currentShaderName = matID.MaterialRef.shader.ToString();
//        string[] substrings = currentShaderName.Split('/');
//        string newShaderName = "";
//        for (int i = 0; i < substrings.Length - 1; i++) newShaderName += substrings[i] + "/";

//        newShaderName += matID.MaterialTypeString; // "/Opaque" or "/Transparent"

//        Material tempMat = new Material(matID.MaterialRef); //Backup the properties
//        matID.MaterialRef.shader = Shader.Find(newShaderName);
//        matID.MaterialRef.CopyPropertiesFromMaterial(tempMat);
//        matID.MaterialRef.SetFloat("_Transparency",1f);

//        if(_Debug) Debug.Log("[ElementHider] Switched shader " + matID.ToString());

//    }


//    private void Update()
//    {
//        //Add or subtract transparency on the material list
//        for(int i=0; i < mMaterialArray.Count; i++)
//        {
//            MaterialID matID = mMaterialArray[i];
//            //If opaque, substract opacity until it's transparent. If it's transparent && needs to turn to opaque, add opacity
//            if (matID.IsTransparenting && matID.OpacityValue != 0)
//            {
//                matID.OpacityValue = Mathf.Clamp01(matID.OpacityValue - mTransitionSpeed*Time.deltaTime);
//                matID.MaterialRef.SetFloat("_Transparency", matID.OpacityValue);
//            }
//            else if (matID.IsOpaquing) //If transparent AND we want it to go opaque
//            {
//                matID.OpacityValue = Mathf.Clamp01(matID.OpacityValue + mTransitionSpeed * Time.deltaTime);
//                matID.MaterialRef.SetFloat("_Transparency", matID.OpacityValue);
//                if (matID.OpacityValue == 1) //If full opaque, switch to an opaque shader and remove it from the list
//                {
//                    if (_Debug) Debug.Log("[ElementHider] The element" + matID.ToString() + "is now full opaque and will be removed from the list");
//                    SwitchShader(matID);
//                    mMaterialArray.Remove(matID);
//                    Destroy(matID); //The reference is still local
//                }
//            }
//        }
        
        


//        //If it's time to check
//        if (++mCounter % mCheckFrequency == 0)
//        {
//            mCounter = 1;
//            for(int i = 0; i < 4; i++) //Check if there's something between the players and the camera
//            {
//                if (StaticResources.PlayerBatch[i].Playable)
//                {
//                    Vector3 direction = (StaticResources.PlayerBatch[i].Position - transform.position);

//                    RaycastHit[] allRayCasts = Physics.RaycastAll(transform.position - transform.forward * mMinDistance, direction.normalized, direction.sqrMagnitude, StaticResources.GameManager.EnvironmentBlockLayerMask);
//                    if (allRayCasts.Length !=0) //Hits something
//                    {
//                        foreach( RaycastHit hitInfo in allRayCasts)
//                        {
//                            if (hitInfo.transform.CompareTag("Transparentable"))
//                            {
//                                MaterialID matID = new MaterialID();
//                                for(int j = 0; j< mMaterialArray.Count; j++)
//                                {
//                                    if(mMaterialArray[i].ID == hitInfo.transform.GetInstanceID())
//                                    {
//                                        matID = mMaterialArray[i];
//                                        break;
//                                    }
//                                }

//                                if (matID.ID == 0) //Adds a new opaque material
//                                {
//                                    MaterialID yetANewMatID = new MaterialID(hitInfo.transform.GetInstanceID(), hitInfo.transform.GetComponent<MeshRenderer>().material);
//                                    Destroy(matID);
//                                    if (_Debug) Debug.Log("[ElementHider] Found new matID " + yetANewMatID.ToString() + "to add to the list.");
//                                    mMaterialArray.Add(yetANewMatID);
//                                    yetANewMatID.IsTransparenting = true;
//                                    SwitchShader(yetANewMatID); //Switches it to transparent

                                    
//                                }

//                                else//If the material ID is already in the list, check it so we won't set it to opaque yet later
//                                {
//                                    mMaterialArray.Find(x => x.ID.Equals(hitInfo.transform.GetInstanceID())).AlreadyChecked = true;
//                                    if (_Debug) Debug.Log("[ElementHider] This mat is already listed!");
//                                }
//                            }
//                        }
                        
//                    }
//                }
//            }

//            foreach (MaterialID matID in mMaterialArray)
//            {
//                if (!matID.AlreadyChecked)
//                {
//                    matID.IsOpaquing = true; //We're turning this opaque
//                    if (_Debug) Debug.Log("[ElementHider] This matID " + matID.ToString() + "hasn't been checked, meaning it'll go back to opaque soon.");
//                }
//                matID.AlreadyChecked = false; //Reset 
//            }


//        }


//    }



//}
