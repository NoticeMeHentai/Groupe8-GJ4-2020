using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FunctionsCurves { Xexp2, Xexp3, Xexp5, linear}
public enum SphericalDirections {  Outwards, Inwards}
public enum OrthogonalDirections { Right,Left,Up,Down,Foward,Backward}
public static class MathHelper
{
    private static Vector3 VectorForward = new Vector3(0,0,1);

    private static int mGroundLayerMask = 0;
    public static int GroundLayerMask { get { if (mGroundLayerMask == 0) mGroundLayerMask = 1 << LayerMask.NameToLayer("Ground"); return mGroundLayerMask; } }
    private static int mEnemiesLayerMask = 0;
    public static int EnemiesLayerMask { get { if (mEnemiesLayerMask == 0) mEnemiesLayerMask = 1 << LayerMask.NameToLayer("Enemy"); return mEnemiesLayerMask; } }

    private static int mBlockersLayerMask = 0;
    public static int BlockersLayerMask { get { if (mBlockersLayerMask == 0) mBlockersLayerMask = 1 << LayerMask.NameToLayer("Blockers"); return mBlockersLayerMask; } }


    private static int mBlockersAndGroundLayerMask = 0;
    public static int BlockersAndGroundLayerMask { get { if (mBlockersAndGroundLayerMask == 0) mBlockersAndGroundLayerMask = BlockersLayerMask|GroundLayerMask; return mBlockersAndGroundLayerMask; } }

    /// <summary>
    /// Transforms a [0,1] value into a smooth one with an smooth in (value goes from 0 to 1).
    /// </summary>
    /// <param name="t">The [0,1] value to smooth. </param>
    /// <param name="function">Kind of curve to smooth the value.</param>
    /// <returns>The smoothed value.</returns>
    public static float EasyIn(float t, FunctionsCurves function)
    {
        switch (function)
        {
            case FunctionsCurves.Xexp2:
                return t * t;
            case FunctionsCurves.Xexp3:
                return t * t * t;
            case FunctionsCurves.Xexp5:
                return t * t * t * t * t;
            case FunctionsCurves.linear:
                return t;
        }
        return 42f;
    }

    /// <summary>
    /// Transforms a [0,1] value into a smooth one with an smooth out (value goes from 1 to 0).
    /// </summary>
    /// <param name="t">The [0,1] value to smooth. </param>
    /// <param name="function">Kind of curve to smooth the value.</param>
    /// <returns>The smoothed value.</returns>
    public static float EasyOut(float t, FunctionsCurves function)
    {
        switch (function)
        {
            case FunctionsCurves.Xexp2:
                return Flip(Mathf.Sqrt(t));
            case FunctionsCurves.Xexp3:
                return Flip(Mathf.Pow(t, 0.333f));
            case FunctionsCurves.Xexp5:
                return Flip(Mathf.Pow(t,0.2f));
            case FunctionsCurves.linear:
                return Flip(t);
        }
        return 42f;
    }

    /// <summary>
    /// Transforms a [0,1] value into a smooth one with a hard in (value goes from 1 to 0).
    /// </summary>
    /// <param name="t">The [0,1] value to smooth. </param>
    /// <param name="function">Kind of curve to smooth the value.</param>
    /// <returns>The smoothed value.</returns>
    public static float HardIn(float t, FunctionsCurves function)
    {
        switch (function)
        {
            case FunctionsCurves.Xexp2:
                return (Mathf.Sqrt(t));
            case FunctionsCurves.Xexp3:
                return (Mathf.Pow(t, 0.333f));
            case FunctionsCurves.Xexp5:
                return (Mathf.Pow(t, 0.2f));
            case FunctionsCurves.linear:
                return (t);
        }
        return 42f;
    }

    /// <summary>
    /// Transforms a [0,1] value into a smooth one with a hard out (value goes from 0 to 1).
    /// </summary>
    /// <param name="t">The [0,1] value to smooth. </param>
    /// <param name="function">Kind of curve to smooth the value.</param>
    /// <returns>The smoothed value.</returns>
    public static float HardOut(float t, FunctionsCurves function)
    {
        switch (function)
        {
            case FunctionsCurves.Xexp2:
                return Flip(t*t);
            case FunctionsCurves.Xexp3:
                return Flip(t * t * t);
            case FunctionsCurves.Xexp5:
                return Flip(t * t * t * t * t);
            case FunctionsCurves.linear:
                return Flip(t);
        }
        return 42f;
    }
    /// <summary>
    /// Lerps a smooth value between 0 and 1 with the given min and max
    /// </summary>
    /// <param name="t">The value to lerp</param>
    /// <param name="min">The step at which the value will return 0</param>
    /// <param name="max">The step at which the value will return 1</param>
    /// <returns>The smoothed value</returns>
    public static float SmoothStep(float t, float min, float max)
    {
        t = (t - min) / (1 - max);
        float smooth = Mathf.Clamp01(3 * t * t - 2 * t * t * t);
        if (max > min)
            return smooth;
        else return 1 - smooth;
    }

