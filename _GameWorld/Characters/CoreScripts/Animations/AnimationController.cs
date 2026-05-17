using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour, IResettable
{
    [Header("References")]
    [SerializeField] private CharacterMediator owner;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;

    [Header("Animation Data")]
    [SerializeField] private AnimationData reloadAnimation;
    [SerializeField] private AnimationData abilityMovementAnimation;
    [SerializeField] private AnimationData abilityUtilityAnimation;
    [SerializeField] private AnimationDataWithDuration shootAnimation;
    [SerializeField] private AnimationData[] extraAnimations;


    [Header("Death visuals")]
    [SerializeField] private Sprite corpseSprite;
    [SerializeField] private Corpse corpsePrefab;

    [Header("Always visible settings")]
    [SerializeField] private SpriteRenderer legs;
    [SerializeField] private SpriteRenderer thirdEye;
    [SerializeField] private LayerMask alwaysVisibleLayer;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingLayerOrder = 10;

    private Coroutine currentlyRunningAnimation;
    private Sprite defaultSprite, defaultOutlineSprite;

    private void Start()
    {
        defaultSprite = spriteRenderer.sprite;
        defaultOutlineSprite = outlineSpriteRenderer.sprite;
    }
    
    public void MakeSpritesAlwaysVisible()
    {
        SetUpSpriteRenderer(spriteRenderer, 1);
        SetUpSpriteRenderer(outlineSpriteRenderer, 0);
        SetUpSpriteRenderer(legs, -1);
        SetUpSpriteRenderer(thirdEye, 2);
    }

    public void SetUpSpriteRenderer(SpriteRenderer sr, int extraOrderSetting)
    {
        sr.gameObject.layer = Mathf.RoundToInt(Mathf.Log(alwaysVisibleLayer.value, 2)); ;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingLayerOrder + extraOrderSetting;
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
    public void PlayAnimationFromExtras(Animations animation, int index, float duration)
    {
        if (extraAnimations.Length > index)
        {
            PlayAnimationSafe(animation, extraAnimations[index], duration);
        }
        else
        {
            Debug.LogWarning($"Trying to play extra {animation} at nonexistent index {index}!");
        }
    }
    public void PlayAnimation(Animations animation, float duration)
    {
        var animationData = GetAnimationData(animation);
        PlayAnimationSafe(animation, animationData, duration);
    }
    private void PlayAnimationSafe(Animations animation, AnimationData animationData, float duration)
    {

        if (animationData != null)
        {
            if (!RequestStopCoroutine(animation)) return;
            PlayAnimation(animation, animationData, duration);
        }
    }
    
    private void PlayAnimation(Animations animationType, AnimationData animationData, float duration)
    {
        currentAnimationType = animationType;
        currentlyRunningAnimation = StartCoroutine(AnimationCoroutine(animationData, duration));
    }

    private IEnumerator AnimationCoroutine(AnimationData animation, float duration)
    {
        if (animation.audioClips.Length > 0)
        {
            SoundPlayer soundPlayer;
            if ((int)currentAnimationType >= 3)
            {
                soundPlayer = owner.AbilityManager.SoundPlayer;
            }
            else
            {
                soundPlayer = owner.SoundPlayer;
            }
            soundPlayer.RequestPlaySound(transform, animation.audioClips, false);
        }

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
        SetLetVisibility(true);
    }

    public void SetLetVisibility(bool visible)
    {
        var color = legs.color;
        color.a = visible ? 1f : 0f;
        legs.color = color;
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
