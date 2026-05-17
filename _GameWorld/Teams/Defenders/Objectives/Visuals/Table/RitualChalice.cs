using UnityEngine;

public class RitualChalice : MonoBehaviour, IActivatable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite empty, filled;
    [SerializeField] private GameObject particleSpawner;
    public void GetActivated()
    {
        spriteRenderer.sprite = filled;
        particleSpawner.SetActive(true);
    }

    public void GetDeactivated()
    {
        spriteRenderer.sprite = empty;
        particleSpawner.SetActive(false);
    }
}