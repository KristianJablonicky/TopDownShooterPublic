using UnityEngine;

public abstract class AnimationWhenEnabled : MonoBehaviour
{
    [field: SerializeField] public float AnimationDuration = 0.25f;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.quadratic;
    [SerializeField] private bool ignoreTheFirstTween = true;
    [SerializeField] protected float minValue = 0f, maxValue = 1f;

    private void OnEnable()
    {
        Tween(minValue, maxValue);
    }
    
    public void PlayAnimationReversed()
    {
        Tween(maxValue, minValue);
    }
    private void Tween(float min, float max)
    {
        if (!gameObject.activeSelf) return;
        if (ignoreTheFirstTween)
        {
            ignoreTheFirstTween = false;
            return;
        }
        Tweener.Tween(this, min, max, AnimationDuration, tweenStyle,
            OnTweenValueChanged);
    }

    protected abstract void OnTweenValueChanged(float value);
}
