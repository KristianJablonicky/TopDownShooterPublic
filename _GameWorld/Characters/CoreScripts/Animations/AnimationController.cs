using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Sprite corpseSprite;
    [SerializeField] private Corpse corpsePrefab;
    public Corpse DeathAnimation(Color color)
    {
        var corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        corpse.Init(corpseSprite, color);
        return corpse;
    }
}
