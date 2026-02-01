using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour, IResettable
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;

    [Header("Animation Data")]
    [SerializeField] private AnimationData reloadAnimation;
    [SerializeField] private AnimationData abilityMovementAnimation;
    [SerializeField] private AnimationData abilityUtilityAnimation;
    [SerializeField] private AnimationDataWithDuration shootAnimation;


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
    public void PlayAnimation(Animations animation)
    {
        var animationData = GetAnimationDataWithDuration(animation);
        if (animationData != null)
        {
            if (!RequestStopCoroutine(animation)) return;
            PlayAnimation(animation, animationData, animationData.Duration);
        }
    }

    public void PlayAnimation(Animations animation, float duration)
    {
        var animationData = GetAnimationData(animation);
        if (animationData != null)
        {
            if (!RequestStopCoroutine(animation)) return;
            PlayAnimation(animation, animationData, duration);
        }
    }

    private void PlayAnimation(Animations animationType, AnimationData animationData, float duration)
    {
        currentlyRunningAnimation = StartCoroutine(AnimationCoroutine(animationData, duration));
        currentAnimationType = animationType;
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
            Animations.Shoot => shootAnimation,
            Animations.AbilityMovement => abilityMovementAnimation,
            Animations.AbilityUtility => abilityUtilityAnimation,
            _ => null
        };
    }

    private AnimationDataWithDuration GetAnimationDataWithDuration(Animations animation)
    {
        return animation switch
        {
            Animations.Shoot => shootAnimation,
            _ => null
        };
    }


    private Animations currentAnimationType = Animations.Shoot;
    /// <summary>
    /// Is the requesting animation allowed to stop the currently running animation?
    /// </summary>
    /// <param name="requestingAnimation">Animations enum entry to determine the priority of the requesting animation.</param>
    /// <returns>True if so (either no animation is playing, or an animation of lower priority is).</returns>
    private bool RequestStopCoroutine(Animations requestingAnimation)
    {
        if (currentlyRunningAnimation == null) return true;
        if ((int)currentAnimationType > (int)requestingAnimation) return false;

        StopCoroutine(currentlyRunningAnimation);
        return true;
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
        currentAnimationType = Animations.None;
    }

    public void Reset()
    {
        RequestStopCoroutine();
        SetDefaultSprites();
    }
}

public enum Animations
{
    None,
    Shoot,
    Reload,
    AbilityMovement,
    AbilityUtility
}
