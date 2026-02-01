using System.Collections.Generic;
using UnityEngine;

public class DamageSummaryManager : MonoBehaviour
{
    [SerializeField] private DamageSummaryUI summaryUI;

    private Dictionary<CharacterMediator, DamageRecord> DamageRecords;
    private void Start()
    {
        if (DataStorage.IsSinglePlayer)
        {
            Destroy(this);
            return;
        }

        GameStateManager.Instance.GameStarted += InitialSetUp;
    }

    private void InitialSetUp()
    {
        var characterManager = CharacterManager.Instance;
        DamageRecords = new();
        foreach (var playerData in characterManager.PlayerData)
        {
            var record = new DamageRecord(playerData);
            DamageRecords.Add(playerData.Mediator, record);
        }

        foreach (var mediator in characterManager.Mediators.Values)
        {
            SetUpPlayer(mediator.playerData);
        }

        var gameStateManager = GameStateManager.Instance;

        RoundStartWait.Instance.OnRoundStartWait += OnNewRoundStarted;
        gameStateManager.RoundEnded += OnRoundEnd;
    }

    private void SetUpPlayer(PlayerData player)
    {
        player.Mediator.HealthComponent.M1TookDamageFromM2 += OnDamageTaken;
    }

    private void OnDamageTaken(int damage, CharacterMediator victim, CharacterMediator shooter)
    {
        if (victim == shooter) return; // Ignore self-damage caused by objectives etc.

        if (victim.IsLocalPlayer)
        {
            var shooterRecord = DamageRecords[shooter];
            shooterRecord.DealtDamage(damage);
        }
        if (shooter.IsLocalPlayer)
        {
            var victimRecord = DamageRecords[victim];
            victimRecord.TookDamage(damage);
        }
    }

    private void OnNewRoundStarted(float initialDelay)
    {
        foreach (var record in DamageRecords.Values)
        {
            record.Reset();
        }

        summaryUI.HideSummary(initialDelay);
    }

    private void OnRoundEnd()
    {
        summaryUI.ShowSummary(DamageRecords);
    }
}

public class DamageRecord : IResettable
{
    public DamageRecord(PlayerData ownerData)
    {
        owner = ownerData;
        Reset();
    }

    public PlayerData owner;
    public int DamageTaken, DamageTakenHits;
    public int DamageDealt, DamageDealtHits;

    public void Reset()
    {
        DamageTaken = 0;
        DamageTakenHits = 0;
        DamageDealt = 0;
        DamageDealtHits = 0;
    }
    public void TookDamage(int damage)
    {
        DamageTaken += damage;
        DamageTakenHits++;
    }
    public void DealtDamage(int damage)
    {
        DamageDealt += damage;
        DamageDealtHits++;
    }
}
