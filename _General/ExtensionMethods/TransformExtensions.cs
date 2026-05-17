using UnityEngine;

public static class TransformExtensions
{
    public static void Lerp(this Transform transform,
        Vector2 startPos, Vector2 startScale, Quaternion startRot,
        Transform target, float progress)
    {
        transform.localPosition = Vector2.Lerp(startPos, target.localPosition, progress);
        transform.localScale = Vector2.Lerp(startScale, target.localScale, progress);
        var rot = startRot;
        rot.z = Mathf.Lerp(rot.z, target.rotation.z, progress);
        transform.rotation = rot;
    }
}
