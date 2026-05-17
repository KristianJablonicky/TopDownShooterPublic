using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNetworkPredictionConfig", menuName = "Netcode/PlayerNetworkPredictionConfig")]
public class PlayerNetworkPredictionConfig : ScriptableObject
{
    [field: SerializeField] public float StandStillThreshold { get; private set; } = 0.01f;
    [field: SerializeField] public float CorrectPositionThreshold { get; private set; } = 0.05f;
    [field: SerializeField] public float RotationThreshold { get; private set; } = 10f;
    [field: SerializeField, Range(0f, 1f)] public float TickPortionToCatchUp { get; private set; } = 0.25f;

}
