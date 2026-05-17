using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ReleaseDjinn", menuName = "Abilities/Utility/ReleaseDjinn")]
public class ReleaseDjinn : UtilityAbility
{
    [field: SerializeField] public float VisionRadiusStart { get; private set; } = 1.5f;
    [field: SerializeField] public float VisionRadiusEnd { get; private set; } = 3f;
    [field: SerializeField] public float Duration { get; private set; } = 3f;
    [field: SerializeField] public float FlyBackDuration { get; private set; } = 0.5f;
    [field: SerializeField] public float MovementSlow { get; private set; } = 0.75f;

    [SerializeField] private float rubInterval = 0.5f;
    [SerializeField] private AudioClip[] rubbingClips;
    private Coroutine coroutine;

    protected override void OnKeyDown(Vector2 position) { }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling) return;

        channelingManager.StartChannelingSlowedDown(Duration, null, owner, MovementSlow, false);
        AlsoPlayAnimation();

        TryInvokeRPC<DjinnRPCs>(rpcs =>
        {
            rpcs.RequestReleaseDjinnRPC(owner.PlayerId, false);
        });
        OnCast();
    }

    public void StartRubbing()
    {
        coroutine = owner.StartCoroutine(RubCoroutine());
    }
    private IEnumerator RubCoroutine()
    {
        var wait = new WaitForSeconds(rubInterval);
        for (int i = 0; i < (int)(Duration / rubInterval); i++)
        {
            soundPlayer.RequestPlaySound(owner.GetTransform(), rubbingClips, true);
            yield return wait;
        }
    }

    public void StopRubbing()
    {
        if (coroutine != null)
        {
            owner.StopCoroutine(coroutine);
        }
    }

    public override string _GetSpecificAttributes()
    {
        return $"Duration: {Duration}s\nFly back duration: {FlyBackDuration}s\nVision radius: {VisionRadiusStart} - {VisionRadiusEnd}\nMovement slow while channeling: {MovementSlow}%";
    }
}
