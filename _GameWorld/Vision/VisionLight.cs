using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VisionLight : MonoBehaviour
{
    [SerializeField] private Light2D frontalCone, aroundCharacter;
    public void ChangeLightState(bool newState)
    {
        frontalCone.enabled = newState;
        aroundCharacter.enabled = newState;
    }
    public void UpdateVision(float frontalRange, float rangeElsewhere, float frontalFov)
    {
        SetLightRange(frontalCone, frontalRange);
        SetLightRange(aroundCharacter, rangeElsewhere);

        SetLightFOV(frontalCone, frontalFov);
        SetLightFOV(aroundCharacter, 360f);
    }

    public void UpdateVision(float frontalRange, float rangeElsewhere)
    {
        SetLightRange(frontalCone, frontalRange);
        SetLightRange(aroundCharacter, rangeElsewhere);
    }

    private void SetLightRange(Light2D light, float range)
    {
        light.pointLightOuterRadius = range;
        light.pointLightInnerRadius = range - 1f;
    }

    private void SetLightFOV(Light2D light, float fov)
    {
        light.pointLightOuterAngle = fov;
        light.pointLightInnerAngle = fov;
    }
}
