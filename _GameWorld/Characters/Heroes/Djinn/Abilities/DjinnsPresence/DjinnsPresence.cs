using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DjinnsPresence", menuName = "Abilities/PostMortem/DjinnsPresence")]
public class DjinnsPresence : AbilityPostMortem
{
    [SerializeField] private float castDelay = 1.0f;
    private DjinnRPCs rpcs;
    Coroutine healCoroutine;

    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Movement;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        if (rpcs.remainingWishes > 0)
        {
            rpcs.RequestWishPostMortemRPC(teamMate.PlayerId);
            rpcs.remainingWishes.Adjust(-1);
            OnCast();
            healCoroutine = owner.StartCoroutine(CastAfterDelay());
        }
    }

    private IEnumerator CastAfterDelay()
    {
        yield return new WaitForSeconds(castDelay);
        rpcs.RequestWishRPC(owner.PlayerId);
    }

    protected override void ThirdEyeOpen()
    {
        rpcs.RequestReleaseDjinnRPC(owner.PlayerId, true);
    }
    
    protected override void ThirdEyeClosed()
    {
        if(healCoroutine != null)
        {
            owner.StopCoroutine(healCoroutine);
        }
    }
    
    protected override void SetUpRPCsReady()
    {
        TryInvokeRPC<DjinnRPCs>(rpcs => this.rpcs = rpcs);
    }
    protected override string _GetAbilitySpecificStats() => string.Empty;
}
