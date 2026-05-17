using UnityEngine;

public class VisionRange : MonoBehaviour
{
    [SerializeField] protected float baseVisionRange = 8f;
    public ModifiableFloat ModifiableValue { get; private set; }
    private void Awake()
    {
        ModifiableValue = new(baseVisionRange);
    }
}
