using UnityEngine;

[CreateAssetMenu(fileName = "HulkingAbomination", menuName = "Abilities/Passive/HulkingAbomination")]
public class HulkingAbomination : PassiveAbility
{
    [SerializeField] private float healCoolDown = 5f;
    [SerializeField] private int healAmount = 5;
    [SerializeField] private bool overheals = true;
    [SerializeField, Range(0, 100)] private int startingHealthPenalty = 10;
    float timeRemaining;
    protected override void AbstractReset()
    {
        timeRemaining = healCoolDown;
        owner.HealthComponent.AdjustMaxHealth(-startingHealthPenalty, true);
        timeRemaining = healCoolDown + owner.PlayerId; // Avoid heal audio effect overlaps
    }

    protected override void SetUp() { }
    protected override void SetUpRPCsReady()
    {
        if (!owner.IsLocalPlayer) return;
        Updater.Instance.UpdatedDuringRound += MidRoundUpdate;
    }
    private void MidRoundUpdate(float dt)
    {
        timeRemaining -= dt;
        if (timeRemaining <= 0f)
        {
            owner.NetworkInput.RequestHealRpc(healAmount, overheals);
            PlaySound();
            timeRemaining += healCoolDown;
        }
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Heal cooldown: {healCoolDown}s\nHeal amount: {healAmount}\nOverheals: {overheals}\nStarting health penalty: {startingHealthPenalty}hp";
    }
}
