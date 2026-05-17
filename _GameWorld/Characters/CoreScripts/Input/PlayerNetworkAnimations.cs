using Unity.Netcode;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void RequestAnimationRpc(Animations animation, float duration, int specialIndex = -1)
    {

        var target = RpcTarget.Not(mediator.PlayerId, RpcTargetUse.Temp);
        var rpcParams = new RpcParams() { Send = target };

        if (specialIndex == -1)
        {
            ClientAnimationRpc(animation, duration, rpcParams);
        }
        else
        {
            ClientAnimationRpc(animation, duration, specialIndex, rpcParams);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void ClientAnimationRpc(Animations animation, float duration, int specialIndex, RpcParams rpcParams = default)
    {
        mediator.AnimationController.PlayAnimationFromExtras(animation, specialIndex, duration);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void ClientAnimationRpc(Animations animation, float duration, RpcParams rpcParams = default)
    {
        mediator.AnimationController.PlayAnimation(animation, duration);
    }
}
