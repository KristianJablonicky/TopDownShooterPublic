using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OneTimeAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float duration;
    
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool destroyOnEnd = true;

    private void OnEnable()
    {
        if (playOnStart) PlayAnimation();
    }

    public void PlayAnimation(float? durationOverwrite = null, bool reverseOrder = false)
    {
        if (!gameObject.activeInHierarchy) return;
        var animationDuration = durationOverwrite ?? duration;
        StopAllCoroutines();
        StartCoroutine(PlayAnimation(animationDuration, reverseOrder));
    }

    private IEnumerator PlayAnimation(float animationDuration, bool reverseOrder)
    {
        var frameLength = animationDuration / frames.Length;
        var usedFrames = reverseOrder ? frames.Reverse() : frames;

        foreach (var frame in usedFrames)
        {
            SetSprite(frame);
            yield return new WaitForSeconds(frameLength);
        }
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
        if (image != null)
        {
            image.sprite = sprite;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }
}
