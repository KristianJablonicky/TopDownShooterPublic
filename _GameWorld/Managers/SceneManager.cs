using static Scene;

public class SceneManager
{
    public static SceneManager Instance { get; private set; } = new SceneManager();
    public static void GoToTheMainMenu()
    {
        LoadScene(MainMenu);
    }

    public static void FindAMatch()
    {
        LoadScene(Gameplay);
    }

    private static void LoadScene(Scene scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.ToString());
    }
}

public enum Scene
{
    MainMenu,
    Gameplay
}
