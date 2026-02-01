using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static void TryToStopCoroutine(this MonoBehaviour mb, Coroutine coroutine)
    {
        if (coroutine != null)
        {
            mb.StopCoroutine(coroutine);
        }
    }
}
