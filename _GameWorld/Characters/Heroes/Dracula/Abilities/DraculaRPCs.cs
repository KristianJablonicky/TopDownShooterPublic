using Unity.Netcode;
using UnityEngine;

public class DraculaRPCs : AbilityRPCs
{
    [SerializeField] private ShadowWaveEffect shadowWave;
    [SerializeField] private Shadowstep shadowstep;
    [SerializeField] private BloodDrinker bloodDrinker;
    //[SerializeField] private GameObject batPrefab;
    [SerializeField] private BatSpawner batSpawner;
    [SerializeField] private AudioClip whenSpookedSound;
    private BatSpawner batSpawnerInstance;

    private const int batCount = 15;

    [Rpc(SendTo.Server)]
    public void RequestShowWaveRPC(Vector2 wavePosition)
    {
        ShowShowWaveRPC(wavePosition);
    }

    [Rpc(SendTo.Everyone)]
    private void ShowShowWaveRPC(Vector2 wavePosition)
    {
        var wave = Instantiate(shadowWave.WaveIndicator, wavePosition, Quaternion.identity);
        wave.transform.localScale = Vector2.one * shadowWave.Area;
        SpawnBats(wavePosition, shadowWave.Area);
    }

    [Rpc(SendTo.Server)]
    public void RequestShowBloodDrinkerProcRPC(ulong targetID)
    {
        ShowShowBloodDrinkerProcRPC(targetID);
    }

    [Rpc(SendTo.Everyone)]
    private void ShowShowBloodDrinkerProcRPC(ulong targetID)
    {
        bloodDrinker.ShowAnimation(targetID);
    }

    private void SpawnBats(Vector2 position, float radius)
    {
        var instance = GetOrInstantiateSpawner();
        instance.transform.position = position;
        instance.SetRadius(radius);
    }

    [Rpc(SendTo.Everyone)]
    public void SpookHeroesRpc(ulong target1, ulong target2)
    {
        SpookHero(target1);
        SpookHero(target2);
    }

    [Rpc(SendTo.Everyone)]
    public void SpookHeroRpc(ulong playerId)
    {
        SpookHero(playerId);
    }

    private void SpookHero(ulong heroId)
    {
        var character = CharacterManager.GetCharacterMediator(heroId);
        var modifier = new Modifier(character, new ShadowWaveSpook(), shadowWave.SpookDuration,
            1, shadowWave.SpookIcon);

        var modifierVisuals = Instantiate(shadowWave.ModifierPrefab, character.GetTransform());
        modifierVisuals.SetUp(character, modifier);

        if (character.IsLocalPlayer) character.SoundPlayer.RequestPlaySound(
            character.GetTransform(), whenSpookedSound, false);
    }

    [Rpc(SendTo.Everyone)]
    public void MaterializeRPC(Vector2 destination)
    {
        SpawnBats(destination, shadowstep.SpookRadius);
    }

    private BatSpawner GetOrInstantiateSpawner()
    {
        if (batSpawnerInstance == null)
        {
            batSpawnerInstance = Instantiate(batSpawner, Vector3.zero, Quaternion.identity);
        }
        return batSpawnerInstance;
    }
}
