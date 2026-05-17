using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerVision : VisionMesh, IResettable
{

    [SerializeField] private float guaranteedVisionMultiplierDefender = 0.8f;
    [Header("References")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private VisionLight visionLight;
    [SerializeField] private CharacterMediator mediator;

    [SerializeField] private GameObject guaranteedTeamMateVision;
    

    [Header("Misc")]
    [SerializeField] private LayerMask teamMateMask;

    private bool localPlayerOrAlly = false;

    private Transform playerTransform;

    private bool isTeamMate = false;
    private float defaultFOV, defaultGuaranteedRange;

    public void GetEnabled(bool teamMate)
    {
        isTeamMate = teamMate;
        if (teamMate)
        {
            localPlayerOrAlly = true;
            gameObject.SetActive(true);

            gameObject.layer = Mathf.RoundToInt(Mathf.Log(teamMateMask.value, 2));
            SwitchLights(false);
            mediator.RotationController.enabled = false;
        }
        else
        {
            SwitchLights(true);
        }
        defaultFOV = frontalFov;
        defaultGuaranteedRange = guaranteedVisionRangeMultiplier;
        mediator.NewRoleAssigned += role => OnRoleAssigned(role, teamMate);

        //guaranteedTeamMateVision.SetActive(true);
        // a better solution that doesn't involve having a small circle of light around you
        mediator.AnimationController.MakeSpritesAlwaysVisible();
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
            SetVisionRange(0f, mediator.Ascendance.TimeToAscend);
        }
    }

    public void SwitchLights(bool enable) => visionLight.ChangeLightState(enable);

    protected override void VirtualStart()
    {
        visionRangeReference.ModifiableValue.CurrentValue.OnValueSet +=
            (newValue) => SetVisionRange(newValue);
        playerTransform = mediator.MovementController.transform;

        if (!localPlayerOrAlly)
        {
            localPlayerOrAlly = mediator.IsLocalPlayer;
        }
        visionRange = visionRangeReference.ModifiableValue.CurrentValue;
        visionLight.UpdateVision(visionRange, visionRange * guaranteedVisionRangeMultiplier, frontalFov);
    }
    private void OnRoleAssigned(Role newRole, bool teamMate)
    {
        if (newRole == Role.Attacker)
        {
            frontalFov = defaultFOV;
            guaranteedVisionRangeMultiplier = defaultGuaranteedRange;
            if (!teamMate)
            {
                visionLight.ChangeDistantLightState(true);
            }
        }
        else
        {
            frontalFov = 0f;
            guaranteedVisionRangeMultiplier = guaranteedVisionMultiplierDefender;
            if (!teamMate)
            {
                visionLight.ChangeDistantLightState(false);
            }
        }
        UpdateVisionLight();
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        UpdateMesh
        (
            playerTransform.position,
            mediator.RotationController.GetRotationAngle
        );

        if (isTeamMate)
        {
            //Debug.Log(mediator.RotationController.GetRotationAngle);
        }
    }

    public void Reset()
    {
        mediator.VisionRange.ModifiableValue.Reset();
    }

    private Coroutine visionChangeAnimationCoroutine;
    private void UpdateVisionLight()
        => visionLight.UpdateVision(visionRange, guaranteedVisionRangeMultiplier * visionRange);
    public void SetVisionRange(float newVisionRange, float duration = 0.25f)
    {
        if (newVisionRange == visionRange) return;

        if (visionChangeAnimationCoroutine != null)
        {
            mediator.StopCoroutine(visionChangeAnimationCoroutine);
        }
        visionChangeAnimationCoroutine = mediator.StartCoroutine
        (
            Tweener.TweenCoroutine(this, visionRange, newVisionRange, duration, TweenStyle.quadratic,
                value => {
                    visionRange = value;
                    UpdateVisionLight();
                },
                onExit: () =>
                {
                    if (newVisionRange <= 0f)
                    {
                        gameObject.SetActive(false);
                    }
                }
            )
        );
    }
}
