using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour, IResettable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;

    [Header("Animation Data")]
    [SerializeField] private AnimationData reloadAnimation;
    [SerializeField] private AnimationData abilityMovementAnimation;
    [SerializeField] private AnimationData abilityUtilityAnimation;


    [Header("Death visuals")]
    [SerializeField] private Sprite corpseSprite;
    [SerializeField] private Corpse corpsePrefab;

    private Coroutine currentlyRunningAnimation;
    private Sprite defaultSprite, defaultOutlineSprite;

    private void Start()
    {
        defaultSprite = spriteRenderer.sprite;
        defaultOutlineSprite = outlineSpriteRenderer.sprite;
    }

    public Corpse DeathAnimation(Color color)
    {
        var corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        corpse.Init(corpseSprite, color);
        return corpse;
    }

    public void PlayAnimation(Animations animation, float duration)
    {
        var animationData = GetAnimationData(animation);
        if (animationData != null)
        {
            RequestStopCoroutine();
            currentlyRunningAnimation = StartCoroutine(AnimationCoroutine(animationData, duration));
        }
    }

    private IEnumerator AnimationCoroutine(AnimationData animation, float duration)
    {
        var frames = animation.FrameCount;
        var wait = new WaitForSeconds(duration / frames);
        for (int i = 0; i < frames; i++)
        {
            (spriteRenderer.sprite, outlineSpriteRenderer.sprite) = animation.GetFrames(i);
            yield return wait;
        }
        SetDefaultSprites();
    }

    private AnimationData GetAnimationData(Animations animation)
    {
        return animation switch
        {
            Animations.Reload => reloadAnimation,
            Animations.AbilityMovement => abilityMovementAnimation,
            Animations.AbilityUtility => abilityUtilityAnimation,
            _ => null
        };
    }

    private void RequestStopCoroutine()
    {
        if (currentlyRunningAnimation == null) return;
        StopCoroutine(currentlyRunningAnimation);
    }
    private void SetDefaultSprites()
    {
        spriteRenderer.sprite = defaultSprite;
        outlineSpriteRenderer.sprite = defaultOutlineSprite;
    }

    public void Reset()
    {
        RequestStopCoroutine();
        SetDefaultSprites();
    }
}

public enum Animations
{
    Reload,
    AbilityMovement,
    AbilityUtility
}
