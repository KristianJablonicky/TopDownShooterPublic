using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Curse", menuName = "Abilities/Utility/Curse")]
public class Curse : UtilityAbility
{
    [field: SerializeField] public float Duration { get; private set; } = 3f;
    [field: SerializeField] public float Range { get; private set; } = 10f;
    [field: SerializeField] public float NearSightedMultiplier { get; private set; } = 0.5f;
    [field: SerializeField] public float ArcDegrees { get; private set; } = 45f;
    [field: SerializeField] public GameObject SweepGO { get; private set; }
    [field: SerializeField] public ModifierPrefab DebuffVisuals { get; private set; }

    [SerializeField] private float castAnimationTime = 0.25f;

    [Header("SFX")]
    [SerializeField] private AudioClip curseSound;
    [SerializeField] private float pitchIncreasePerCurse = 0.1f;
    [SerializeField] private float curseSoundInterval = 0.25f;
    [field: SerializeField] public AudioClip WhenCursedDebuffSound { get; private set; }

    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling) return;
        channelingManager.StartChanneling(castAnimationTime, CastEffect);
        AlsoPlayAnimation(durationMultiplier: 1.75f);
        OnCast();
    }

    private void CastEffect()
    {
        HideRangeIndicator();
        TryInvokeRPC<BabaYagaRPCs>(rpcs =>
        {
            rpcs.RequestSweepCurseRPC(owner.PlayerId);
        });
    }

    public class CurseModifier : IModifierStrategy
    {
        private readonly float nearSightedMultiplier;
        public CurseModifier(float nearSightedMultiplier)
        {
            this.nearSightedMultiplier = nearSightedMultiplier;
        }

        public ModifierType ModifierType => ModifierType.Debuff;

        public void Apply(CharacterMediator owner, Modifier modifier)
        {
            owner.AbilityManager.DisableAbilities(modifier.Duration);
            owner.VisionRange.ModifiableValue.AddOrChangeMultiplier(this, -nearSightedMultiplier);
        }

        public void Expire(CharacterMediator owner)
        {
            if (owner.IsAlive)
            {
                owner.VisionRange.ModifiableValue.RemoveMultiplier(this);
            }
        }

        public bool ExpireOnRoundEnd() => true;

        public string GetDescription() => "Nearsighted and silenced!";

        public bool RealTimeDuration() => true;
    }

    public void PlayCurseSounds(int curseCount)
    {
        owner.StartCoroutine(CurseSounds(curseCount));
    }

    private IEnumerator CurseSounds(int curseCount)
    {
        var wait = new WaitForSeconds(curseSoundInterval);
        for (int i = 0; i < curseCount; i++)
        {
            soundPlayer.RequestPlaySound(owner.GetTransform(), curseSound, i * pitchIncreasePerCurse);
            yield return wait;
        }
    }

    public override string _GetSpecificAttributes()
    {
        return $"Duration: {Duration}\nRange: {Range}\nNearsighted vision: {Mathf.RoundToInt(100f * NearSightedMultiplier)}%\nCast delay: {castAnimationTime}s";
    }
}