    /// <summary>
    /// Lerps a smooth value between 0 and 1 with the given min max using the cosinus function 
    /// </summary>
    /// <param name="t"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns>The smoothed value</returns>
    public static float CosStep(float t, float min, float max)
    {
        float direction = max > min ? -1 : 1;
        float scale = Mathf.Abs(max - min) * 0.5f;
        return direction * Mathf.Cos(t * Mathf.PI) * scale - scale + Mathf.Max(min, max);
    }

    public static float Flip(float t)
    { return 1 - t; }

    /// <summary>
    /// Tracks the time that has passed since a given previous time.
    /// </summary>
    /// <param name="previousTime">The point in time from which we track the time that has passed from.</param>
    /// <param name="targetTime">The time you're targetting(needed for normalized)</param>
    /// <param name="normalized">Returns a normalized value or not.</param>
    /// <returns>The time that has passed from a given time.</returns>
    public static float TrackTimeEditor(float previousTime, bool normalized, float targetTime = -1)
    {
        if(Time.realtimeSinceStartup<previousTime)
        {
            Debug.LogWarning("WARNING: You set up a previous time bigger than the actual time!");
            return 0f;
        }
        else if(previousTime<0 ||( normalized&&targetTime < 0))
        {
            Debug.LogWarning("WARNING: You cannot have negative times!");
            return 0f;
        }
        else if (normalized&&targetTime < previousTime)
        {
            Debug.LogWarning("WARNING: Your posTime is smaller than the previousTime!");
            return 0f;
        }
        else
        {
            float progressed = Time.realtimeSinceStartup - previousTime;
            if (normalized) return Mathf.Clamp01( progressed / (targetTime - previousTime));
            else return progressed;
        }
        
    }
    /// <summary>
    /// Tries to get the furthest ground point in a given direction within the distance allowed. 
    /// </summary>
    /// <param name="origin">The point in space from which we want to check if there's ground in direction </param>
    /// <param name="directionTest"> The direction, paralel to the ground</param>
    /// <param name="maxDistance">The maximum distance to check towards direction</param>
    /// <param name="steps">The number of steps to check</param>
    /// <returns> The furthest point. If none could be found, origin is returned.</returns>
    public static Vector3 TestGroundPosition(Vector3 origin, Vector3 directionTest, float maxDistance, int steps, float maxHeightTestDistance = 0.2f)
    {
        steps = Mathf.Max(Mathf.Abs(steps), 1);
        maxDistance = Mathf.Abs(maxDistance);
        float singleStep = maxDistance / (float)steps;
        RaycastHit hitInfo;
        for(int i = 0; i < steps; i++)
        {
            if (Physics.Raycast(origin + directionTest*(steps-i)*singleStep, Vector3.down, out hitInfo, maxHeightTestDistance, GroundLayerMask) 
                || Physics.Raycast(origin + directionTest * (steps - i) * singleStep, Vector3.up, out hitInfo, maxHeightTestDistance, GroundLayerMask))
            {
                Debug.LogFormat("[MathHelper] Starting point:{0}, target direction{1}, target distance{2}, target found {3}, Red line: original point, blue line: target pos, white line: pos found", origin, directionTest, maxDistance, hitInfo.point);
                Debug.DrawLine(origin, origin + Vector3.up * 3f, Color.red, 5f);
                Debug.DrawLine(origin + directionTest*(maxDistance+0.1f), origin + directionTest * maxDistance + Vector3.up * 3f, Color.blue, 5f);
                Debug.DrawLine(hitInfo.point + directionTest * ( 0.15f), hitInfo.point + directionTest * (0.15f) + Vector3.up * 3f, Color.white, 5f);
                return hitInfo.point;
            }
        }
        Debug.LogFormat("[MathHelper] Bad raycasts! Starting point:{0}, target direction{1}, target distance{2},  Red line: original point, blue line: target pos", origin, directionTest, maxDistance);
        return origin;
    }

