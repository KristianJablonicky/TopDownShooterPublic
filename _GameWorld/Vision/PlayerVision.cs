using UnityEngine;

public class PlayerVision : VisionMesh, IResettable
{

    [Header("References")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private VisionLight visionLight;
    [SerializeField] private CharacterMediator mediator;

    [SerializeField] private GameObject guaranteedTeamMateVision;

    [Header("Misc")]
    [SerializeField] private LayerMask teamMateMask;

    private bool localPlayerOrAlly = false;

    private Transform playerTransform;
    private float baseVisionRange;
    public void GetEnabled(bool teamMate)
    {
        if (teamMate)
        {
            localPlayerOrAlly = true;
            gameObject.SetActive(true);

            gameObject.layer = Mathf.RoundToInt(Mathf.Log(teamMateMask.value, 2));

            SwitchLights(false);

            // TODO: reconsider
            // guaranteedTeamMateVision.SetActive(true);
        }
        else
        {
            SwitchLights(true);
        }
    }

    public void ChangeActivityIfPlayerOrAlly(bool active)
    {
        if (!localPlayerOrAlly) return;

        if (active)
        {
            Reset();
            gameObject.SetActive(active);
        }
        else
        {
            SetVisionRange(0f, Ascendance.timeToAscend);
        }
    }

    public void SwitchLights(bool enable) => visionLight.ChangeLightState(enable);

    protected override void VirtualStart()
    {
        playerTransform = mediator.MovementController.transform;

        if (!localPlayerOrAlly)
        {
            localPlayerOrAlly = mediator.IsLocalPlayer;
        }

        visionLight.UpdateVision(visionRange, visionRange * guaranteedVisionRangeMultiplier, frontalFov);

        baseVisionRange = visionRange;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        UpdateMesh
        (
            playerTransform.position,
            mediator.RotationController.GetRotationAngle
        );
    }

    public void Reset()
    {
        // reset before the Start() method executed leads to bugs
        if (baseVisionRange == 0) return;

        SetVisionRange(baseVisionRange);
    }

    public void SetVisionRangeProportional(float newVisionPercentage)
    {
        SetVisionRange(baseVisionRange * newVisionPercentage);
    }
    public void SetVisionRange(float newVisionRange, float duration = 0.25f)
    {
        if (newVisionRange == visionRange) return;
        Tweener.Tween(this, visionRange, newVisionRange, duration, TweenStyle.quadratic,
            value => {
                visionRange = value;
                visionLight.UpdateVision(value, guaranteedVisionRangeMultiplier * value);
            },
            onExit: () =>
            {
                if (newVisionRange <= 0f)
                {
                    gameObject.SetActive(false);
                }
            }
        );
    }

    public void AdjustBaseVisionRange(float newBase)
    {
        baseVisionRange += newBase;
        SetVisionRange(baseVisionRange);
    }
}
