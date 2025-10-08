using UnityEngine;

[CreateAssetMenu(fileName = "CursePostMortem", menuName = "Abilities/PostMortem/Curse")]
public class CursePostMortem : AbilityPostMortem
{
    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        if (owner.AbilityRPCs is BabaYagaRPCs rcps)
        {
            rcps.RequestCurseRPC(owner.PlayerId);
            OnCast();
        }
        else
        {
            Debug.LogError("Wrong RPC type");
        }
    }

    protected override void ThirdEyeClosed() { }

    protected override void ThirdEyeOpen() { }
}
