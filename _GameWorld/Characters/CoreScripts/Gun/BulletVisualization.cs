using UnityEngine;

public class BulletVisualization : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float fadeIn = 0.05f,
        duration = 0.1f,
        fadeOut = 0.1f,
        maxAlpha = 0.5f;
    [SerializeField, Range(0f, 1f)] private float fullAlphaPercent = 0.25f;
    private Gradient defaultGradient;
    private Vector3 bulletStart, bulletEnd;

    private Gradient gradient;
    private GradientAlphaKey[] alphaKeysDefault;
    private Vector2 minDist;
    private void Awake()
    {
        defaultGradient = lineRenderer.colorGradient;
        gradient = defaultGradient;
        alphaKeysDefault = gradient.alphaKeys;
    }
    public void Shoot(Vector2 startPosition, Vector2 targetPosition)
    {
        bulletStart = startPosition;
        
        bulletEnd = targetPosition;
        var direction = bulletEnd - bulletStart;
        
        minDist = bulletStart + direction * fullAlphaPercent;
        lineRenderer.SetPosition(0, minDist);
        
        minDist = bulletStart + direction * (fullAlphaPercent * 2f);
        lineRenderer.SetPosition(1, minDist);
        
        FadeIn();
    }


    private void FadeIn()
    {
        Tweener.Tween(this, 0f, 1f, fadeIn, TweenStyle.quadratic,
            value => { SetAlpha(value * maxAlpha); SetLength(value); }, FadeOut);
    }

    private void FadeOut()
    {
        Tweener.Tween(this, maxAlpha, 0f, fadeOut, TweenStyle.quadratic,
            value => SetAlpha(value), () => Destroy(gameObject), duration);
    }


    private void SetAlpha(float newAlpha)
    {
        var alphaKeys = gradient.alphaKeys;
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = alphaKeysDefault[i].alpha * newAlpha;
        }

        gradient.alphaKeys = alphaKeys;
        lineRenderer.colorGradient = gradient;
    }

    private void SetLength(float newLength)
    {
        var direction = bulletEnd - bulletStart;
        var currentBulletEnd = bulletStart + direction * Mathf.Clamp01(newLength);
        lineRenderer.SetPosition(2, currentBulletEnd);
    }
}
