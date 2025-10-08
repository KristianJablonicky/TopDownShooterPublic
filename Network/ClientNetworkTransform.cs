using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    /*
    [SerializeField] private float snapThreshold = 3f;

    protected override void OnNetworkTransformStateUpdated(
        ref NetworkTransformState oldState,
        ref NetworkTransformState newState)
    {
        var targetPos = GetSpaceRelativePosition(true);
        var targetRot = GetSpaceRelativeRotation(true);
        var targetScale = GetScale(true);

        // Determine current position in the same space the transform uses
        var currentPos = InLocalSpace ? transform.localPosition : transform.position;

        // Snap to position if Transform moved too far
        if (newState.IsTeleportingNextFrame
            || Vector2.Distance(currentPos, targetPos) > snapThreshold)
        {
            Teleport(targetPos, targetRot, targetScale);

            OnTransformUpdated();

            return;
        }

        // Otherwise interpolate
        base.OnNetworkTransformStateUpdated(ref oldState, ref newState);
    }
    */
}