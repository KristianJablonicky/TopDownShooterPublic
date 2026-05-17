using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUtilities : SingletonMonoBehaviour<NavMeshUtilities>
{
    [SerializeField] private NavMeshSurface surface;
    public static bool IsPointOnWalkableSurface(Vector2 point)
        => NavMesh.SamplePosition(point, out var hit, 1f, NavMesh.AllAreas);
}
