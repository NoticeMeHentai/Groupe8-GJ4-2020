using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object containing all the infos necessary to display previews for each player.
/// </summary>
[CreateAssetMenu(fileName = "PreviewSkills", menuName = "TFA/Character/PreviewSkills")]
public class PreviewSkillSO : ScriptableObject
{

    /// <summary>
    /// The distance skill type info.
    /// X-> Stretching distance.
    /// Y-> Forward offset
    /// Z-> Overall scale
    /// </summary>
    [Header("Distance skill info")]
    public Vector3 DistanceInfo = new Vector3(0f, 0f, 1f);
    public Color DistanceColor =  Color.red;
    public Texture2D DistanceTexture;

    /// <summary>
    /// The area skill type info
    /// X-> Minimal Radius
    /// Y-> Maximal radius
    /// Z-> Border distance
    /// W -> Border smoothness
    /// </summary>
    [Header("Area skill info")]
    public Vector4 AreaInfo = new Vector4(1f, 2f, 0f, 0f);
    public Color AreaColor = Color.red * 0.5f;
    public Texture2D AreaTexture;
    
}
