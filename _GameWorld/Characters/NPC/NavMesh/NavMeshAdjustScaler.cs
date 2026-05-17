using UnityEngine;
#if UNITY_EDITOR
using NavMeshPlus.Components;
using UnityEditor;
#endif
public class NavMeshAdjustScaler : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private NavMeshSurface surface;
    [ContextMenu("Rebuild NavMesh")]
    private void RebuildNavMesh()
    {
        if (surface == null)
        {
            Debug.LogError("NavMeshSurface reference is missing.");
            return;
        }
        surface.BuildNavMesh();
        Debug.Log("NavMesh rebuilt successfully.");
    }

    [ContextMenu("Scale NavigationModifiers to Sprite Size")]
    private void ScaleAllNavigationModifiers()
    {
        var modifiers = FindObjectsByType<NavigationModifier>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (modifiers.Length == 0)
        {
            Debug.Log("No NavigationModifier found in scene.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        foreach (var modifier in modifiers)
        {
            if (modifier.SpriteRenderer == null)
                continue;

            Undo.RecordObject(modifier.transform, "Scale NavigationModifier");

            modifier.ApplyScaleFromSprite();
            EditorUtility.SetDirty(modifier.transform);
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log($"Scaled {modifiers.Length} NavigationModifiers.");
    }
#endif
}