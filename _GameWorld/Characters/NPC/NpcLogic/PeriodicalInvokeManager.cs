using System.Collections;
using UnityEngine;

public class PeriodicalInvokeManager : MonoBehaviour
{
    [field: SerializeField] public float CheckInterval { get; protected set; } = 1f;
    [SerializeField] private PeriodicallyInvoked[] periodicalInvoke;
    private Coroutine invokeCoroutine;
    private void OnEnable()
    {
        if (invokeCoroutine != null) return;
        invokeCoroutine = StartCoroutine(PeriodicalInvoke()); 
    }
    private void OnDisable()
    {
        if (invokeCoroutine == null) return;
        StopCoroutine(invokeCoroutine);
    }
    private IEnumerator PeriodicalInvoke()
    {
        var waitInterval = new WaitForSeconds(CheckInterval);
        while (true)
        {
            yield return waitInterval;
            foreach (var invokee in periodicalInvoke)
            {
                invokee.Invoke();
            }
        }
    }
}
public abstract class PeriodicallyInvoked : MonoBehaviour
{
    public abstract void Invoke();
}