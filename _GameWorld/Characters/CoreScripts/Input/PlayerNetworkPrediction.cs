using Unity.Netcode;
using UnityEngine;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    [SerializeField] private PlayerNetworkPredictionConfig config;
    private Vector2 lastBroadcastPosition, positionDiff;
    private float lastBroadcastRotation;
    private bool ignoreFirstTick = true;

    private bool stoodStill = false;
    private float tickDurationToCatchUp, tickDuration;
    private void SetUpPrediction(bool isOwner)
    {
        var networkTickSystem = NetworkManager.Singleton.NetworkTickSystem;
        tickDuration = 1f / networkTickSystem.TickRate;
        tickDurationToCatchUp = tickDuration * config.TickPortionToCatchUp;
        if (isOwner)
        {
            networkTickSystem.Tick += InformOthers;
            enabled = false;
        }
        else
        {
            enabled = true;
        }
    }
    #region Inform Others
    private void InformOthers()
    {
        if (ignoreFirstTick)
        {
            lastBroadcastPosition = mediator.GetPosition();
            ignoreFirstTick = false;
            return;
        }

        if (mediator.GetDistance(lastBroadcastPosition) > config.StandStillThreshold)
        {
            lastBroadcastPosition = mediator.GetPosition();
            stoodStill = false;
            PositionChangedRpc(lastBroadcastPosition);
        }
        else if (!stoodStill)
        {
            stoodStill = true;
            lastBroadcastPosition = mediator.GetPosition();
            StandingStillRpc(lastBroadcastPosition);
        }

        if (Mathf.Abs(mediator.RotationController.GetRotationAngle
            - lastBroadcastRotation) > config.RotationThreshold)
        {
            lastBroadcastRotation = mediator.RotationController.GetRotationAngle;
            RotationChangedRpc(lastBroadcastRotation);
        }
    }

    [Rpc(SendTo.NotMe)]
    private void PositionChangedRpc(Vector2 newPosition)
    {
        positionDiff = newPosition - lastBroadcastPosition;
        lastBroadcastPosition = newPosition;
        /*
        if (mediator.GetDistance(newPosition) > config.CorrectPositionThreshold)
        {
            //mediator.MovementController.SetPositionQuiet(newPosition);
        }
        */
        enabled = false;
        Tweener.Tween(this, mediator.GetPosition(), newPosition, tickDurationToCatchUp,
            TweenStyle.linear, mediator.MovementController.SetPositionQuiet,
            onExit: () => enabled = true);
    }

    [Rpc(SendTo.NotMe)]
    private void StandingStillRpc(Vector2 newPosition)
    {
        lastBroadcastPosition = newPosition;
        mediator.MovementController.SetPositionQuiet(newPosition);
        positionDiff = Vector2.zero;
    }

    [Rpc(SendTo.NotMe)]
    private void RotationChangedRpc(float newRotation)
    {
        var current = mediator.RotationController.GetRotationAngle;
        newRotation = current + Mathf.DeltaAngle(current, newRotation);
        Tweener.Tween(this, current, newRotation, tickDuration,
            TweenStyle.linear, mediator.RotationController.SetRotation);
        //mediator.RotationController.SetRotation(newRotation);
    }
    #endregion

    #region Get Informed
    private void Update()
    {
        if (positionDiff == Vector2.zero) return;
        //mediator.MovementController.WalkInDirection(positionDiff);
        mediator.MovementController.RigidBody.linearVelocity = positionDiff;
    }
    #endregion
}
