using System.Collections;
using UnityEngine;

public class FadeOutThenGetDestroyed : MonoBehaviour
{
    [SerializeField] private float duration = 1f, fadeOutTime = 0.5f;
    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(duration);
        float timeRemaining = fadeOutTime;
        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            sr.SetAlpha(timeRemaining / fadeOutTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}
