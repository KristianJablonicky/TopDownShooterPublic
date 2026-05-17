using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardUI : PlayerCardConcrete
{
    [Header("UI References")]
    [SerializeField] private Image filledBorder, filledBorderWhite;
    [SerializeField] private JaggedPolygonUI whiteJaggedPolygon;
    [SerializeField] private TMP_Text currentHealth, playerName;
    [SerializeField] private CanvasGroup playerNameCanvasGroup;
    [SerializeField] private bool visibleToLocalPlayer = true;
    [SerializeField] private Emote emote;
    [field: SerializeField] public DamageSummaryEntryManager DamageSummaryEntryManager { get; private set; }


    [Header("Visual settings")]
    [SerializeField, Range(0f, 1f)] private float healthFloorFill = 0f;
    [SerializeField] float animationDuration = 0.5f, initialDelay = 0.25f;
    [SerializeField] private Transform rotatingElement;
    [SerializeField] private bool reverseRotation = false;
    [SerializeField] private float colorGBMinValues = 0.25f;

    private float sign;
    private Transform startingTransform;

    public Action<ulong, PlayerCardUI> CardSetUp;

    private CharacterMediator owner;

    public void Init(CharacterMediator mediator)
    {
        owner = mediator;

        cardBase.Init(mediator.Toolkit.CharacterVisuals);

        sign = rotatingElement.transform.eulerAngles.z < 180f ? 1f : -1f;
        if (reverseRotation) sign *= -1f;

        startingTransform = rotatingElement.transform;

        mediator.Died += OnDeathAndRespawn;
        mediator.RespawnedAfterDying += OnDeathAndRespawn;

        if (DataStorage.IsSinglePlayer)
        {
            Destroy(playerName.transform.parent.gameObject);
        }
        else
        {
            playerName.text = mediator.PlayerName;
            if (mediator.IsBot)
            {
                playerName.color = CommonColors.Instance.BotNameColor;
            }
            EscapeMenuManager.Instance.WindowBase.VisibilityChanged += newState =>
                Tweener.TweenCanvasGroupAtRate(playerNameCanvasGroup, 0.25f, TweenStyle.quadratic, newState);
            ScoreBoard.Instance.Shown += newState =>
                Tweener.TweenCanvasGroupAtRate(playerNameCanvasGroup, 0.25f, TweenStyle.quadratic, newState);
            playerNameCanvasGroup.alpha = 0f;

        }

        if (visibleToLocalPlayer)
        {
            mediator.HealthComponent.CurrentHealth.OnValueSet += _ => OnHealthChanged(mediator);
            OnHealthChanged(mediator);
            SetDefaultFillAmount(1f);
        }
        else
        {
            SetDefaultFillAmount(0f);
            currentHealth.text = string.Empty;
            GameStateManager.Instance.RoundEnded += OnRoundStart;
            RoundStartWait.Instance.OnRoundStartWait += ShowEnemyHealthOnRoundEnd;
        }

        emote.Init(mediator);

        CardSetUp?.Invoke(mediator.PlayerId, this);
    }


    private void SetDefaultFillAmount(float fillAmount)
    {
        filledBorder.fillAmount = fillAmount;
        filledBorderWhite.fillAmount = fillAmount;
    }

    private Coroutine spinCoroutine;
    private float targetRotation;
    private void OnDeathAndRespawn(CharacterMediator mediator)
    {
        float startZ;
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            startZ = targetRotation;
        }
        else
        {
            startZ = rotatingElement.transform.eulerAngles.z;
        }
        targetRotation = startZ + 180f * sign;

        spinCoroutine = StartCoroutine(
            Tweener.TweenCoroutine(this, startZ, targetRotation, 0.5f, TweenStyle.quadratic,
                value => rotatingElement.transform.eulerAngles = new (0f, 0f, value)
            )
        );
    }

    public void RotateForFun()
    {
        StopAllCoroutines();
        rotatingElement = startingTransform;
        rotatingElement.Rotate(0f, 0f, 180f);
        OnDeathAndRespawn(null);
    }

    private Coroutine flashCoroutine, hpDropCoroutine, whiteHpDrop;
    private void OnHealthChanged(CharacterMediator mediator)
    {
        var health = mediator.HealthComponent;
        var currentHealthValue = health.CurrentHealth;
        
        if (currentHealthValue > 0)
        {
            var target = GetTargetHealth(health);

            StopHpCoroutines();
            hpDropCoroutine = Tween(filledBorder, target, 0f, 0f);
            whiteHpDrop = Tween(filledBorderWhite, target, animationDuration, initialDelay);
            flashCoroutine = FlashRed(whiteJaggedPolygon, animationDuration);
            currentHealth.text = currentHealthValue.ToString();
        }
        else
        {
            StopHpCoroutines();
            ClearHealth();
        }
    }
    private float GetTargetHealth(HealthComponent health)
    {
        var ratio = (float)health.CurrentHealth / health.MaxHealth;
        return healthFloorFill + (1f - healthFloorFill) * ratio;
    }
    private void ClearHealth()
    {
        SetDefaultFillAmount(0f);
        currentHealth.text = string.Empty;
    }

    private void StopHpCoroutines()
    {
        this.TryToStopCoroutine(whiteHpDrop);
        this.TryToStopCoroutine(hpDropCoroutine);
        this.TryToStopCoroutine(flashCoroutine);
    }

    private Coroutine Tween(Image image, float targetRatio, float animationDuration, float initialDelay)
    {
        return StartCoroutine(
            Tweener.TweenCoroutine(this, image.fillAmount, targetRatio, animationDuration,
                TweenStyle.quadratic,
                value => image.fillAmount = value,
                initialDelay: initialDelay
            )
        );
    }

    private Coroutine FlashRed(JaggedPolygonUI filledBorderWhite, float animationDuration)
    {
        return StartCoroutine(
            Tweener.TweenCoroutine(this, 1f, colorGBMinValues, animationDuration,
                TweenStyle.sinusPingPong,
                value =>
                {
                    filledBorderWhite.SetColor(new(1f, value, value));
                }
            )
        );
    }
    private void OnRoundStart()
    {
        if (!owner.IsAlive) return;
        var hc = owner.HealthComponent;
        SetDefaultFillAmount(GetTargetHealth(hc));
        currentHealth.text = hc.CurrentHealth.ToString();
    }
    private void ShowEnemyHealthOnRoundEnd(float duration)
    {
        CoroutineUtilities.ExecuteAfterDelay(duration, ClearHealth);
    }
}
