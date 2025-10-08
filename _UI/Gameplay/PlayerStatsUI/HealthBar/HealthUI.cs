using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    private HealthComponent playerHealth;

    private float red;
    private void Awake()
    {
        red = healthText.color.r;
        PlayerNetworkInput.OwnerSpawned += (mediator) =>
        {
            playerHealth = mediator.HealthComponent;
            playerHealth.CurrentHealth.OnValueSet += UpdateHealth;
        };
    }

    private void UpdateHealth(int newHealth)
    {
        healthText.text = newHealth.ToString();
        var ratio = (float)newHealth / playerHealth.MaxHealth;
        healthFill.fillAmount = ratio;

        healthFill.color = new(red, ratio, ratio);
    }
}
