using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VFXParameter
{
    public enum Type { Float, Vector, Texture, Color, Particles}
    [Header("Common")]
    public Type ParameterType = Type.Float;
    public string ParameterName = "_Parameter";
    public float TimeStart = 0.5f;
    [Header("Float")]
    [HideIf(nameof(ParameterType), 0)] public float StartingValue = 0f;
    [HideIf(nameof(ParameterType), 0)] public float EndingValue = 1f;
    [Header("Texture")]
    [HideIf(nameof(ParameterType), 2)] public Texture2D TextureToReplace;
    [Header("Color")]
    [HideIf(nameof(ParameterType), 3)] public Color EndColor;
    [Header("Particles")]
    [HideIf(nameof(ParameterType), 4)] public ParticleSystem Particles;


    [HideIf(nameof(ParameterType), 0|3)]public float Duration = 0.5f;
    public bool UseCustomCurve = false;
    [HideIf(nameof(UseCustomCurve), true)]public AnimationCurve ParameterCurve;

    public bool CanBeActivated(float currentTime)
    {
        return currentTime >= TimeStart;
    }
    public bool IsUsed { get; set; }
    private float CurrentProgress(float currentTime)
    {
        float progress = Mathf.Clamp01((currentTime - TimeStart) / Duration);
        return UseCustomCurve ? ParameterCurve.Evaluate(progress) : progress;
    }

    public float GetCurrentFloat(float currentTime)
    {
        return Mathf.Lerp(StartingValue, EndingValue, CurrentProgress(currentTime));
    }

    [HideInInspector] public Color startingColor;
    public Color GetCurrentColor(float currentTime)
    {
        return Color.Lerp(startingColor, EndColor, CurrentProgress(currentTime));
    }


}

public class VFXCaller : MonoBehaviour
{
    public VFXParameter[] m_VFXParams;
    [Tooltip("The renderer we want to apply those effects on. (If it's just calling a ParticleSystem, no need for that)")]
    public Renderer m_Renderer;

    private Material instancedMaterial;
    public void Activate()
    {
        if (m_Renderer != null)
        {
            instancedMaterial = new Material(m_Renderer.material);
            m_Renderer.material = instancedMaterial;
        }
        StartCoroutine(ExecuteVFX());
    }


    private IEnumerator ExecuteVFX()
    {
        float maxTime = 0;
        for(int i = 0; i < m_VFXParams.Length; i++)
        {
            maxTime = Mathf.Max(maxTime, m_VFXParams[i].Duration);
            if (m_VFXParams[i].startingColor == Color.black) m_VFXParams[i].startingColor = instancedMaterial.GetColor(m_VFXParams[i].ParameterName);
        }
        float currentTime = 0;
        VFXParameter vfx;
        while (currentTime < maxTime)
        {

            for (int i = 0; i < m_VFXParams.Length; i++)
            {
                vfx = m_VFXParams[i];
                switch (vfx.ParameterType)
                {
                    case VFXParameter.Type.Float:
                        instancedMaterial.SetFloat(vfx.ParameterName, vfx.GetCurrentFloat(currentTime));
                        break;
                    case VFXParameter.Type.Vector:
                        break;
                    case VFXParameter.Type.Texture:
                        if (vfx.CanBeActivated(currentTime) && !vfx.IsUsed)
                        {
                            vfx.IsUsed = true;
                            instancedMaterial.SetTexture(vfx.ParameterName, vfx.TextureToReplace);
                        }
                        break;
                    case VFXParameter.Type.Color:
                        instancedMaterial.SetColor(vfx.ParameterName, vfx.GetCurrentColor(currentTime));
                        break;
                    case VFXParameter.Type.Particles:
                        if (vfx.CanBeActivated(currentTime) && !vfx.IsUsed)
                        {
                            vfx.IsUsed = true;
                            vfx.Particles.Play();
                        }
                        break;
                    default:
                        break;
                }
            }
            currentTime += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < m_VFXParams.Length; i++)
        {
            m_VFXParams[i].IsUsed = false;
        }
    }
}
