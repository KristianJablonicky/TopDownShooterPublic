using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image HeroGraphic;
    [SerializeField] private Image filledBorder, filledBorderWhite;
    [SerializeField] private TMP_Text currentHealth, playerName;
    [SerializeField] private bool visibleToLocalPlayer = true;
    [field: SerializeField] public DamageSummaryEntryManager DamageSummaryEntryManager { get; private set; }

    [Header("Visual settings")]
    [SerializeField, Range(0f, 1f)] private float healthFloorFill = 0f;
    [SerializeField] float animationDuration = 0.5f, initialDelay = 0.25f;
    [SerializeField] private Transform rotatingElement;
    [SerializeField] private bool reverseRotation = false;
    [SerializeField] private float colorMultiplier = 1.5f;
    [SerializeField] private float colorGBMinValues = 0.25f;

    private float sign;
    private Transform startingTransform;

    public Action<ulong, PlayerCardUI> CardSetUp;

    public void Init(CharacterMediator mediator)
    {
        filledBorder.color = mediator.Toolkit.PrimaryColor * colorMultiplier;
        HeroGraphic.sprite = mediator.Toolkit.SplashArt;


        sign = rotatingElement.transform.eulerAngles.z < 180f ? 1f : -1f;
        if (reverseRotation) sign *= -1f;

        startingTransform = rotatingElement.transform;

        mediator.Died += OnDeathAndRespawn;
        mediator.RespawnedAfterDying += OnDeathAndRespawn;

        if (DataStorage.Instance.GetGameMode() != GameMode.SinglePlayer)
        {
            playerName.text = mediator.PlayerName;
            //playerName.gameObject.transform.SetParent(transform);
        }
        else
        {
            Destroy(playerName.gameObject);
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
        }

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
            var ratio = (float)currentHealthValue / health.MaxHealth;
            ratio = healthFloorFill + (1f - healthFloorFill) * ratio;

            StopHpCoroutines();
            hpDropCoroutine = Tween(filledBorder, ratio, 0f, 0f);
            whiteHpDrop = Tween(filledBorderWhite, ratio, animationDuration, initialDelay);
            flashCoroutine = FlashRed(filledBorderWhite, animationDuration);
            currentHealth.text = currentHealthValue.ToString();
        }
        else
        {
            StopHpCoroutines();
            filledBorderWhite.fillAmount = 0f;
            filledBorder.fillAmount = 0f;
            currentHealth.text = string.Empty;
        }
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

    private Coroutine FlashRed(Image filledBorderWhite, float animationDuration)
    {
        return StartCoroutine(
            Tweener.TweenCoroutine(this, 1f, colorGBMinValues, animationDuration,
                TweenStyle.sinusPingPong,
                value =>
                {
                    filledBorderWhite.color = new(1f, value, value);
                }
            )
        );
    }
}
