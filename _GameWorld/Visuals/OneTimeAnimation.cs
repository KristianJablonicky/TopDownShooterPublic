using System.Collections;
using UnityEngine;

public class OneTimeAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float duration;

    private IEnumerator Start()
    {
        var frameLength = duration / frames.Length;
        foreach (var frame in frames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameLength);
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
