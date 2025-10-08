using System;
using System.Collections;
using UnityEngine;

public class CoroutineUtilities : SingletonMonoBehaviour<CoroutineUtilities>
{
    public static IEnumerator ExecuteAfterDelayEnumerator(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    public static void ExecuteAfterDelay(float delay, Action action)
    {
        Instance.StartCoroutine(ExecuteAfterDelayEnumerator(delay, action));
    }
}
