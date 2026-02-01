using UnityEngine;

public class RitualChalice : MonoBehaviour, IActivatable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite empty, filled;

    public void GetActivated()
    {
        spriteRenderer.sprite = filled;
    }

    public void GetDeactivated()
    {
        spriteRenderer.sprite = empty;
    }
}