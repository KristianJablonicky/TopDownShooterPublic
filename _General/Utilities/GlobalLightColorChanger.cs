using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLightColorChanger : MonoBehaviour
{
    [SerializeField] private Color lightColorBasement;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float intensityMultiplierBasement = 0.6f;
    private Color lightColorOutside;
    private float baseIntensity;
    void Start()
    {
        lightColorOutside = globalLight.color;
        baseIntensity = globalLight.intensity;
        PlayerNetworkInput.PlayerSpawned += player =>
        {
            player.MovementController.FloorChanged += OnFloorChanged;
            OnFloorChanged(FloorUtilities.GetCurrentFloor(player.GetPosition()));
        };
    }
    private void OnFloorChanged(Floor floor)
    {
        if (floor == Floor.Basement)
        {
            globalLight.color = lightColorBasement;
            globalLight.intensity = baseIntensity * intensityMultiplierBasement;
        }
        else
        {
            globalLight.color = lightColorOutside;
            globalLight.intensity = baseIntensity;
        }
    }
}
