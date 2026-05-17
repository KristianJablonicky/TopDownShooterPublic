using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DjinnSummoned : NetworkBehaviour
{
    [SerializeField] private DjinnVision[] visions;
    [SerializeField] private DjinnConfig config;
    [SerializeField] private ReleaseDjinn ability;
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private GameObject trail;

    [field: SerializeField] public NetworkObject NetworkObjectReference { get; private set; }
    private Action updateAction;

    private CharacterMediator owner, ally;
    private float maxDistanceDelta;
    private bool postMortem = false;

    public override void OnNetworkSpawn()
    {
        enabled = false;
        var isMultiPlayer = !DataStorage.IsSinglePlayer;
        var manager = CharacterManager.Instance;

        var summoner = manager.Mediators[NetworkObjectReference.OwnerClientId];

        var gameStateManager = GameStateManager.Instance;

        if (!summoner.IsAlive && isMultiPlayer) // post mortem
        {
            foreach (var vision in visions)
            {
                vision.Init(config.VisionRadiusPostMortem);
            }
            maxDistanceDelta = config.MoveSpeedPostMortem;
            postMortem = true;
            Destroy(trail);
        }
        else // Release Djinn
        {
            foreach (var vision in visions)
            {
                vision.Init(ability.Duration, ability.VisionRadiusStart, ability.VisionRadiusEnd);
            }
            maxDistanceDelta = config.MoveSpeed;
            if (IsOwner)
            {
                StartCoroutine(FlyBackAfterDelay());
            }
        }

        if (IsOwner)
        {
            owner = summoner;

            // one frame of NPC's djinn being controlled by the player
            updateAction = UpdateActionPlayer;
            enabled = true;
            owner.MovementController.FloorChanged += OwnerChangedFloor;

            if (postMortem && owner.playerData is not null)
            {
                ally = owner.playerData.GetTeamMate().Mediator;
            }
        }
        // Djinn summoned by an enemy
        else if (!gameStateManager.GameInProgress
                || manager.LocalPlayer != summoner.playerData.GetTeamMate())
        {
            ConditionallyDisableVision(summoner);
        }
    }

    private void OwnerChangedFloor(Floor newFloor)
    {
        FloorUtilities.ApplyYOffset(transform, newFloor);
        if (trail != null) Destroy(trail); // hide 75f trail glitch
    }

    private ulong? npcOwnerId = null;
    public void SetUp(ulong actualOwnerId)
    {
        owner.MovementController.FloorChanged -= OwnerChangedFloor;
        npcOwnerId = actualOwnerId;
        owner = CharacterManager.Instance.Mediators[actualOwnerId];
        team = owner.AiDecisions.Team;
        updateAction = UpdateActionNPC;
        ConditionallyDisableVision(owner);
        enabled = true;

        var gameStateManager = GameStateManager.Instance;
        if (gameStateManager.GameInProgress)
        {
            gameStateManager.RoundEnded += RequestDespawnServerRpc;
            gameStateManager.NewRoundStarted += RequestDespawnServerRpc;
        }
        owner.MovementController.FloorChanged -= OwnerChangedFloor;
    }

    private void ConditionallyDisableVision(CharacterMediator summoner)
    {
        if (!GameStateManager.Instance.GameInProgress
        || CharacterManager.Instance.LocalPlayer != summoner.playerData.GetTeamMate())
        {
            foreach (var vision in visions)
            {
                Destroy(vision.gameObject);
            }
        }
    }

    private void Update()
    {
        updateAction();
    }

    private void UpdateActionPlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            PlayerInputHandler.CursorPosition,
            maxDistanceDelta * Time.deltaTime
        );
        if (postMortem)
        {
            var delta = GetDelta();
            var distance = delta.magnitude;

            if (distance > 2f * config.MaxDistancePostMortem) // most likely switched a floor
            {
                SwitchFloor();
                // update delta and distance after switching floor
                delta = GetDelta();
                distance = delta.magnitude;
            }
            if (distance > config.MaxDistancePostMortem)
            {
                var direction = delta.normalized;
                transform.position = ally.GetPosition() + direction * config.MaxDistancePostMortem;
            }
        }
        else
        {
            if (Input.GetKeyUp((KeyCode)ability.KeyCode))
            {
                owner.Gun.ChannelingManager.RequestInterrupt(true);
                owner.AnimationController.Reset();
                StartCoroutine(FlyBack());
                FlyBackRpc();
            }
        }
    }

    private const float refreshTarget = 1f;
    private float currentRefresh = 0f;
    private Vector2 enemyPosition;
    private BotTeam team;
    private void UpdateActionNPC()
    {
        if (currentRefresh <= 0f)
        {
            currentRefresh += refreshTarget;
            enemyPosition = AiAbilityCaster.GetCastPositionStatic(team);
        }
        currentRefresh -= Time.deltaTime;

        transform.position = Vector2.MoveTowards(
            transform.position,
            enemyPosition,
            maxDistanceDelta * Time.deltaTime
        );
    }
    private Vector2 GetDelta() => (Vector2)transform.position - ally.GetPosition();

    private void SwitchFloor()
    {
        var diff = ally.GetPosition().y - transform.position.y;
        var targetTransform = transform.position +
            (Vector3.up *
            (Mathf.Sign(diff) * Constants.floorYOffset));
        transform.position = targetTransform;
        networkTransform.Teleport(targetTransform, transform.rotation, transform.localScale);
    }

    private IEnumerator FlyBackAfterDelay()
    {
        yield return new WaitForSeconds(ability.Duration);
        StartCoroutine(FlyBack());
    }
    private IEnumerator FlyBack()
    {
        if (!enabled) yield break;
        enabled = false;
        owner.Gun.ChannelingManager.StartChannelingSlowedDown(
            ability.FlyBackDuration, null, owner, ability.MovementSlow, true);

        var timeElapsed = 0f;
        var startPos = transform.position;
        var duration = ability.FlyBackDuration;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, owner.GetPosition(), timeElapsed / duration);
            yield return null;
        }
        Despawn();
    }

    private void Despawn()
    {
        CharacterMediator mediator = null;
        if (npcOwnerId.HasValue)
        {
            mediator = CharacterManager.Instance.Mediators[npcOwnerId.Value];
        }
        else if (owner.IsLocalPlayer)
        {
            mediator = owner;
        }
        if (mediator != null)
        {
            RequestDespawnServerRpc();
        }
    }

    [ServerRpc]
    private void RequestDespawnServerRpc()
    {
        if (NetworkObject == null) return;
        Unsubscribe();
        NetworkObject.Despawn(true);
    }
    public override void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        var manager = GameStateManager.Instance;
        manager.NewRoundStarted -= RequestDespawnServerRpc;
        manager.RoundEnded -= RequestDespawnServerRpc;
        if (owner != null)
        {
            owner.MovementController.FloorChanged -= OwnerChangedFloor;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void FlyBackRpc()
    {
        var summoner = CharacterManager.GetCharacterMediator(NetworkObjectReference.OwnerClientId);
        if (summoner.AbilityManager.UtilityAbility is ReleaseDjinn ability)
        {
            ability.StopRubbing();
        }
        summoner.Gun.ChannelingManager.RequestInterrupt(true);
        summoner.AnimationController.Reset();
    }
}
