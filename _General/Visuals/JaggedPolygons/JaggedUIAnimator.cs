using System.Collections;
using UnityEngine;

public class JaggedUIAnimator : MonoBehaviour
{
    [SerializeField] private float interval = 1f;
    [SerializeField] private JaggedPolygonUI[] jaggedPolygons;
    [SerializeField] private bool staggerStartPerJaggedPolygon = false;
    [SerializeField] private bool staggerStart = false;
    private static float startOffset;

    private int currentSeed;
    private Coroutine routine;

    private void OnEnable()
    {
        routine = StartCoroutine(Run());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
    }

    private IEnumerator Run()
    {
        if (staggerStart)
        {
            yield return new WaitForSeconds(startOffset);
            startOffset += interval * 0.25f;
        }

        currentSeed = Random.Range(0, 101);
        var wait = new WaitForSeconds(interval);
        var waitBetweenPolygons = new WaitForSeconds(interval / jaggedPolygons.Length);
        while (true)
        {
            if (!staggerStartPerJaggedPolygon) yield return wait;
            foreach (var polygon in jaggedPolygons)
            {
                polygon.RenderAgain(currentSeed);
                if (staggerStartPerJaggedPolygon) yield return waitBetweenPolygons;
            }
            currentSeed++;
        }
    }
}
