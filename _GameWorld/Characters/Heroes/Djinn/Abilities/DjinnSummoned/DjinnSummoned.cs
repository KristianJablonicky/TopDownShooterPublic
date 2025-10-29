using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DjinnSummoned : NetworkBehaviour
{
    [SerializeField] private DjinnVision[] visions;
    [SerializeField] private DjinnConfig config;
    [SerializeField] private ReleaseDjinn ability;

    [field: SerializeField] public NetworkObject NetworkObjectReference { get; private set; }

    private CharacterMediator owner, ally;
    private float maxDistanceDelta;
    private bool postMortem = false;

    public override void OnNetworkSpawn()
    {
        enabled = false;
        var manager = CharacterManager.Instance;
        var summoner = manager.Mediators[NetworkObjectReference.OwnerClientId];
        
        if (!summoner.IsAlive) // post mortem
        {
            foreach (var vision in visions)
            {
                vision.Init(config.VisionRadiusPostMortem);
            }
            maxDistanceDelta = config.MoveSpeedPostMortem;

            postMortem = true;
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
                StartCoroutine(FlyBack());
            }
        }


        if (IsOwner)
        {
            enabled = true;
            owner = summoner;
            if (postMortem)
            {
                ally = owner.playerData.GetTeamMate().Mediator;
            }
        }
        // Djinn summoned by an enemy
        else if (!GameStateManager.Instance.GameInProgress
                || manager.LocalPlayer != summoner.playerData.GetTeamMate())
        {
            foreach (var vision in visions)
            {
                Destroy(vision.gameObject);
            }
        }
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            owner.InputHandler.CursorPosition,
            maxDistanceDelta * Time.deltaTime
        );
        if (postMortem)
        {
            var delta = (Vector2)transform.position - ally.GetPosition();
            var distance = delta.magnitude;

            if (distance > config.MaxDistancePostMortem)
            {
                var direction = delta.normalized;
                transform.position = ally.GetPosition() + direction * config.MaxDistancePostMortem;
            }
        }
    }

    private IEnumerator FlyBack()
    {
        yield return new WaitForSeconds(ability.Duration);
        enabled = false;
        var timeElapsed = 0f;
        var startPos = transform.position;
        var duration = ability.FlyBackDuration;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, owner.GetPosition(), timeElapsed / duration);
            yield return null;
        }
    }
}

public enum Ownership
{
    LocalPlayer,
    TeamMate,
    Enemy
}