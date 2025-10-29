using UnityEngine;

[CreateAssetMenu(fileName = "DjinnConfig", menuName = "Abilities/Extras/DjinnConfig")]
public class DjinnConfig : ScriptableObject
{
    [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;
    [field: SerializeField] public float MoveSpeedPostMortem { get; private set; } = 3f;
    [field: SerializeField] public float VisionRadiusPostMortem { get; private set; } = 2f;
    [field: SerializeField] public float MaxDistancePostMortem { get; private set; } = 10f;
}
