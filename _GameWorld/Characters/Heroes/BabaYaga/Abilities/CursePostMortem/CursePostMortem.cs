using UnityEngine;

[CreateAssetMenu(fileName = "CursePostMortem", menuName = "Abilities/PostMortem/Curse")]
public class CursePostMortem : AbilityPostMortem
{
    [field: SerializeField] public float Range { get; private set; } = 5f;
    [field: SerializeField] public GameObject curseVisuals; 
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        TryInvokeRPC<BabaYagaRPCs>(rpcs =>
        {
            rpcs.RequestCursePostMortemRPC(teamMate.PlayerId);
            OnCast();
        });
    }

    public override string _GetSpecificAttributes()
    {
        return $"Curse range: {Range}";
    }
}
