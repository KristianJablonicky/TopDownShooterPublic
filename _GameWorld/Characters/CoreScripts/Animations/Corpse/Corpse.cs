using UnityEngine;

public class Corpse : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public void Init(Sprite sprite, Color color)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
    }
}
