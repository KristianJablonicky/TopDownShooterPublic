using TMPro;
using UnityEngine;

public class ObjectiveTextUpdater : MonoBehaviour
{
    [SerializeField] private DefenderObjective objective;
    [SerializeField] private TMP_Text costTMP;
    [SerializeField] private ObservableVariableBinder progressBinder;

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
        localPlayer.HealthComponent.DamageTaken += (_, p) => UpdateCostText(p);
        localPlayer.NewRoleAssigned += (_) => UpdateCostText(localPlayer);

        progressBinder.Bind(objective.SacrificesRemaining, true);
    }

    private void UpdateCostText(CharacterMediator mediator)
    {
        costTMP.text = $"{objective.GetCurrentCost(mediator)} health cost";
    }
}
