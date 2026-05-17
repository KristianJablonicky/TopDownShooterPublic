using UnityEngine;

public class GunCardSetUpInGame : MonoBehaviour
{
    [SerializeField] private GunCardSetUp gunCard;
    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;
    }

    private void OnPlayerSpawn(CharacterMediator mediator)
    {
        gunCard.Init(mediator.Toolkit, mediator.Toolkit.GunConfig);
    }
}
