using UnityEngine;
using UnityEngine.UI;

public class DamageSummaryEntryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private DamageSummaryEntry damageTakenEntry, damageDealtEntry;
    [Header("Damage color settings")]
    [SerializeField] private float colorMultiplier = 1.5f;
    [SerializeField] private Graphic[] damageDealtRecolors, damageTakenRecolors;
    [Header("Tween Settings")]
    [SerializeField] private float fadeIn = 0.25f;
    [SerializeField] private float fadeOut = 1f;
    private bool setUp = false;
    public void ShowSummary(DamageRecord record)
    {
        if (!setUp) SetUp();
        gameObject.SetActive(true);
        damageTakenEntry.ShowDamage(record.DamageTaken, record.DamageTakenHits);
        damageDealtEntry.ShowDamage(record.DamageDealt, record.DamageDealtHits);
        Tween(1f, fadeIn, 0f);
    }
    private void SetUp()
    {
        setUp = true;
        var team = CharacterManager.Instance.LocalPlayer.Team;
        Recolor(damageTakenRecolors, CommonColors.GetTeamColor(team.Name));
        Recolor(damageDealtRecolors, CommonColors.GetTeamColor(team.EnemyTeamData.Name));
    }

    private void Recolor(Graphic[] graphics, Color color)
    {
        color *= colorMultiplier;
        foreach (var graphic in graphics)
        {
            graphic.color = color;
        }
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
