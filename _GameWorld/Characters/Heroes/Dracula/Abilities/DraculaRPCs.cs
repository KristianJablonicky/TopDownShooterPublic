using Unity.Netcode;
using UnityEngine;

public class DraculaRPCs : AbilityRPCs
{
    [SerializeField] private ShadowWaveEffect shadowWave;
    [SerializeField] private BloodDrinker bloodDrinker;

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
}
