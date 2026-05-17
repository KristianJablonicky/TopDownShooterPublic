using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShadowWaveEffect", menuName = "Abilities/Extras/ShadowWaveEffect")]
public class ShadowWaveEffect : ScriptableObject
{
    [field: SerializeField] public float Range { get; private set; } = 10f;
    [field: SerializeField] public float Area { get; private set; } = 4f;
    [field: SerializeField] public float ForceMax { get; private set; } = 15f;
    [field: SerializeField] public float ForceMin { get; private set; } = 10f;
    [field: SerializeField] public float SpookDuration { get; private set; } = 5f;

    [Header("References")]
    [field: SerializeField] public GameObject WaveIndicator { get; private set; }
    [field: SerializeField] public FadeOutThenGetDestroyed PreWaveIndicator { get; private set; }
    [field: SerializeField] public Sprite SpookIcon { get; private set; }
    [field: SerializeField] public ModifierPrefab ModifierPrefab { get; private set; }

    public void Cast(CharacterMediator caster, Vector2 destination, DraculaRPCs rpcs)
    {
        var hitCharacters = new List<(CharacterMediator, Vector2)>();

        var manager = CharacterManager.Instance;
        foreach (var player in manager.Mediators.Values)
        {
            if (!player.IsAlive) continue;

            var distance = Vector2.Distance(player.GetPosition(), destination);
            if (distance <= Area)
            {
                var force = Mathf.Lerp(ForceMax, ForceMin, distance / Area);
                var direction = (player.GetPosition() - destination).normalized;
                hitCharacters.Add((player, force * direction));
            }
        }
        var gameInProgress = GameStateManager.Instance.GameInProgress;
        ulong? id1 = null, id2 = null;
        foreach (var player in hitCharacters)
        {
            var hitPlayer = player.Item1;
            rpcs.RequestApplyForceRPC(hitPlayer.PlayerId, player.Item2);
            if (!gameInProgress)
            {
                if (!hitPlayer.IsLocalPlayer)
                {
                    rpcs.SpookHeroRpc(hitPlayer.PlayerId);
                }
            }
            else if (caster.playerData.Team != hitPlayer.playerData.Team)
            {
                if (!id1.HasValue)
                {
                    id1 = hitPlayer.PlayerId;
                }
                else
                {
                    id2 = hitPlayer.PlayerId;
                }
            }
        }

        if (id1.HasValue)
        {
            if (id2.HasValue)
            {
                rpcs.SpookHeroesRpc(id1.Value, id2.Value);
            }
            else
            {
                rpcs.SpookHeroRpc(id1.Value);
            }
        }

        rpcs.RequestShowWaveRPC(destination);
    }
}
public class ShadowWaveSpook : IModifierStrategy
{
    public ModifierType ModifierType => ModifierType.Debuff;

    public void Apply(CharacterMediator owner, Modifier modifier) { }

    public void Expire(CharacterMediator owner) { }

    public bool ExpireOnRoundEnd() => true;

    public string GetDescription() => "Spooked (and vulnerable to Dracula)";

    public bool RealTimeDuration() => true;
}
