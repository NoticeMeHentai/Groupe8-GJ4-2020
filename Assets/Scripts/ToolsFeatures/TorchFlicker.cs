using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class TorchFlicker : MonoBehaviour
{


    [SerializeField] private bool mPreviewEditor = false;
    [Header("MinMax")]
    [Tooltip("The minimal and maximal values of the light intensity.")]public Vector2 mMinMaxIntensityFlicker = new Vector2(0.9f, 1.1f);
    [Tooltip("The minimal and maximal values of the light movement (sphere distance).")] public Vector2 mMinMaxDistanceTransform = new Vector2(0.2f, 0.5f);
    [Tooltip("The minimal and maximal values of the light range.")] public Vector2 mMinMaxRangeFlicker = new Vector2(9f, 20f);
    [Header("Regular")]
    [Tooltip("The ratio between which the regular flicking will have its intensity (0 being the minimal, 1 being the maximal)")] [MinMaxSlider(0f, 1f)] public Vector2 mRegularFlickIntensity = new Vector2(0.4f, 0.6f);
    [Range(0f, .5f)] [Tooltip("The ratio between which the regular flicking will have its range (0 being the minimal, 1 being the maximal)")] public float mRegularDistanceFlick = 0.2f;
    [Tooltip("The ratio between which the regular flicking will have its movement (0 being the minimal, 1 being the maximal)")] [MinMaxSlider(0f,1f)] public Vector2 mRegularFlickRange = new Vector2(0.4f,0.6f);
    [Header("Timing")]
    [Tooltip("The minimal and maximal time before the next big flickering comes.")] public Vector2 mMinMaxTimeUntilNextBigFlick = new Vector2(2f, 7f);
    [Tooltip("The minimal and maximal overall duration of the flickering.")]public Vector2 mMinMaxFlicksDuration = new Vector2(0.2f, 0.3f);
    [Tooltip("The curve deciding the duration repartition of the flickerings.")]public AnimationCurve mFlickDurationCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
    
    private Light mLight;
    private Light _Light { get { if (mLight == null) mLight = GetComponent<Light>(); return mLight; } }
    private float mFlickTime = 0f;
    private float mFlickDuration = 0f;
    private float mNextBigFlickTime = 0f;
    private Vector3 mDefaultPosition = default;
    private float defaultIntensity = 0;
    private float defaultRange = 0; 


    //Intensity, Range (alphabetical order)
    private Vector2 mStartFlickValues = default;
    private Vector2 mTargetFlickValues = default;
    private Vector3 mStartFlickPosition = default;
    private Vector3 mTargetFlickPosition = default;

    private void OnEnable()
    {
        mPreviewEditor = false;
        mLight = GetComponent<Light>();
        mDefaultPosition = transform.localPosition;
        defaultIntensity = mLight.intensity;
        defaultRange = mLight.range;

        mStartFlickValues = mTargetFlickValues = new Vector2(defaultIntensity, defaultRange);
        mStartFlickPosition = mTargetFlickPosition = transform.localPosition;
        ComputeNewFlick();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!mPreviewEditor) return;
#endif
        mFlickTime += Time.deltaTime;
        float progress = Mathf.SmoothStep(0f,1f, mFlickTime / mFlickDuration);
        transform.localPosition = Vector3.Lerp(mStartFlickPosition, mTargetFlickPosition, progress);
        mLight.intensity = Mathf.SmoothStep(mStartFlickValues.x, mTargetFlickValues.x, progress);
        mLight.range = Mathf.SmoothStep(mStartFlickValues.y, mTargetFlickValues.y, progress);
        if (progress == 1f) ComputeNewFlick(Time.time< mNextBigFlickTime);
        

    }



    public void ComputeNewFlick(bool regular = true)
    {
        mStartFlickValues = mTargetFlickValues;
        mStartFlickPosition = transform.localPosition;
        float random = Random.value;
        Vector2 intensityChoice = regular ? new Vector2(mMinMaxIntensityFlicker.Lerp(mRegularFlickIntensity.x), mMinMaxIntensityFlicker.Lerp(mRegularFlickIntensity.y)) : mMinMaxIntensityFlicker;
        Vector2 rangeChoice = regular ? new Vector2(mMinMaxRangeFlicker.Lerp(mRegularFlickRange.x), mMinMaxRangeFlicker.Lerp(mRegularFlickRange.y)) : mMinMaxRangeFlicker;
        mTargetFlickPosition = mDefaultPosition+ Random.insideUnitSphere * mMinMaxDistanceTransform.Lerp(random) * (regular ? mRegularDistanceFlick:1);
        
        mTargetFlickValues = new Vector3(intensityChoice.Lerp(random), rangeChoice.Lerp(random));
        mFlickTime = 0f;
        mFlickDuration = mMinMaxFlicksDuration.Lerp(mFlickDurationCurve.Evaluate(Random.value)); //This way we can control the amount of small or bigger flicks
        if (!regular) mNextBigFlickTime = mMinMaxTimeUntilNextBigFlick.Lerp(Random.value);
    }


}
