using TMPro;
using UnityEngine;

public class ObjectiveTextUpdater : MonoBehaviour
{
    [SerializeField] private DefenderObjective objective;
    [SerializeField] private TMP_Text costTMP;

    private void Start()
    {
        if (DataStorage.IsSinglePlayer)
        {
            Destroy(gameObject);
            return;
        }
        PlayerNetworkInput.PlayerSpawned += OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(CharacterMediator localPlayer)
    {
        localPlayer.BloodManager.OnBloodPickedUp += () => UpdateCostText(localPlayer);
        localPlayer.HealthComponent.DamageTaken += () => UpdateCostText(localPlayer);
        localPlayer.NewRoleAssigned += (_) => UpdateCostText(localPlayer);
    }

    private void UpdateCostText(CharacterMediator mediator)
    {
        var cost = objective.GetCurrentCost(mediator);
        if (cost > 0)
        {
            costTMP.text = objective.GetCurrentCost(mediator).ToString();
        }
        else
        {
            costTMP.text = string.Empty;
        }
    }
}
