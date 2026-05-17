using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using static Scene;

public class SceneManager : SingletonMonoBehaviour<SceneManager>
{
    [SerializeField] private JaggedTransition jaggedTransition;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float loadingDuration = 0.5f, jaggedDuration = 1f;
    [SerializeField] private GameObject visuals;
    [Header("Audio")]
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip transitionStart, transitionEnd;
    private void Start()
    {
        FadeOut(true);
    }
    private void FadeOut(bool showLoading)
    {
        visuals.SetActive(true);
        if (showLoading)
        {
            Tweener.Tween(this, 1f, 0f, loadingDuration, TweenStyle.quadraticEaseOut,
                value => loadingCanvasGroup.alpha = value);
        }
        else
        {
            loadingCanvasGroup.alpha = 0f;
        }
        var mainMenu = IsMainMenu();
        PlaySound(transitionEnd, mainMenu);

        // Main menu, single player, or player is spawned already (somehow [probably not possible])
        if (mainMenu
        || DataStorage.IsSinglePlayer
        || (CharacterManager.Instance != null && CharacterManager.Instance.LocalPlayerMediator != null))
        {
            jaggedTransition.FadeOut(jaggedDuration, () => visuals.SetActive(false));
        }
        else // it's multiplayer online and the player has not spawned yet
        {
            PlayerNetworkInput.PlayerSpawned += OnLocalPlayerConnected;
        }
    }

    private void OnLocalPlayerConnected(CharacterMediator mediator)
    {
        PlayerNetworkInput.PlayerSpawned -= OnLocalPlayerConnected;
        jaggedTransition.FadeOut(jaggedDuration, () => visuals.SetActive(false));
    }
    public static void GoToTheMainMenu()
    {
        LoadScene(MainMenu, true);
    }

    public static void Disconnect()
    {
        Instance.FadeThenInvoke(
            () => NetworkManager.Singleton.Shutdown(), true
        );
    }

    public static void StartGameplay(GameMode gameMode)
    {
        DataStorage.Instance.SetInt(DataKeyInt.GameMode, (int)gameMode);
        LoadScene(Gameplay, false);
    }

    private static void LoadScene(Scene scene, bool immediate)
    {
        if (immediate)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString());
            return;
        }
        Instance.FadeThenInvoke(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString()), true
        );
    }

    private void FadeThenInvoke(Action actionAfterFade, bool showLoading)
    {
        visuals.SetActive(true);
        jaggedTransition.FadeIn(jaggedDuration, actionAfterFade);

        if (showLoading)
        {
            Tweener.Tween(this, 0f, 1f, loadingDuration, TweenStyle.quadratic,
                value => loadingCanvasGroup.alpha = value);
        }
        else
        {
            loadingCanvasGroup.alpha = 0f;
        }
        PlaySound(transitionStart, null);
    }

    public void ActionInMiddleOfAnimation(Action action)
    {
        action += () => FadeOut(false);
        FadeThenInvoke(action, false);
    }

    private void PlaySound(AudioClip clip, bool? mainMenu)
    {
        if (!mainMenu.HasValue)
        {
            mainMenu = IsMainMenu();
        }
        if (mainMenu.Value)
        {
            soundPlayer.RequestPlaySound(transform, transitionStart, false);
        }
        else
        {
            PlayerCameraSoundPlayer.Instance.PlaySound(transitionStart, false);
        }
    }

    private bool IsMainMenu() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == MainMenu.ToString();

}

public enum Scene
{
    MainMenu,
    Gameplay
}
