using UnityEngine;

public class FadeInOnEnabled : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private CanvasGroup[] canvasGroups;

    private Coroutine routine;

    private void OnEnable()
    {
        routine = StartCoroutine(
            Tweener.TweenCoroutine(this, 0f, 1f, duration,
                TweenStyle.quadratic,
                value =>
                {
                    foreach (var group in canvasGroups)
                    {
                        group.alpha = value;
                    }
                }
            )
        );
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
    }

}
