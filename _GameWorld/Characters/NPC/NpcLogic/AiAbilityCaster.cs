using System.Collections;
using UnityEngine;

public class AiAbilityCaster : PeriodicallyInvoked, IAiComponent
{
    [SerializeField] private float maxRandomDelay = 3f;
    [SerializeField] private AbilityType type;

    [Header("Settings")]
    [SerializeField] private bool mustBeOnTheSameFloor = true;
    [SerializeField] private CastOpportunityType castOpportunityType = CastOpportunityType.Whenever;

    private enum CastOpportunityType
    {
        Whenever,
        OutOfCombat,
        DuringCombat
    }

    private AiDecisions decisions;
    private AbilityManager abilityManager;
    private ActiveAbility ability;
    bool abilityBeingCast = false;
    private AiCommunicationManager communicationManager;
    public void SetDecisionsReference(AiDecisions decisions)
    {
        this.decisions = decisions;
        abilityManager = decisions.Mediator.AbilityManager;
        ability = (ActiveAbility)abilityManager.GetAbility(type);
        communicationManager = AiCommunicationManager.Instance;
    }

    public override void Invoke()
    {
        if (decisions.CurrentGoal == AiGoal.RemainIdle) return;
        if (!ability.ReadyToCast) return;
        if (abilityBeingCast) return;
        if (!IsEnemyOnTheSameFloor()) return;
        if (!RightOpportunity()) return;
        StartCoroutine(CastAbilityAfterDelay(ability));
    }

    private bool IsEnemyOnTheSameFloor()
    {
        if (!mustBeOnTheSameFloor) return true;
        var pos = communicationManager.EnemyLastSeenPosition(decisions.Team);
        if (!pos.HasValue) return false; // we need to be on the same floor but can't see the enemy

        return FloorUtilities.IsOnTheSameFloor(
            decisions.Mediator.GetPosition(),
            pos.Value);
    }

    private bool RightOpportunity()
    {
        var alert = decisions.ShootingHandler.IsAlert;
        if (castOpportunityType == CastOpportunityType.Whenever) return true;
        else if (castOpportunityType == CastOpportunityType.OutOfCombat)
        {
            return !alert;
        }
        else if (castOpportunityType == CastOpportunityType.DuringCombat)
        {
            return alert;
        }
        Debug.LogWarning("Opportunity type not handled!");
        return false;
    }

    private IEnumerator CastAbilityAfterDelay(ActiveAbility ability)
    {
        abilityBeingCast = true;
        yield return new WaitForSeconds(Random.Range(0f, maxRandomDelay));
        abilityBeingCast = false;

        if (IsEnemyOnTheSameFloor())
        {
            var castPosition = GetCastPosition();
            ability.OnKeyInteraction(true, castPosition);
            ability.OnKeyInteraction(false, castPosition);
        }
    }
    public static Vector2 GetCastPositionStatic(BotTeam team) =>
        GetCastPositionInternal(
            AiCommunicationManager.Instance.EnemyLastSeenPosition(team));
    private Vector2 GetCastPosition()
        => GetCastPositionInternal(EnemyPos);
    private static Vector2 GetCastPositionInternal(Vector2? enemyPos)
    {
        Vector2 castPosition;
        if (enemyPos.HasValue)
        {
            castPosition = enemyPos.Value;
        }
        else
        {
            castPosition = ImportantPositions.Instance.AltarPosition;
        }
        castPosition += Random.insideUnitCircle;
        return castPosition;
    }

    private Vector2? EnemyPos => communicationManager.EnemyLastSeenPosition(decisions.Team);
}