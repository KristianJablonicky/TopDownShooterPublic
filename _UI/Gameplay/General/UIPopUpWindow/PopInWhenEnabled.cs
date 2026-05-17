using UnityEngine;

public sealed class PopInWhenEnabled : AnimationWhenEnabled
{
    protected override void OnTweenValueChanged(float value)
    {
        gameObject.transform.localScale = Vector2.one * value;
    }
}
