using UnityEngine;

[CreateAssetMenu(fileName = "NpcDifficultyConfig", menuName = "Scriptable Objects/NpcDifficultyConfig")]
public class NpcDifficultyConfig : ScriptableObject
{
    [field: Header("Settings")]
    [field: SerializeField] public float CheckInterval { get; private set; } = 1f;
    [field: SerializeField] public float CheckIntervalAlert { get; private set; } = 0.25f;
    [field: SerializeField] public float LoseInterestDuration { get; private set; } = 1.5f;
    [field: SerializeField] public float VisionRangeBonus { get; private set; } = 0f;

    [field: Header("Difficulty")]
    [field: SerializeField] public FloatRange InaccuracyMultiplier { get; private set; } = (0.25f, 0.1f);
    [field: SerializeField] public FloatRange InaccuracyMultiplierOutOfVision { get; private set; } = (1f, 0.8f);
    [field: SerializeField] public FloatRange DelayBeforeInitialShooting { get; private set; } = (1.5f, 0.75f);
    [field: SerializeField] public float DifficultyStep { get; private set; } = 0.1f;
}
