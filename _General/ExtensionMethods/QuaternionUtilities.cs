using UnityEngine;

public static class QuaternionUtilities
{
    public static Quaternion Random(float minDegrees = 0f, float maxDegrees = 360f)
    => Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(minDegrees, maxDegrees));
}
