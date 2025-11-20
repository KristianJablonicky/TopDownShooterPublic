using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenuManager : SingletonMonoBehaviour<EscapeMenuManager>
{
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private Button startTrainingButton, exitToMainMenuButton;

    public static bool MenuOpen => Instance.visible;
    public event Action<bool> Toggled;

    private bool visible = false;
    private void Start()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;

        if (DataStorage.Instance.GetGameMode() == GameMode.SinglePlayer)
        {
            startTrainingButton.gameObject.SetActive(true);
            startTrainingButton.onClick.AddListener( () =>
                {
                    if (CharacterManager.Instance.LocalPlayerMediator.Gun.ChannelingManager.Channeling) return;
                    SinglePlayerManager.Instance.StartTraining();
                    ToggleMenu();
                }
            );
        }

        exitToMainMenuButton.onClick.AddListener(() => NetworkManager.Singleton.Shutdown());
    }

    private void OnPlayerSpawn(CharacterMediator player)
    {
        player.InputHandler.EscapePressed += ToggleMenu;
    }

    public void ToggleMenu()
    {
        visible = !visible;
        escapeMenu.SetActive(visible);
        Toggled?.Invoke(visible);
    }
}
