using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class JaggedTransition : JaggedShapeUI
{
    [Header("Duration settings")]
    [SerializeField] private int updatesCount = 10;
    [Header("Point settings")]
    [SerializeField] private int extraPoints = 3;
    [SerializeField] private float randomOffset = 100f;
    public void FadeOut(float duration, Action actionOnExit)
        => StartCoroutine(Animate(false, duration, actionOnExit));
    public void FadeIn(float duration, Action actionOnExit)
        => StartCoroutine(Animate(true, duration, actionOnExit));

    private IEnumerator Animate(bool fadeIn, float duration, Action actionOnExit)
    {
        //ResetPoints();
        var expanded = BuildExpandedPoints();
        
        Vector2[] startPoints;
        Vector2[] endPoints;
        transform.rotation = fadeIn ? Quaternion.identity : Quaternion.Euler(0, 0, 180);

        var count = expanded.Length;

        if (fadeIn)
        {
            endPoints = expanded;
            startPoints = new Vector2[count];
            for (int i = 0; i < count; i++)
                startPoints[i] = expanded[0];
        }
        else
        {
            startPoints = expanded;
            endPoints = new Vector2[count];
            for (int i = 0; i < count; i++)
                endPoints[i] = expanded[0];
            yield return null;
        }

        points = (Vector2[])startPoints.Clone();

        var wait = new WaitForSeconds(duration / updatesCount);
        var center = (count - 1) * 0.5f;

        for (int i = 0; i < updatesCount; i++)
        {
            var t = (float)(i + 1) / updatesCount;

            for (int p = 1; p < count; p++)
            {
                points[p] = Vector2.Lerp(startPoints[p], endPoints[p], t);

                // randomize position of extra points, but not the corners
                if (p != topLeftIndex && p != bottomRightIndex)
                {
                    points[p] += UnityEngine.Random.insideUnitCircle * randomOffset;
                }
            }

            SetVerticesDirty();
            yield return wait;
        }

        points = endPoints;
        SetVerticesDirty();

        actionOnExit?.Invoke();
    }
    private int topLeftIndex, bottomRightIndex;
    private Vector2[] BuildExpandedPoints()
    {
        Rect rect = GetPixelAdjustedRect();

        Vector2 bottomLeft = new(rect.xMin, rect.yMin);
        Vector2 topLeft = new(rect.xMin, rect.yMax);
        Vector2 topRight = new(rect.xMax, rect.yMax);
        Vector2 bottomRight = new(rect.xMax, rect.yMin);

        var result = new List<Vector2>
        {
            bottomLeft,
            topLeft
        };
        topLeftIndex = 1;


        for (int i = 1; i <= extraPoints; i++)
        {
            //var t = 50f + (50f / (extraPoints + 1)) * i / 100f;
            var t = i / (extraPoints + 1f);
            result.Add(Vector2.Lerp(topLeft, topRight, t));
            //result.Add(Vector2.Lerp(bottomRight, topRight, t));
        }
        result.Add(topRight);
        
        for (int i = 1; i <= extraPoints; i++)
        {
            var t = 1f - i / (extraPoints + 1f);
            //result.Add(Vector2.Lerp(topLeft, topRight, t));
            result.Add(Vector2.Lerp(bottomRight, topRight, t));
        }
        bottomRightIndex = result.Count;
        result.Add(bottomRight);

        return result.ToArray();
    }
}
