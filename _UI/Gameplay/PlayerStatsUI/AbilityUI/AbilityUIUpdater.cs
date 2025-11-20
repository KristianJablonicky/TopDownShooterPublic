using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIUpdater : Hoverable
{
    [SerializeField] private AbilityType type;
    [SerializeField] private AbilityUI abilityUI;
    
    [Header("Hover")]
    [SerializeField] private float targetYOnHover;
    [SerializeField] private float hoverDuration = 1f;

    [Header("References")]
    [SerializeField] private Image abilityIcon, cooldownOverlay;
    [SerializeField] private GameObject coolDownGrayedOut;
    [SerializeField] private TMP_Text cooldownText, abilityDescription;

    private Ability ability;
    private ActiveAbility activeAbility;

    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnOwnerSpawned;
    }

    private void OnOwnerSpawned(CharacterMediator mediator)
    {
        ability = GetAbility(mediator.AbilityManager);
        if (ability is ActiveAbility ab)
        {
            activeAbility = ab; 
            activeAbility.CurrentCoolDown.OnValueSet += OnCoolDownChanged;
        }
        abilityUI.Init(ability, false);
        abilityDescription.text = abilityUI.Text;

        ability.IconChanged += newIcon => abilityIcon.sprite = newIcon;
    }

    private Ability GetAbility(AbilityManager AbilityManager)
    {
        return type switch
        {
            AbilityType.Movement => AbilityManager.MovementAbility,
            AbilityType.Utility => AbilityManager.UtilityAbility,
            AbilityType.Passive => AbilityManager.PassiveAbility,
            AbilityType.PostMortem => AbilityManager.AbilityPostMortem,
            _ => null
        };
    }

    private void OnCoolDownChanged(float newValue)
    {
        cooldownText.text = newValue > 0f ? newValue.ToString("F1") : "";
        if (newValue == 0f)
        {
            coolDownGrayedOut.SetActive(false);
        }
        else if (!coolDownGrayedOut.activeSelf)
        {
            coolDownGrayedOut.SetActive(true);
        }
        cooldownOverlay.fillAmount = newValue / activeAbility.CoolDown;
    }
    private Coroutine hoverCoroutine;
    public override void HoverStateChanged(bool hoveredOn)
    {
        if (hoveredOn)
        {
            var targetY = targetYOnHover + abilityDescription.renderedHeight;
            hoverCoroutine = StartCoroutine(
                Tweener.TweenCoroutine(this, 0f, targetY, hoverDuration,
                    TweenStyle.quadratic, value => SetYPosition(value)
                )
            );
        }
        else
        {
            StopCoroutine(hoverCoroutine);
            SetYPosition(0f);
        }
    }

    private void SetYPosition(float y)
    {
        transform.localPosition = new Vector2(transform.localPosition.x, y);
    }
}
