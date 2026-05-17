using System;
using System.Collections;
using UnityEngine;

public class Invoker : SingletonMonoBehaviour<Invoker>
{
    public void ExecuteAfterDelay(float delay, Action action)
    {
        StartCoroutine(AfterDelay(delay, action));
    }

    public void ExecuteAfterDelay(float delay, IEnumerator coroutine)
    {
        StartCoroutine(AfterDelay(delay, () => StartCoroutine(coroutine)));
    }

    public void ExecuteAfterOneFrame(Action action)
    {
        StartCoroutine(AfterDelay(action));
    }

    private IEnumerator AfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    /*
    private IEnumerator AfterDelay(float delay, IEnumerator enumerator)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(enumerator);
    }
    */
    private IEnumerator AfterDelay(Action action)
    {
        yield return null;
        action();
    }
}
