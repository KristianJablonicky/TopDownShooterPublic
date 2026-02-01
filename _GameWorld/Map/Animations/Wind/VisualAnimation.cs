using System;
using System.Collections;
using UnityEngine;

public class VisualAnimation : MonoBehaviour
{
    [SerializeField] private AnimationSprites[] animations;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        MapAnimationPlayer.RegisterAnimation(this);
        ChangeState(false);
    }

    public bool PlayingAnimation { get; private set; } = false;
    public void PlayAnimation()
    {
        if (!PlayingAnimation) ChangeState(true);
    }
    private void StartAnimationCoroutine()
    {
        var animation = animations[UnityEngine.Random.Range(0, animations.Length)];
        StartCoroutine(PlayAnimation(animation));
    }

    public IEnumerator PlayAnimation(AnimationSprites animations)
    {
        var wait = new WaitForSeconds(animations.animationDuration / animations.sprites.Length);
        foreach (var sprite in animations.sprites)
        {
            spriteRenderer.sprite = sprite;
            yield return wait;
        }

        ChangeState(false);
    }

    private void ChangeState(bool enabled)
    {
        PlayingAnimation = enabled;
        spriteRenderer.enabled = enabled;
        if (enabled) StartAnimationCoroutine();
    }

}

[Serializable]
public class AnimationSprites
{
    public Sprite[] sprites;
    public float animationDuration;
}