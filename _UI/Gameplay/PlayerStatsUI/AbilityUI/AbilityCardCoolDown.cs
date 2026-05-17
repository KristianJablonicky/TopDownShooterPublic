using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class AbilityCardCoolDown : MonoBehaviour
{
    [SerializeField] private AbilityType type;

    [Header("References")]
    [SerializeField] private AbilityCardSetUp abilityCard;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image bordersFill;
    [SerializeField] private GameObject coolDownGrayedOut;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private RectTransform rectTransform;

    [Header("Visual settings")]
    [SerializeField] private float minY = -32f;
    [SerializeField] private float onCastAnimationDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxPopSizeMultiplier = 0.25f;
    [SerializeField] private AnimationCurve cooldownToPosition;

    private Ability ability;
    private ActiveAbility activeAbility;
    private Vector2 lowestPoint;

    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnOwnerSpawned;
        lowestPoint = new(0f, minY);
    }

    private void OnOwnerSpawned(CharacterMediator mediator)
    {
        ability = mediator.AbilityManager.GetAbility(type);
        if (ability is ActiveAbility ab)
        {
            activeAbility = ab; 
            activeAbility.CurrentCoolDown.OnValueSet += OnCoolDownChanged;
            activeAbility.Cast += OnAbilityCast;
            activeAbility.PutOnCoolDown += OnStateChange;
        }
        abilityCard.Init(mediator.Toolkit, ability);
        
        ability.IconChanged += newIcon => abilityIcon.sprite = newIcon;
    }

    private void OnAbilityCast()
    {
        OnStateChange();

        Tweener.Tween(this, 0f, 1f, onCastAnimationDuration, TweenStyle.quadratic,
            value => rectTransform.anchoredPosition = new(0f, value * minY)
        );
        var baseScale = rectTransform.localScale;
        Tweener.Tween(this, 0f, 1f, onCastAnimationDuration, TweenStyle.sinusPingPong,
            value => rectTransform.localScale = baseScale * (1f + value * maxPopSizeMultiplier)
        );

    }

    private void OnStateChange()
    {
        coolDownGrayedOut.SetActive(true);
        cooldownText.enabled = true;
    }

    private void OnCoolDownChanged(float newValue)
    {
        if (newValue == 0f)
        {
            CleanUp();
            return;
        }

        cooldownText.text = newValue.ToString("F1");

        var progress = 1f - newValue / activeAbility.CoolDown;
        progress = cooldownToPosition.Evaluate(progress);
        rectTransform.anchoredPosition =
            Vector2.Lerp(lowestPoint, Vector2.zero, progress);
    }

    private void CleanUp()
    {
        coolDownGrayedOut.SetActive(false);
        rectTransform.anchoredPosition = Vector2.zero;
        bordersFill.fillAmount = 1f;
        cooldownText.enabled = false;
    }
}
