using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    void Start()
    {
        var sprite = sprites[Random.Range(0, sprites.Length)];
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
