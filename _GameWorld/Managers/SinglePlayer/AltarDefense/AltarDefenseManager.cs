using UnityEngine;

public class AltarDefenseManager : MonoBehaviour
{
    [SerializeField] private AD_NpcSpawner npcSpawner;

    public void StartDefense()
    {
        var player = CharacterManager.Instance.LocalPlayerMediator;
        player.MovementController.SetPosition(Vector2.zero, Floor.Basement);
        npcSpawner.StartSpawning();
    }
}
