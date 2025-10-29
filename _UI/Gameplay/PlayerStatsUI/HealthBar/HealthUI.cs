using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private bool teamMateHealth = false;
    [SerializeField] private CanvasGroup canvasGroup;
    private HealthComponent playerHealth;

    private float red;
    private void Awake()
    {
        red = healthText.color.r;
        if (teamMateHealth) return;

        PlayerNetworkInput.OwnerSpawned += Subscribe;
    }

    private void Start()
    {
        if (!teamMateHealth) return;

        GameStateManager.Instance.GameStarted += () =>
        {
            Subscribe(CharacterManager.Instance.LocalPlayer.GetTeamMate().Mediator);
            canvasGroup.alpha = 1f;
        };
    }

    private void Subscribe(CharacterMediator mediator)
    {
        playerHealth = mediator.HealthComponent;
        playerHealth.CurrentHealth.OnValueSet += UpdateHealth;
    }

    private void UpdateHealth(int newHealth)
    {
        healthText.text = newHealth.ToString();
        var ratio = (float)newHealth / playerHealth.MaxHealth;
        healthFill.fillAmount = ratio;

        healthFill.color = new(red, ratio, ratio);
    }
}
