using UnityEngine;

public class BotManager : SingletonMonoBehaviour<BotManager>
{
    [SerializeField] private BotSpawner spawner;
    public void FillLobbyWithBots()
    {
        spawner.FillLobbyWithBots();
    }
}
