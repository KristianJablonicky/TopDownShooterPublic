using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShadowWaveEffect", menuName = "Abilities/Extras/ShadowWaveEffect")]
public class ShadowWaveEffect : ScriptableObject
{
    [field: SerializeField] public float Range { get; private set; } = 10f;
    [field: SerializeField] public float Area { get; private set; } = 4f;
    [field: SerializeField] public float ForceMax { get; private set; } = 15f;
    [field: SerializeField] public float ForceMin { get; private set; } = 10f;

    [Header("References")]
    [field: SerializeField] public GameObject WaveIndicator { get; private set; }
    [field: SerializeField] public FadeOutThenGetDestroyed PreWaveIndicator { get; private set; }
    public void Cast(Vector2 destination, DraculaRPCs rpcs)
    {
        var hitCharacters = new List<(CharacterMediator, Vector2)>();

        var manager = CharacterManager.Instance;
        foreach (var player in manager.Mediators.Values)
        {
            var distance = Vector2.Distance(player.GetPosition(), destination);
            if (distance <= Area)
            {
                var force = Mathf.Lerp(ForceMax, ForceMin, distance / Area);
                var direction = (player.GetPosition() - destination).normalized;
                hitCharacters.Add((player, force * direction));
            }
        }

        foreach (var player in hitCharacters)
        {
            rpcs.RequestApplyForceRPC(player.Item1.PlayerId, player.Item2);
        }
        rpcs.RequestShowWaveRPC(destination);
    }
}
