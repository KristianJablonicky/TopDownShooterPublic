using UnityEngine;
using UnityEngine.UI;

public class EscapeMenuManager : SingletonMonoBehaviour<EscapeMenuManager>
{
    [SerializeField] private PopUpWindowBase popUpWindowBase;
    [SerializeField] private Button settingsButton, startTrainingButton, exitToMainMenuButton;
    [SerializeField] private PopUpWindowBase settingsPopUp;

    public PopUpWindowBase WindowBase => popUpWindowBase;
    private void Start()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;

        settingsButton.onClick.AddListener(() => {
            settingsPopUp.GetActivated();
        });

        if (DataStorage.Instance.GetGameMode() == GameMode.Training)
        {
            startTrainingButton.gameObject.SetActive(true);
            startTrainingButton.onClick.AddListener( () =>
                {
                    if (CharacterManager.Instance.LocalPlayerMediator.Gun.ChannelingManager.Channeling) return;
                    SinglePlayerManager.Instance.StartTraining();
                    WindowStackManager.Instance.CloseWindow();
                }
            );
        }

        exitToMainMenuButton.onClick.AddListener(SceneManager.Disconnect);
    }

    private void OnPlayerSpawn(CharacterMediator player)
    {
        player.InputHandler.EscapePressed += ShowMenu;
    }
    private void ShowMenu()
    {
        if (WindowStackManager.Instance.WindowsOpen) return;
        popUpWindowBase.GetActivated();
    }
}
