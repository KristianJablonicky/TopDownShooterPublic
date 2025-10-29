using System.Collections;
using System.Linq;
using UnityEngine;

public class DefenderObjective : MonoBehaviour, IActivatable
{
    [field: SerializeField] public DefenderObjectiveReward Reward { get; private set; }
    [SerializeField] private Sprite buffIcon;

    [SerializeField] private float checkAreaInterval = 0.2f;

    [SerializeField] private AudioClip beep;
    [field: SerializeField] public string Location { get; private set; }
    [SerializeField] private Transform defenderSpawnPosition;

    [Header("References")]
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private GameObject physicalObjective;
    [SerializeField] private ObjectiveArea objectiveArea;

    private Coroutine activeCoroutine;
    
    public void GetActivated()
    {
        physicalObjective.SetActive(true);
        objectiveArea.GetActivated();

        activeCoroutine = StartCoroutine(CheckArea());


        var localPlayer = CharacterManager.Instance.LocalPlayerMediator;


        if (localPlayer.role != Role.Defender) return;

        localPlayer.MovementController.SetPosition
        (
            (Vector2)defenderSpawnPosition.position
            + (Random.insideUnitCircle * 1.5f),
            null
        );
    }

    public void GetDeactivated()
    {
        physicalObjective.SetActive(false);
        objectiveArea.GetDeactivated();
        objectiveArea.Highlight(false);

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
    }

    private IEnumerator CheckArea()
    {
        var contestedTime = 0f;
        var wait = new WaitForSeconds(checkAreaInterval);

        var manager = GameStateManager.Instance;
        var attackers = manager.GetTeamDataByRole(Role.Attacker);
        var defenders = manager.GetTeamDataByRole(Role.Defender);
       
        var beepGoal = 1f;

        while (true)
        {
            if (IsPlayerOfATeamInArea(attackers))
            {
                if (!IsPlayerOfATeamInArea(defenders))
                {
                    if (contestedTime == 0f)
                    {
                        objectiveArea.Highlight(true);
                        soundPlayer.RequestPlaySound(transform, beep, false);
                    }
                    contestedTime += checkAreaInterval;
                    Debug.Log($"Area contested for {contestedTime:0.0} seconds.");

                    if (contestedTime >= beepGoal)
                    {
                        soundPlayer.RequestPlaySound(transform, beep, false);
                        beepGoal += 1f;
                    }

                    if (contestedTime >= Constants.objectiveCaptureTime)
                    {
                        Debug.Log("Objective captured by attackers!");
                        GameStateManager.Instance.ObjectiveCaptured();
                    }
                }
            }
            else if (contestedTime > 0f)
            {
                objectiveArea.Highlight(false);
                contestedTime = 0f;
                beepGoal = 1f;
            }

            yield return wait;
        }
    }

    private bool IsPlayerOfATeamInArea(TeamData team)
    {
        return team.Players.Any(p => IsPlayerInArea(p.Mediator));
    }
    private bool IsPlayerInArea(CharacterMediator mediator)
    {
        return mediator.IsAlive && 
            mediator.InRange(transform.position, objectiveArea.transform.localScale.x);
    }

    public (Sprite, DefenderObjectiveReward) GetBuff() => (buffIcon, Reward);
}
