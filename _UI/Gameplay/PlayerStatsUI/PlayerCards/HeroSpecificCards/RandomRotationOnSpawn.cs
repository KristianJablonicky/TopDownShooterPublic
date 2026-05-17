using UnityEngine;

public class RandomRotationOnSpawn : MonoBehaviour
{
    [SerializeField] private FloatRange rotationRange;
    private void Start()
    {
        if (rotationRange.start != rotationRange.end)
        {
            transform.rotation = QuaternionUtilities.Random(rotationRange.start, rotationRange.end);
        }
        else
        {
            transform.rotation = QuaternionUtilities.Random();
        }
    }
}
