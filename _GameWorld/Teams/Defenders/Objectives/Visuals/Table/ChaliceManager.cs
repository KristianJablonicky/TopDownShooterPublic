using UnityEngine;

public class ChaliceManager : MonoBehaviour
{
    [SerializeField] private RitualChalice[] chalices;
    [SerializeField] private GameObject channelingEffectPrefab;
    private int lastSacrificeIndex;
    private void Start()
    {
        DefenderObjective.Instance.SacrificesRemaining.OnValueSet += OnSacrificeMade;
        lastSacrificeIndex = chalices.Length;

        // Reverse chalice order
        (chalices[0], chalices[2]) = (chalices[2], chalices[0]);
    }

    private void OnSacrificeMade(int sacrificesRemaining)
    {
        lastSacrificeIndex = sacrificesRemaining;
        if (sacrificesRemaining >= chalices.Length)
        {
            foreach (var chalice in chalices)
            {
                chalice.GetDeactivated();
            }
        }
        else
        {
            chalices[sacrificesRemaining].GetActivated();
        }
    }

    public void StartChannelingAnimation(CharacterMediator sacrificingMediator, float duration)
    {
        var targetChalice = chalices[lastSacrificeIndex - 1];
        var channelingEffect = Instantiate(channelingEffectPrefab, sacrificingMediator.GetPosition(), Quaternion.identity);
        Tweener.Tween(this, 0f, 1f, duration, TweenStyle.linear,
            value =>
            {
                channelingEffect.transform.position = Vector2.Lerp(
                    sacrificingMediator.GetPosition(),
                    targetChalice.transform.position,
                    value
                );

                Vector2 direction = (targetChalice.transform.position
                    - channelingEffect.transform.position).normalized;
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                channelingEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle);

                channelingEffect.transform.localScale = Vector2.one * Mathf.Sin(value * Mathf.PI);
            },
            () => Destroy(channelingEffect)
        );
    }
}