    public static Vector3 PlaceGroundHeight(Vector3 origin, float offset = 0f)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(origin, Vector3.down, out hitInfo, 5f, GroundLayerMask) || Physics.Raycast(origin, Vector3.up, out hitInfo, 5f, GroundLayerMask))
        {
            return hitInfo.point + Vector3.up * offset;
        }
        else
        {
             Debug.Log("[MathHelper][PlaceGroundHeight] No ground position found.");
            return origin;
        }
    }


    public static Vector3 SpiralLerp(float value, int ToursAmount, Vector3 startingPoint, Vector3 endingPoint, SphericalDirections direction, Vector3 GlobalUpAxis)
    {
        value = Mathf.Clamp01(value);
        ToursAmount = Mathf.Max(1,Mathf.Abs(ToursAmount));
        GlobalUpAxis.Normalize();
        //General equation goes outwards, but if it's inwards, we inverse starting and ending point, and one-minus the value 
        if(direction == SphericalDirections.Inwards)
        {
            Vector3 temporalStart = endingPoint;
            endingPoint = startingPoint;
            startingPoint = temporalStart;
            value = 1 - value;
        }
        
        Vector3 VectorForward = endingPoint - startingPoint;
        VectorForward.y = 0;
        float distance = VectorForward.magnitude;
        VectorForward.Normalize();
        Vector3 VectorRight = Vector3.Cross(VectorForward, GlobalUpAxis);
        VectorRight.y = 0;
        VectorRight.Normalize();
        Vector3 newPos = startingPoint 
            + (VectorRight*(value*Mathf.Sin(ToursAmount*value*2f*Mathf.PI)) 
            + VectorForward*(value*Mathf.Cos(ToursAmount*value*2f*Mathf.PI))
            )*distance + (endingPoint.y - startingPoint.y)*value*GlobalUpAxis;
        return newPos;
    }

        

        
    

    


    public static float PulsatingValue(int toursPerSecond, int frequency, float timeDelay = 0)
    {
        float time = Time.time + timeDelay;
        time *= toursPerSecond;
        float underRemainder = frequency * 2 * Mathf.PI;
        time %= underRemainder;
        float firstStep = time < 2 * Mathf.PI ? 1 : 0; 
        time = Mathf.Abs(Mathf.Sin(time));
        return firstStep * time;
    }

    /// <summary>
    /// From an array of transforms, computes which ones are within the given range and returns and array with those.
    /// </summary>
    /// <param name="toCheck"> The array of transforms to check.</param>
    /// <param name="position">The starting position.</param>
    /// <param name="direction">The facing direction.</param>
    /// <param name="angle">The amplitude of the angle (total, not half).</param>
    /// <param name="maxDistance">The maximal distance to check.</param>
    /// <returns></returns>
    public static Transform[] WithinRange(Transform[] toCheck, Vector3 position, Vector3 direction, float angle, float maxDistance)
    {
        List<Transform> toReturn = new List<Transform>();
        for(int i = 0; i < toCheck.Length; i++)
        {
            Transform target = toCheck[i];
            if (target == null) continue;
            Vector3 dirToTarget = target.position - position;
            //Is it further than required
            float distance = (dirToTarget).sqrMagnitude;
            if (distance > maxDistance) continue;

            //Is it outside "view"
            dirToTarget = dirToTarget.normalized;
            float dot = Vector3.Dot(dirToTarget, direction);
            if (Mathf.Acos(dot) > angle * 0.5f) continue;

            //Else it's within range
            toReturn.Add(target);
        }
        return toReturn.ToArray();
    }

    /// <summary>
    /// From an array of transforms, computes which ones are within the given range and returns and array with those.
    /// </summary>
    /// <param name="toCheck"> The array of transforms to check.</param>
    /// <param name="position">The starting position.</param>
    /// <param name="direction">The facing direction.</param>
    /// <param name="angle">The amplitude of the angle (total, not half).</param>
    /// <param name="maxDistance">The maximal distance to check.</param>
    /// <returns></returns>
    public static Transform ClosestWithinRange(Transform[] toCheck, Vector3 position, Vector3 direction, float angle, float maxDistance)
    {
        Transform closest = null;
        float closestDistance = 0f;
        for (int i = 0; i < toCheck.Length; i++)
        {
            Transform target = toCheck[i];
            if (target == null) continue;
            Vector3 dirToTarget = target.position - position;

            //Is it outside "view"
            dirToTarget = dirToTarget.normalized;
            float dot = Vector3.Dot(dirToTarget, direction);
            if (Mathf.Acos(dot) > angle * 0.5f) continue;

            //Is it further than required
            float distance = (dirToTarget).sqrMagnitude;
            if (distance > maxDistance) continue;
            //Else it's within range
            if (i == 0)
            {
                closest = target;
                closestDistance = distance;
            }
            else if (distance < closestDistance)
            {
                closest = target;
                closestDistance = distance;
            }
        }
        return closest;
    }

    /// <summary>
    /// Converts a 6 digits hexadecimal code into a color and returns it. \n
    /// If the hexCode's length is different than 6, it returns a black color automatically.
    /// </summary>
    public static Color HexToRGB(string hexCode, float alpha = 1f)
    {
        if (hexCode.Length != 6) return Color.black;

        Vector3 vec = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            string hexFraction = hexCode;

            hexFraction = hexFraction.Substring(i * 2, 2);
            vec[i] = System.Convert.ToInt32(hexFraction, 16) / 255f;
        }


        return new Color(vec[0], vec[1], vec[2], alpha);
    }

    public static Texture2D BackgroundColor(Color backgroundColor)
    {
        Texture2D texToReturn = new Texture2D(2, 2);
        Color[] pix = new Color[2 * 2];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = backgroundColor;
        }
        texToReturn.SetPixels(pix);
        texToReturn.Apply();
        return texToReturn;
    }
}


