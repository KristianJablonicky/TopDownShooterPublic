using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    private ShootManager manager;
    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnOwnerSpawn;
    }

    private void OnOwnerSpawn(CharacterMediator mediator)
    {
        manager = mediator.Gun.ShootManager;
        manager.CurrentAmmo.OnValueSet += UpdateAmmoText;
        UpdateAmmoText(manager.CurrentAmmo);
    }

    private void UpdateAmmoText(int newAmmo)
    {
        ammoText.text = $"{newAmmo} / {manager.GetCapacity()}";
    }
}
