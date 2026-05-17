using UnityEngine;

public class NavigationModifier : MonoBehaviour
{
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    public void ApplyScaleFromSprite()
    {
        if (SpriteRenderer == null || SpriteRenderer.sprite == null)
        {
            Debug.LogWarning($"{this} missing sprite renderer!");
            return;
        }

        var size = SpriteRenderer.size;
        transform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
