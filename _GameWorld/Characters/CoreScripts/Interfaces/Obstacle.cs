using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    [field: Range(0f, 1f)][field: SerializeField] public float DamageMultiplier { get; private set; }
    [field: SerializeField] public bool SeenThrough { get; private set; } = false;
}
