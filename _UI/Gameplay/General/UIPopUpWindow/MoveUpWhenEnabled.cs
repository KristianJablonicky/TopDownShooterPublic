public sealed class MoveUpWhenEnabled : AnimationWhenEnabled
{
    protected override void OnTweenValueChanged(float value)
    {
        transform.localPosition = new(0f, value);
    }
}
