using UnityEngine;

public class DamageSummaryEntryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private DamageSummaryEntry damageTakenEntry, damageDealtEntry;

    [Header("Tween Settings")]
    [SerializeField] private float fadeIn = 0.25f;
    [SerializeField] private float fadeOut = 1f;

    public void ShowSummary(DamageRecord record)
    {
        gameObject.SetActive(true);
        damageTakenEntry.ShowDamage(record.DamageTaken, record.DamageTakenHits);
        damageDealtEntry.ShowDamage(record.DamageDealt, record.DamageDealtHits);
        Tween(1f, fadeIn, 0f);
    }

    public void HideSummary(float initialDelay) => Tween(0f, fadeOut, initialDelay);

    private void Tween(float targetAlpha, float duration, float initialDelay)
    {
        Tweener.Tween(this, canvasGroup.alpha, targetAlpha, duration, TweenStyle.quadratic,
            value => canvasGroup.alpha = value,
            () => { if (targetAlpha == 0f) gameObject.SetActive(false); },
            initialDelay: initialDelay
        );
    }
}