public static class Vector2Extensions
{
    /// <summary>
    /// Returns the ratio in which the value inputted is situated between the vector values.
    /// That means the inverse lerp of 3 in a (2,5) vector would return 0.33
    /// </summary>
    public static float InverseLerp(this Vector2 v, float value)
    {
        return Mathf.InverseLerp(v.x, v.y, value);
    }

    public static float Lerp(this Vector2 v, float ratio)
    {
        return Mathf.Lerp(v.x, v.y, ratio);
    }
    /// <summary>
    /// Returns a random value between the x and y value of this vector2.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static float Random(this Vector2 v)
    {
        return v.Lerp(UnityEngine.Random.value);
    }

    public static Vector2 LimitMagnitude(this Vector2 v, float maxLength)
    {
        float squareLength = v.x * v.x + v.y * v.y;
        if ((squareLength > maxLength*maxLength ) && (v.sqrMagnitude > 0))
        {
            float ratio = maxLength / Mathf.Sqrt(squareLength);
            v.x *= ratio;
            v.y *= ratio;
        }
        return v;
    }

    /// <summary>
    /// Is the value input between the x and Y value of this vector? [Extremes included]
    /// </summary>
    /// <param name="value"> The value to compare</param>
    /// <returns>Wether it's in between or not</returns>
    public static bool WithinRange(this Vector2 v, float value)
    { 
        if (v.y > v.x)
            return (value >= v.x && value <= v.y);
        else return value <= v.x && value >= v.y;
    }

    public static float Length(this Vector2 v)
    {
        return Mathf.Abs(v.y - v.x);
    }

    //https://stackoverflow.com/questions/45270723/how-to-rotate-vector
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return new Vector2(
            (float)(v.x * Mathf.Cos(degrees) - v.y * Mathf.Sin(degrees)),
            (float)(v.x * Mathf.Sin(degrees) + v.y * Mathf.Cos(degrees))
        );
    }

    public static Vector2 Rotate(this Vector2 v, float degrees, Vector2 pivot)
    {
        v -= pivot;
        return new Vector2(
            (float)(v.x * Mathf.Cos(degrees) - v.y * Mathf.Sin(degrees)),
            (float)(v.x * Mathf.Sin(degrees) + v.y * Mathf.Cos(degrees))
        ) + pivot;
    }

}

public static class Vector3Extensions
{
    public enum Axis { x,y,z}

    /// <summary>
    /// Returns a vector with one of the axis flattened to 0
    /// </summary>
    /// <param name="axis">The axis to flat.</param>
    /// <param name="normalize">Wether the vector should be normalized or not.</param>
    /// <returns> The vector with one of the axis flattened to 0.</returns>
    public static Vector3 FlatOneAxis(this Vector3 v, Axis axis, bool normalize = false)
    {
        Vector3 toReturn = new Vector3(axis == Axis.x ? 0 : v.x, axis == Axis.y ? 0 : v.y, axis == Axis.z ? 0 : v.z);
        return normalize ? toReturn.normalized : toReturn;
    }
}



public static class RandomExtensions
{
    //https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }

        //Usage example
        //var array = new int[] { 1, 2, 3, 4 };
        //var rng = new Random();
        //rng.Shuffle(array);
        //rng.Shuffle(array); // different order from first call to Shuffle
    }
}





